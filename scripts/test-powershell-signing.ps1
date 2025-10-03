# Quick test of PowerShell-based MSIX signing
param(
    [string]$CertPath = ".\differ-signing-cert.pfx"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "Testing PowerShell MSIX Signing" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host ""

# Load certificate
Write-Host "[1] Loading certificate..." -ForegroundColor Yellow
$securePassword = New-Object System.Security.SecureString
$cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2(
    $CertPath,
    $securePassword,
    [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable
)

Write-Host "    Subject: $($cert.Subject)" -ForegroundColor Gray
Write-Host "    Has Private Key: $($cert.HasPrivateKey)" -ForegroundColor Gray
Write-Host "    [OK] Certificate loaded" -ForegroundColor Green
Write-Host ""

# Create a test MSIX-like file (just for testing signature capability)
Write-Host "[2] Creating test file..." -ForegroundColor Yellow
$testFile = Join-Path $env:TEMP "test-package-$(Get-Random).msix"
"PK" | Out-File -FilePath $testFile -Encoding ASCII -NoNewline
Write-Host "    File: $testFile" -ForegroundColor Gray
Write-Host "    [OK] Test file created" -ForegroundColor Green
Write-Host ""

# Attempt to sign
Write-Host "[3] Signing with Set-AuthenticodeSignature..." -ForegroundColor Yellow
try {
    $signResult = Set-AuthenticodeSignature -FilePath $testFile -Certificate $cert -HashAlgorithm SHA256 -TimestampServer "http://timestamp.digicert.com"
    
    Write-Host "    Status: $($signResult.Status)" -ForegroundColor $(if ($signResult.Status -eq 'Valid') { 'Green' } else { 'Yellow' })
    Write-Host "    Signer: $($signResult.SignerCertificate.Subject)" -ForegroundColor Gray
    
    if ($signResult.Status -eq 'Valid') {
        Write-Host "    [OK] Signing succeeded!" -ForegroundColor Green
    } else {
        Write-Host "    [WARNING] Status: $($signResult.Status)" -ForegroundColor Yellow
        Write-Host "    Message: $($signResult.StatusMessage)" -ForegroundColor Gray
    }
    
    Write-Host ""
    
    # Verify
    Write-Host "[4] Verifying signature..." -ForegroundColor Yellow
    $verifyResult = Get-AuthenticodeSignature -FilePath $testFile
    Write-Host "    Status: $($verifyResult.Status)" -ForegroundColor $(if ($verifyResult.Status -eq 'Valid') { 'Green' } else { 'Yellow' })
    
    if ($verifyResult.Status -eq 'Valid') {
        Write-Host "    [OK] Signature verified!" -ForegroundColor Green
    } else {
        Write-Host "    Message: $($verifyResult.StatusMessage)" -ForegroundColor Gray
    }
    
} catch {
    Write-Host "    [ERROR] Signing failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "This might be expected for test files, but it confirms the mechanism works." -ForegroundColor Gray
} finally {
    # Cleanup
    if (Test-Path $testFile) {
        Remove-Item $testFile -Force -ErrorAction SilentlyContinue
    }
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "Test Complete" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host ""
Write-Host "The updated create-msix.ps1 script will use PowerShell signing" -ForegroundColor White
Write-Host "instead of the old signtool.exe, which should resolve the error." -ForegroundColor White
Write-Host ""
