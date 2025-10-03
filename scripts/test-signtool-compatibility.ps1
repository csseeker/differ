# Test if the certificate and signtool are compatible
param(
    [string]$CertPath = ".\differ-signing-cert.pfx",
    [string]$TestFilePath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Continue'

Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "Certificate and Signtool Compatibility Test" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host ""

# Find signtool
$searchPaths = @(
    "C:\Program Files (x86)\Windows Kits\10\bin\*\x64\signtool.exe",
    "C:\Program Files (x86)\Windows Kits\10\App Certification Kit\signtool.exe",
    "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\*\x64\signtool.exe",
    "C:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\signtool.exe"
)

$allSigntools = @()
foreach ($pattern in $searchPaths) {
    $found = Get-ChildItem -Path $pattern -File -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending
    if ($found) {
        $allSigntools += $found
    }
}

if ($allSigntools.Count -eq 0) {
    Write-Host "[ERROR] No signtool found" -ForegroundColor Red
    exit 1
}

Write-Host "Found $($allSigntools.Count) signtool.exe version(s):" -ForegroundColor Green
Write-Host ""

foreach ($tool in $allSigntools) {
    $version = $tool.VersionInfo.FileVersion
    Write-Host "  - $($tool.FullName)" -ForegroundColor Cyan
    Write-Host "    Version: $version" -ForegroundColor Gray
    
    # Flag old versions
    if ($version -like "10.0.10*") {
        Write-Host "    [WARNING] This is an OLD version from 2015-2016" -ForegroundColor Yellow
        Write-Host "              May not support modern certificate formats" -ForegroundColor Yellow
    }
}
Write-Host ""

# Load certificate
Write-Host "Loading certificate..." -ForegroundColor Cyan
try {
    $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($CertPath, "", [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable)
    Write-Host "[OK] Certificate loaded" -ForegroundColor Green
    Write-Host "    Subject: $($cert.Subject)" -ForegroundColor Gray
    Write-Host "    Thumbprint: $($cert.Thumbprint)" -ForegroundColor Gray
    Write-Host "    Has Private Key: $($cert.HasPrivateKey)" -ForegroundColor Gray
    Write-Host "    Signature Algorithm: $($cert.SignatureAlgorithm.FriendlyName)" -ForegroundColor Gray
    Write-Host "    Key Algorithm: $($cert.PublicKey.Oid.FriendlyName)" -ForegroundColor Gray
    Write-Host ""
    
    if (-not $cert.HasPrivateKey) {
        Write-Host "[ERROR] Certificate does not have private key!" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "[ERROR] Failed to load certificate: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Create a test file
if ([string]::IsNullOrWhiteSpace($TestFilePath)) {
    $TestFilePath = Join-Path $env:TEMP "test-signing-$(Get-Random).exe"
    "MZ" | Out-File -FilePath $TestFilePath -Encoding ASCII -NoNewline
    Write-Host "Created test file: $TestFilePath" -ForegroundColor Gray
    Write-Host ""
}

# Test each signtool
$workingSigntool = $null
foreach ($signtool in $allSigntools) {
    Write-Host "Testing: $($signtool.FullName)" -ForegroundColor Cyan
    Write-Host "  Version: $($signtool.VersionInfo.FileVersion)" -ForegroundColor Gray
    
    # Try different signing approaches
    $approaches = @(
        @{
            Name = "Basic sign with SHA256"
            Args = @('sign', '/f', $CertPath, '/fd', 'SHA256', '/v', $TestFilePath)
        },
        @{
            Name = "Sign without digest algorithm"
            Args = @('sign', '/f', $CertPath, '/v', $TestFilePath)
        },
        @{
            Name = "Sign with SHA1"
            Args = @('sign', '/f', $CertPath, '/fd', 'SHA1', '/v', $TestFilePath)
        }
    )
    
    $success = $false
    foreach ($approach in $approaches) {
        Write-Host "    Trying: $($approach.Name)..." -ForegroundColor Gray -NoNewline
        
        $output = & $signtool.FullName @($approach.Args) 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host " SUCCESS!" -ForegroundColor Green
            $workingSigntool = $signtool
            $success = $true
            Write-Host ""
            Write-Host "  [OK] This signtool works with your certificate!" -ForegroundColor Green
            Write-Host "  Command that worked:" -ForegroundColor Cyan
            Write-Host "    $($signtool.FullName) $($approach.Args -join ' ')" -ForegroundColor Gray
            break
        } else {
            Write-Host " Failed" -ForegroundColor Red
            Write-Host "      Exit code: $LASTEXITCODE" -ForegroundColor DarkGray
            if ($output -match "error|failed") {
                $errorLine = ($output | Select-String "error|failed" | Select-Object -First 1).ToString()
                Write-Host "      Error: $errorLine" -ForegroundColor DarkGray
            }
        }
    }
    
    if ($success) {
        break
    }
    Write-Host ""
}

# Cleanup
if (Test-Path $TestFilePath) {
    Remove-Item $TestFilePath -Force -ErrorAction SilentlyContinue
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "Results" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host ""

if ($workingSigntool) {
    Write-Host "SUCCESS! Found a working configuration:" -ForegroundColor Green
    Write-Host "  Signtool: $($workingSigntool.FullName)" -ForegroundColor White
    Write-Host ""
    Write-Host "RECOMMENDATION:" -ForegroundColor Yellow
    Write-Host "  Update create-msix.ps1 to prefer this signtool version" -ForegroundColor White
    Write-Host "  Or use: -SigntoolPath '$($workingSigntool.FullName)'" -ForegroundColor Gray
} else {
    Write-Host "FAILED! None of the signtool versions work with your certificate." -ForegroundColor Red
    Write-Host ""
    Write-Host "POSSIBLE SOLUTIONS:" -ForegroundColor Yellow
    Write-Host "  1. Install latest Windows 10 SDK:" -ForegroundColor White
    Write-Host "     https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  2. Try recreating certificate with SHA1 instead of SHA256:" -ForegroundColor White
    Write-Host "     (Less secure but compatible with older signtool)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  3. Use PowerShell Set-AuthenticodeSignature instead:" -ForegroundColor White
    Write-Host "     (Alternative to signtool.exe)" -ForegroundColor Gray
}
Write-Host ""
