# Quick certificate recreation script (non-interactive)
# This will create a certificate with NO password for simplicity

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$Publisher = "CN=csseeker"
$CertPath = ".\differ-signing-cert.pfx"
$ValidityYears = 3

Write-Host "Creating self-signed certificate for MSIX signing..." -ForegroundColor Cyan
Write-Host "Publisher: $Publisher" -ForegroundColor Gray
Write-Host ""

# Use a simple password (or you can use empty by using SecureString constructor)
$securePassword = New-Object System.Security.SecureString

# Remove old certificate from store if it exists
$existingCerts = Get-ChildItem Cert:\CurrentUser\My | Where-Object { $_.Subject -eq $Publisher }
foreach ($oldCert in $existingCerts) {
    Write-Host "Removing old certificate: $($oldCert.Thumbprint)" -ForegroundColor Yellow
    Remove-Item -Path "Cert:\CurrentUser\My\$($oldCert.Thumbprint)" -Force
}

# Create the certificate with exportable private key
$cert = New-SelfSignedCertificate `
    -Type CodeSigningCert `
    -Subject $Publisher `
    -KeyUsage DigitalSignature `
    -FriendlyName "Differ MSIX Signing Certificate" `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -KeyExportPolicy Exportable `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}") `
    -NotAfter (Get-Date).AddYears($ValidityYears)

Write-Host "[OK] Certificate created successfully" -ForegroundColor Green
Write-Host "    Thumbprint: $($cert.Thumbprint)" -ForegroundColor Gray
Write-Host "    Subject: $($cert.Subject)" -ForegroundColor Gray
Write-Host "    Valid Until: $($cert.NotAfter)" -ForegroundColor Gray
Write-Host "    Has Private Key: $($cert.HasPrivateKey)" -ForegroundColor Gray
Write-Host ""

# Export the certificate to a PFX file WITH private key
$certFile = (Resolve-Path $CertPath -ErrorAction SilentlyContinue)
if (-not $certFile) {
    $certFile = Join-Path (Get-Location) (Split-Path $CertPath -Leaf)
}

Write-Host "Exporting certificate with private key..." -ForegroundColor Gray
Export-PfxCertificate -Cert $cert -FilePath $certFile -Password $securePassword -ChainOption BuildChain | Out-Null

# Verify the exported PFX contains the private key
Write-Host "Verifying exported certificate..." -ForegroundColor Gray
$verifyCert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($certFile, $securePassword, [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable)

if ($verifyCert.HasPrivateKey) {
    Write-Host "[OK] Certificate exported successfully with private key" -ForegroundColor Green
    Write-Host "    File: $certFile" -ForegroundColor Gray
    Write-Host "    Has Private Key: TRUE" -ForegroundColor Green
} else {
    Write-Host "[ERROR] Certificate was exported but does not contain private key!" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Export the public certificate (for distribution to users)
$cerFile = $certFile -replace '\.pfx$', '.cer'
Export-Certificate -Cert $cert -FilePath $cerFile | Out-Null
Write-Host "[OK] Public certificate exported to: $cerFile" -ForegroundColor Green
Write-Host ""

# Install to trusted root
Write-Host "Installing certificate to Trusted Root store..." -ForegroundColor Gray
$store = New-Object System.Security.Cryptography.X509Certificates.X509Store(
    [System.Security.Cryptography.X509Certificates.StoreName]::Root,
    [System.Security.Cryptography.X509Certificates.StoreLocation]::CurrentUser
)
$store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
$store.Add($cert)
$store.Close()

Write-Host "[OK] Certificate installed to Trusted Root" -ForegroundColor Green
Write-Host ""

Write-Host "=" * 80 -ForegroundColor Green
Write-Host "SUCCESS! Certificate is ready for MSIX signing" -ForegroundColor Green
Write-Host "=" * 80 -ForegroundColor Green
Write-Host ""
Write-Host "To sign your MSIX package, use:" -ForegroundColor Cyan
Write-Host "  -Sign -CertificatePath '$certFile'" -ForegroundColor Gray
Write-Host "  (No password needed)" -ForegroundColor Gray
Write-Host ""
