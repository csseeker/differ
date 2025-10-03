# Create a self-signed certificate for signing the MSIX package
# This certificate will be valid for 3 years and can be used for code signing
# Users will need to install this certificate to their Trusted Root/People store

param(
    [string]$Publisher = "CN=csseeker",
    [string]$CertPath = ".\differ-signing-cert.pfx",
    [string]$Password = "",
    [int]$ValidityYears = 3
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Write-Host "Creating self-signed certificate for MSIX signing..." -ForegroundColor Cyan
Write-Host "Publisher: $Publisher" -ForegroundColor Gray
Write-Host "Certificate will be saved to: $CertPath" -ForegroundColor Gray
Write-Host ""

# Check if running with appropriate permissions
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
$isAdmin = $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if ($isAdmin) {
    Write-Host "[INFO] Running as Administrator" -ForegroundColor Green
} else {
    Write-Host "[INFO] Running as regular user (this is fine for CurrentUser certificate store)" -ForegroundColor Gray
}
Write-Host ""

# Prompt for password if not provided
if ([string]::IsNullOrWhiteSpace($Password)) {
    $securePassword = Read-Host -AsSecureString "Enter a password to protect the certificate"
} else {
    $securePassword = ConvertTo-SecureString -String $Password -AsPlainText -Force
}

# Create the certificate
$cert = New-SelfSignedCertificate `
    -Type CodeSigningCert `
    -Subject $Publisher `
    -KeyUsage DigitalSignature `
    -FriendlyName "Differ MSIX Signing Certificate" `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}") `
    -NotAfter (Get-Date).AddYears($ValidityYears)

Write-Host "[OK] Certificate created successfully" -ForegroundColor Green
Write-Host "    Thumbprint: $($cert.Thumbprint)" -ForegroundColor Gray
Write-Host "    Subject: $($cert.Subject)" -ForegroundColor Gray
Write-Host "    Valid Until: $($cert.NotAfter)" -ForegroundColor Gray
Write-Host ""

# Export the certificate to a PFX file WITH private key
$certPath = (Resolve-Path -Path (Split-Path $CertPath -Parent)).Path
$certFile = Join-Path $certPath (Split-Path $CertPath -Leaf)

# Use -ChainOption BuildChain to include the full certificate chain
# The private key should be automatically included from the CurrentUser\My store
try {
    Export-PfxCertificate -Cert $cert -FilePath $certFile -Password $securePassword -ChainOption BuildChain | Out-Null
    
    # Verify the exported PFX contains the private key
    $verifyCert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($certFile, $securePassword, [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable)
    
    if (-not $verifyCert.HasPrivateKey) {
        throw "Exported certificate does not contain private key!"
    }
    
    $okMsg = "Certificate exported to: $certFile"
    Write-Host "[OK] $okMsg" -ForegroundColor Green
    Write-Host "    Private key: Included" -ForegroundColor Green
    Write-Host ""
} catch {
    $errMsg = "Failed to export certificate with private key: $($_.Exception.Message)"
    Write-Host "[ERROR] $errMsg" -ForegroundColor Red
    throw
}

# Export the public certificate (for distribution to users)
$cerFile = $certFile -replace '\.pfx$', '.cer'
Export-Certificate -Cert $cert -FilePath $cerFile | Out-Null

$okMsg2 = "Public certificate exported to: $cerFile"
Write-Host "[OK] $okMsg2" -ForegroundColor Green
Write-Host ""

Write-Host "=" * 80 -ForegroundColor Yellow
Write-Host "IMPORTANT: Certificate Usage Instructions" -ForegroundColor Yellow
Write-Host "=" * 80 -ForegroundColor Yellow
Write-Host ""
Write-Host "1. TO SIGN YOUR MSIX PACKAGE:" -ForegroundColor Cyan
Write-Host "   Run create-msix.ps1 with these parameters:" -ForegroundColor White
Write-Host "   -Sign -CertificatePath `"$certFile`" -CertificatePassword `"YourPassword`"" -ForegroundColor Gray
Write-Host ""
Write-Host "2. FOR USERS TO INSTALL THE MSIX:" -ForegroundColor Cyan
Write-Host "   They must install the certificate first. Provide them with:" -ForegroundColor White
Write-Host "   - The .cer file: $cerFile" -ForegroundColor Gray
Write-Host "   - Instructions to install it (see install-certificate.ps1)" -ForegroundColor Gray
Write-Host ""
Write-Host "3. AUTOMATED INSTALLATION OPTION:" -ForegroundColor Cyan
Write-Host "   Run: powershell -ExecutionPolicy Bypass -File scripts\install-certificate.ps1" -ForegroundColor Gray
Write-Host ""
Write-Host "=" * 80 -ForegroundColor Yellow
Write-Host ""

# Add certificate to the current user's trusted root (for local testing)
$answer = Read-Host "Do you want to install this certificate to your Trusted Root store now? (y/n)"
if ($answer -eq 'y' -or $answer -eq 'Y') {
    $store = New-Object System.Security.Cryptography.X509Certificates.X509Store(
        [System.Security.Cryptography.X509Certificates.StoreName]::Root,
        [System.Security.Cryptography.X509Certificates.StoreLocation]::CurrentUser
    )
    $store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
    $store.Add($cert)
    $store.Close()
    
    $okMsg3 = "Certificate installed to Trusted Root Certificate Authorities"
    Write-Host "[OK] $okMsg3" -ForegroundColor Green
    Write-Host "    You can now install MSIX packages signed with this certificate" -ForegroundColor Gray
} else {
    $infoMsg = "Certificate not installed. You will need to install it manually or run install-certificate.ps1"
    Write-Host "[INFO] $infoMsg" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Done! Your certificate is ready for use." -ForegroundColor Green
