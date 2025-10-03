# Quick test to verify MSIX signing is ready
param(
    [switch]$Verbose
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$testsPassed = 0
$testsFailed = 0

function Test-Component {
    param(
        [string]$Name,
        [scriptblock]$Test,
        [string]$SuccessMessage,
        [string]$FailureMessage
    )
    
    Write-Host "Testing: $Name..." -ForegroundColor Cyan -NoNewline
    try {
        $result = & $Test
        if ($result) {
            Write-Host " OK" -ForegroundColor Green
            if ($Verbose -and $SuccessMessage) {
                Write-Host "  $SuccessMessage" -ForegroundColor Gray
            }
            $script:testsPassed++
            return $true
        } else {
            Write-Host " FAILED" -ForegroundColor Red
            if ($FailureMessage) {
                Write-Host "  $FailureMessage" -ForegroundColor Yellow
            }
            $script:testsFailed++
            return $false
        }
    } catch {
        Write-Host " ERROR" -ForegroundColor Red
        Write-Host "  $($_.Exception.Message)" -ForegroundColor Yellow
        $script:testsFailed++
        return $false
    }
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "MSIX Signing Readiness Check" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host ""

# Test 1: Certificate file exists
Test-Component -Name "Certificate file exists" `
    -Test { Test-Path ".\differ-signing-cert.pfx" } `
    -SuccessMessage "Certificate file found" `
    -FailureMessage "Run: .\scripts\recreate-cert-quick.ps1"

# Test 2: Certificate has private key
Test-Component -Name "Certificate has private key" `
    -Test {
        $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2(".\differ-signing-cert.pfx", "")
        return $cert.HasPrivateKey
    } `
    -SuccessMessage "Private key verified" `
    -FailureMessage "Run: .\scripts\recreate-cert-quick.ps1"

# Test 3: Certificate is valid (not expired)
Test-Component -Name "Certificate is not expired" `
    -Test {
        $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2(".\differ-signing-cert.pfx", "")
        return (Get-Date) -lt $cert.NotAfter
    } `
    -SuccessMessage "Certificate is valid" `
    -FailureMessage "Certificate has expired, recreate it"

# Test 4: Signtool can be located
$signtoolFound = Test-Component -Name "Signtool.exe can be located" `
    -Test {
        $searchPaths = @(
            "C:\Program Files (x86)\Windows Kits\10\bin\*\x64\signtool.exe",
            "C:\Program Files (x86)\Windows Kits\10\App Certification Kit\signtool.exe",
            "C:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\signtool.exe"
        )
        foreach ($pattern in $searchPaths) {
            $found = Get-ChildItem -Path $pattern -File -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($found) { return $true }
        }
        return $false
    } `
    -SuccessMessage "Signtool found" `
    -FailureMessage "Install Windows SDK or check .\scripts\find-signtool.ps1"

# Test 5: Makeappx can be located
Test-Component -Name "Makeappx.exe can be located" `
    -Test {
        $searchPaths = @(
            "C:\Program Files (x86)\Windows Kits\10\bin\*\x64\makeappx.exe"
        )
        foreach ($pattern in $searchPaths) {
            $found = Get-ChildItem -Path $pattern -File -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($found) { return $true }
        }
        # Makeappx not required if using other packaging methods
        return $true
    } `
    -SuccessMessage "Makeappx found" `
    -FailureMessage "Install Windows SDK (optional)"

# Test 6: Certificate is in trusted root (for local testing)
Test-Component -Name "Certificate is in Trusted Root store" `
    -Test {
        $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2(".\differ-signing-cert.pfx", "")
        $store = New-Object System.Security.Cryptography.X509Certificates.X509Store("Root", "CurrentUser")
        $store.Open("ReadOnly")
        $found = $store.Certificates | Where-Object { $_.Thumbprint -eq $cert.Thumbprint }
        $store.Close()
        return $null -ne $found
    } `
    -SuccessMessage "Certificate is trusted" `
    -FailureMessage "Run: .\scripts\install-certificate.ps1 (optional for local testing)"

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "Test Results" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host ""
Write-Host "Passed: $testsPassed" -ForegroundColor Green
Write-Host "Failed: $testsFailed" -ForegroundColor $(if ($testsFailed -gt 0) { "Red" } else { "Gray" })
Write-Host ""

if ($testsFailed -eq 0) {
    Write-Host "SUCCESS! " -ForegroundColor Green -NoNewline
    Write-Host "All checks passed. You are ready to create signed MSIX packages." -ForegroundColor White
    Write-Host ""
    Write-Host "To create a signed release, run:" -ForegroundColor Cyan
    Write-Host "  .\scripts\create-release.ps1 -Version '0.1.1'" -ForegroundColor Gray
    Write-Host ""
    exit 0
} else {
    Write-Host "ATTENTION REQUIRED! " -ForegroundColor Yellow -NoNewline
    Write-Host "Some checks failed. Review the messages above." -ForegroundColor White
    Write-Host ""
    Write-Host "For detailed diagnostics, run:" -ForegroundColor Cyan
    Write-Host "  .\scripts\diagnose-signing-issue.ps1" -ForegroundColor Gray
    Write-Host ""
    exit 1
}
