# MSIX Signing Issue - Resolution Guide

## Problem
When trying to sign the MSIX package during release creation, you encountered:
```
SignTool Error: An unexpected internal error has occurred.
```

## Root Causes Identified

### 1. Certificate Missing Private Key ✅ FIXED
**Problem:** The `differ-signing-cert.pfx` file did not contain the private key.
- Original file size: 776 bytes (public key only)
- This happened because the certificate export didn't include `-KeyExportPolicy Exportable`

**Solution:** Recreated the certificate with private key included
- New file size: 2,678 bytes (includes private key)
- Used `scripts\recreate-cert-quick.ps1`
- Certificate now validates with `HasPrivateKey: True`

### 2. Signtool.exe Not Found ⚠️ NEEDS ATTENTION
**Problem:** The Windows SDK `signtool.exe` is not installed or not in PATH.

**Solution Options:**

#### Option A: Install Windows SDK (Recommended)
1. Download and install Windows 10 SDK:
   https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/

2. During installation, ensure these components are selected:
   - ✅ Windows SDK Signing Tools for Desktop Apps
   - ✅ Windows SDK for UWP Managed Apps

3. After installation, `signtool.exe` will be at:
   ```
   C:\Program Files (x86)\Windows Kits\10\bin\{version}\x64\signtool.exe
   ```

4. The `create-msix.ps1` script will automatically find it

#### Option B: Use Existing SDK Installation
If you have Visual Studio installed, signtool may already be on your system:

```powershell
# Search for signtool
Get-ChildItem "C:\Program Files (x86)" -Recurse -Filter signtool.exe -ErrorAction SilentlyContinue
Get-ChildItem "C:\Program Files" -Recurse -Filter signtool.exe -ErrorAction SilentlyContinue
```

Then update your PATH or use the `-SigntoolPath` parameter:
```powershell
.\scripts\create-msix.ps1 -Sign -CertificatePath ".\differ-signing-cert.pfx" -SigntoolPath "C:\Path\To\signtool.exe"
```

#### Option C: Skip Signing (Development Only)
For development builds, you can skip signing:
```powershell
.\scripts\create-release.ps1 -SkipMsix  # Skip MSIX entirely
# OR create unsigned MSIX manually:
.\scripts\create-msix.ps1  # Without -Sign parameter
```

## Verification Steps

### 1. Verify Certificate
```powershell
powershell -ExecutionPolicy Bypass -File scripts\diagnose-signing-issue.ps1
```

Expected output:
```
[OK] Certificate loaded successfully (no password required)
     Has Private Key: True
```

### 2. Verify Signtool
```powershell
where.exe signtool.exe
# OR
Get-Command signtool.exe
```

Should return path to signtool.exe

### 3. Test Signing
Once both are fixed, test with:
```powershell
.\scripts\create-release.ps1 -Version "0.1.1-test"
```

## Files Modified/Created

- ✅ `differ-signing-cert.pfx` - Recreated with private key
- ✅ `differ-signing-cert.cer` - Updated public certificate
- ✅ `scripts\create-certificate.ps1` - Enhanced with private key verification
- ✅ `scripts\recreate-cert-quick.ps1` - Quick certificate recreation script
- ✅ `scripts\diagnose-signing-issue.ps1` - Diagnostic tool

## Next Steps

1. **Install Windows SDK** (if not already installed)
2. **Run diagnostic again** to confirm both certificate and signtool are ready
3. **Create a test release** to verify signing works end-to-end

## Technical Details

### Why the Certificate Failed
The original `New-SelfSignedCertificate` command didn't include `-KeyExportPolicy Exportable`, which is required for exporting the private key. The fixed version includes:

```powershell
New-SelfSignedCertificate `
    -KeyExportPolicy Exportable `  # This is critical!
    -Type CodeSigningCert `
    -Subject $Publisher `
    # ... other parameters
```

### Signtool Command Being Used
```powershell
signtool.exe sign `
    /tr http://timestamp.digicert.com `  # Timestamp server
    /td SHA256 `                          # Timestamp digest
    /fd SHA256 `                          # File digest
    /f differ-signing-cert.pfx `          # Certificate file
    Differ_x.x.x.x_x64.msix              # File to sign
```

The `/tr` parameter requires internet connectivity to timestamp the signature.
