# Diagnose MSIX signing issues
param(
    [string]$CertPath = ".\differ-signing-cert.pfx",
    [string]$Password = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "MSIX Signing Diagnostics" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host ""

# 1. Check if certificate file exists
Write-Host "[1] Checking certificate file..." -ForegroundColor Yellow
if (Test-Path $CertPath) {
    $certInfo = Get-Item $CertPath
    Write-Host "  [OK] Certificate file found: $($certInfo.FullName)" -ForegroundColor Green
    Write-Host "      Size: $($certInfo.Length) bytes" -ForegroundColor Gray
    Write-Host "      Modified: $($certInfo.LastWriteTime)" -ForegroundColor Gray
} else {
    Write-Host "  [ERROR] Certificate file not found at: $CertPath" -ForegroundColor Red
    exit 1
}
Write-Host ""

# 2. Try to load certificate without password
Write-Host "[2] Testing certificate without password..." -ForegroundColor Yellow
try {
    $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($CertPath, '')
    Write-Host "  [OK] Certificate loaded successfully (no password required)" -ForegroundColor Green
    Write-Host "      Subject: $($cert.Subject)" -ForegroundColor Gray
    Write-Host "      Thumbprint: $($cert.Thumbprint)" -ForegroundColor Gray
    Write-Host "      Valid From: $($cert.NotBefore)" -ForegroundColor Gray
    Write-Host "      Valid To: $($cert.NotAfter)" -ForegroundColor Gray
    Write-Host "      Has Private Key: $($cert.HasPrivateKey)" -ForegroundColor Gray
    
    if (-not $cert.HasPrivateKey) {
        Write-Host "  [ERROR] Certificate does not contain private key!" -ForegroundColor Red
        Write-Host "         You need a .pfx file with the private key to sign packages." -ForegroundColor Red
    }
} catch {
    Write-Host "  [INFO] Certificate is password protected (expected)" -ForegroundColor Gray
    Write-Host "        Error: $($_.Exception.Message)" -ForegroundColor Gray
}
Write-Host ""

# 3. Check for signtool
Write-Host "[3] Searching for signtool.exe..." -ForegroundColor Yellow
$signtoolPaths = @()

# Check common locations
$searchPaths = @(
    "C:\Program Files (x86)\Windows Kits\10\bin\*\x64\signtool.exe",
    "C:\Program Files\Windows Kits\10\bin\*\x64\signtool.exe",
    "C:\Program Files (x86)\Windows Kits\10\App Certification Kit\signtool.exe",
    "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\*\x64\signtool.exe"
)

foreach ($pattern in $searchPaths) {
    $found = Get-ChildItem -Path $pattern -File -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
    if ($found) {
        $signtoolPaths += $found.FullName
    }
}

if ($signtoolPaths.Count -eq 0) {
    Write-Host "  [ERROR] signtool.exe not found!" -ForegroundColor Red
    Write-Host "         Install Windows 10 SDK from:" -ForegroundColor Red
    Write-Host "         https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/" -ForegroundColor Red
} else {
    Write-Host "  [OK] Found $($signtoolPaths.Count) signtool installation(s):" -ForegroundColor Green
    foreach ($path in $signtoolPaths) {
        Write-Host "      - $path" -ForegroundColor Gray
        
        # Get version
        $versionInfo = (Get-Item $path).VersionInfo
        Write-Host "        Version: $($versionInfo.FileVersion)" -ForegroundColor Gray
    }
    
    # Use the first one for testing
    $signtool = $signtoolPaths[0]
    Write-Host ""
    Write-Host "  Testing with: $signtool" -ForegroundColor Cyan
    
    # 4. Test signtool sign command with detailed output
    Write-Host ""
    Write-Host "[4] Testing signtool sign command..." -ForegroundColor Yellow
    
    # Create a dummy file to sign
    $testFile = Join-Path $env:TEMP "test-signing-$(Get-Random).txt"
    "Test file for signing" | Out-File -FilePath $testFile -Encoding UTF8
    
    try {
        Write-Host "  Creating test file: $testFile" -ForegroundColor Gray
        
        $signArgs = @(
            'sign',
            '/debug',
            '/v',
            '/fd', 'SHA256',
            '/f', $CertPath,
            $testFile
        )
        
        if (-not [string]::IsNullOrWhiteSpace($Password)) {
            $signArgs = $signArgs[0..4] + '/p' + $Password + $signArgs[5..6]
        }
        
        Write-Host "  Running signtool with verbose output..." -ForegroundColor Gray
        Write-Host "  Command: signtool $($signArgs -join ' ')" -ForegroundColor DarkGray
        Write-Host ""
        
        $output = & $signtool @signArgs 2>&1
        Write-Host $output
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "  [OK] Test signing succeeded!" -ForegroundColor Green
        } else {
            Write-Host ""
            Write-Host "  [ERROR] Test signing failed with exit code: $LASTEXITCODE" -ForegroundColor Red
            Write-Host "         This is the root cause of your MSIX signing failure." -ForegroundColor Red
        }
    } catch {
        Write-Host "  [ERROR] Exception during signing test:" -ForegroundColor Red
        Write-Host "         $($_.Exception.Message)" -ForegroundColor Red
    } finally {
        if (Test-Path $testFile) {
            Remove-Item $testFile -Force -ErrorAction SilentlyContinue
        }
    }
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "Diagnostics Complete" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan
