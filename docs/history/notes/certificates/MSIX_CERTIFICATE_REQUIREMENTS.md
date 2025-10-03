# MSIX Certificate Requirements

## Critical Discovery

For MSIX packages to install successfully with self-signed certificates, the certificate **MUST be installed in TWO certificate stores**:

### Required Certificate Stores

1. **Trusted Root Certification Authorities** (`Cert:\LocalMachine\Root`)
   - Establishes the certificate as a trusted root
   - Required for general certificate trust chain validation

2. **Trusted People** (`Cert:\LocalMachine\TrustedPeople`)
   - **CRITICAL for MSIX installation**
   - Without this, MSIX installation fails with error:
     ```
     error 0x800B0100: The app package must be digitally signed for signature validation.
     ```

## Installation Commands

### PowerShell (Administrator)

```powershell
# Install to Trusted Root
Import-Certificate -FilePath "differ-signing-cert.cer" `
    -CertStoreLocation "Cert:\LocalMachine\Root"

# Install to Trusted People (REQUIRED for MSIX)
Import-Certificate -FilePath "differ-signing-cert.cer" `
    -CertStoreLocation "Cert:\LocalMachine\TrustedPeople"
```

### Verification

```powershell
# Check Trusted Root
Get-ChildItem -Path Cert:\LocalMachine\Root | Where-Object { $_.Subject -like '*csseeker*' }

# Check Trusted People
Get-ChildItem -Path Cert:\LocalMachine\TrustedPeople | Where-Object { $_.Subject -like '*csseeker*' }
```

## Why Both Stores?

- **Trusted Root**: General certificate trust for code signing
- **Trusted People**: Specific requirement for Windows App Installer/MSIX deployment
  - Windows checks this store when validating MSIX package signatures
  - Missing this causes signature validation to fail even if the signature is technically valid

## Automated Installation

The `install-differ.ps1` script automatically installs the certificate to **both** stores:

```powershell
# Installs to Root store
$rootStore = New-Object System.Security.Cryptography.X509Certificates.X509Store(
    [System.Security.Cryptography.X509Certificates.StoreName]::Root,
    [System.Security.Cryptography.X509Certificates.StoreLocation]::LocalMachine
)

# Installs to TrustedPeople store
$peopleStore = New-Object System.Security.Cryptography.X509Certificates.X509Store(
    [System.Security.Cryptography.X509Certificates.StoreName]::TrustedPeople,
    [System.Security.Cryptography.X509Certificates.StoreLocation]::LocalMachine
)
```

## Testing Results

✅ **Successful Installation**: After installing certificate to both stores, MSIX installation completes without errors
✅ **Package Verification**: `Get-AppxPackage` shows Differ installed successfully
✅ **Start Menu Integration**: Application appears in Windows Start Menu
✅ **App Launch**: Application launches successfully from Start Menu

## References

- Error Code: `0x800B0100` - Package must be digitally signed
- Error Code: `0x80073CF0` - Package could not be opened (signature validation failed)
- Certificate Thumbprint: `4397B8F5AB16B21A83F1691E11DFA68C91C75E6C`
- Certificate Subject: `CN=csseeker`
- Valid Until: October 3, 2028

## Historical Note

Initial installation attempts failed because only the Trusted Root store was being used. After researching the specific error codes and MSIX requirements, I discovered that the TrustedPeople store is also required for MSIX package deployment on Windows.
