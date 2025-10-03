# Install the Differ signing certificate to enable MSIX installation
# This script must be run by users before they can install the MSIX package
# Requires Administrator privileges

param(
    [string]$CertPath = ".\differ-signing-cert.cer"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Check if running as administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "=" * 80 -ForegroundColor Red
    Write-Host "ERROR: This script must be run as Administrator" -ForegroundColor Red
    Write-Host "=" * 80 -ForegroundColor Red
    Write-Host ""
    Write-Host "Right-click PowerShell and select 'Run as Administrator', then run this script again." -ForegroundColor Yellow
    Write-Host ""
    exit 1
}

if (-not (Test-Path $CertPath)) {
    Write-Host "ERROR: Certificate file not found at: $CertPath" -ForegroundColor Red
    Write-Host "Please ensure the .cer file is in the same directory as this script." -ForegroundColor Yellow
    exit 1
}

Write-Host "Installing Differ Signing Certificate..." -ForegroundColor Cyan
Write-Host "Certificate: $CertPath" -ForegroundColor Gray
Write-Host ""

try {
    # Load the certificate
    $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($CertPath)
    
    Write-Host "Certificate Details:" -ForegroundColor White
    Write-Host "  Subject:    $($cert.Subject)" -ForegroundColor Gray
    Write-Host "  Issuer:     $($cert.Issuer)" -ForegroundColor Gray
    Write-Host "  Valid From: $($cert.NotBefore)" -ForegroundColor Gray
    Write-Host "  Valid To:   $($cert.NotAfter)" -ForegroundColor Gray
    Write-Host "  Thumbprint: $($cert.Thumbprint)" -ForegroundColor Gray
    Write-Host ""
    
    # Install to Trusted Root (required for self-signed certificates)
    $rootStore = New-Object System.Security.Cryptography.X509Certificates.X509Store(
        [System.Security.Cryptography.X509Certificates.StoreName]::Root,
        [System.Security.Cryptography.X509Certificates.StoreLocation]::LocalMachine
    )
    $rootStore.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
    $rootStore.Add($cert)
    $rootStore.Close()
    
    Write-Host "[OK] Certificate installed to Trusted Root Certificate Authorities" -ForegroundColor Green
    Write-Host ""
    
    # Also install to Trusted People (sometimes required for MSIX)
    $peopleStore = New-Object System.Security.Cryptography.X509Certificates.X509Store(
        [System.Security.Cryptography.X509Certificates.StoreName]::TrustedPeople,
        [System.Security.Cryptography.X509Certificates.StoreLocation]::LocalMachine
    )
    $peopleStore.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
    $peopleStore.Add($cert)
    $peopleStore.Close()
    
    Write-Host "[OK] Certificate installed to Trusted People" -ForegroundColor Green
    Write-Host ""
    Write-Host "=" * 80 -ForegroundColor Green
    Write-Host "SUCCESS: Certificate installed successfully!" -ForegroundColor Green
    Write-Host "=" * 80 -ForegroundColor Green
    Write-Host ""
    Write-Host "You can now install the Differ MSIX package by double-clicking it." -ForegroundColor White
    Write-Host ""
    
} catch {
    Write-Host "=" * 80 -ForegroundColor Red
    Write-Host "ERROR: Failed to install certificate" -ForegroundColor Red
    Write-Host "=" * 80 -ForegroundColor Red
    Write-Host ""
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    exit 1
}
