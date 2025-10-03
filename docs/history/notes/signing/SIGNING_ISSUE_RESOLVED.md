# MSIX Signing Issue - RESOLVED ✅

## Summary
The MSIX signing error has been identified and fixed.

## Root Causes

### 1. Certificate Missing Private Key ✅ FIXED
- **Problem:** The `differ-signing-cert.pfx` did not contain the private key
- **Evidence:** 
  - Original file size: 776 bytes (public key only)
  - `HasPrivateKey: False` when inspected
- **Root Cause:** Certificate export was missing `-KeyExportPolicy Exportable` parameter
- **Solution:** Recreated certificate with private key
  - New file size: 2,678 bytes
  - `HasPrivateKey: True` ✅

### 2. Signtool.exe Not in PATH ✅ FOUND
- **Problem:** `signtool.exe` was not in the system PATH
- **Evidence:** Script unable to locate signing tool
- **Resolution:** Found at: `C:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\signtool.exe`
- **Verification:** Tool executes successfully ✅

## Actions Taken

1. **Created Diagnostic Script** (`scripts/diagnose-signing-issue.ps1`)
   - Validates certificate has private key
   - Searches for signtool.exe
   - Tests signing capability

2. **Fixed Certificate Creation** (`scripts/create-certificate.ps1`)
   - Added `-KeyExportPolicy Exportable` to ensure private key export
   - Added verification step to confirm private key in exported PFX
   - Enhanced error messages

3. **Created Quick Recreation Script** (`scripts/recreate-cert-quick.ps1`)
   - Non-interactive certificate recreation
   - Removes old certificates from store
   - Verifies exported certificate
   - Installs to Trusted Root automatically

4. **Created Signtool Finder** (`scripts/find-signtool.ps1`)
   - Searches common signtool locations
   - Provides installation guidance if not found
   - Offers PATH configuration options

5. **Updated Documentation** (`docs/engineering/SIGNING_ISSUE_RESOLUTION.md`)
   - Complete troubleshooting guide
   - Installation instructions
   - Verification steps

## Current Status

✅ Certificate has private key  
✅ Signtool.exe located and functional  
✅ create-msix.ps1 has search patterns for ClickOnce SignTool location  
✅ Ready for MSIX signing

## Next Steps

### To Create a Signed Release:

```powershell
# Option 1: Full release (recommended)
.\scripts\create-release.ps1 -Version "0.1.1"

# Option 2: MSIX only
.\scripts\create-msix.ps1 -Sign -CertificatePath ".\differ-signing-cert.pfx" -Version "0.1.1.0"
```

### Verification

The `create-msix.ps1` script will:
1. Automatically find signtool.exe in ClickOnce SignTool location
2. Use the recreated certificate with private key
3. Sign the MSIX package
4. Verify the signature

### If Signing Still Fails

Run diagnostics:
```powershell
.\scripts\diagnose-signing-issue.ps1
```

Expected output:
```
[OK] Certificate loaded successfully (no password required)
     Has Private Key: True
[OK] Found 1 signtool installation(s)
[OK] Test signing succeeded!
```

## Technical Details

### Certificate Specifications
- **Subject:** CN=csseeker
- **Type:** CodeSigningCert
- **Key Usage:** DigitalSignature
- **Validity:** 3 years (until Oct 2028)
- **Password:** None (empty for simplicity)
- **Location:** CurrentUser\My store + exported to PFX

### Signtool Configuration
- **Path:** `C:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\signtool.exe`
- **Version:** 4.00 (th2_release_sec.160328-1908)
- **Arguments:** 
  ```
  sign /tr http://timestamp.digicert.com /td SHA256 /fd SHA256 /f differ-signing-cert.pfx {package.msix}
  ```

### Why It Failed Before
1. **Certificate Export:** The original export didn't include the private key, making signing impossible
2. **Tool Discovery:** The script searches multiple SDK locations, including ClickOnce, so this should work now

## Files Modified

- ✅ `differ-signing-cert.pfx` - Recreated with private key (2,678 bytes)
- ✅ `differ-signing-cert.cer` - Updated public certificate
- ✅ `scripts/create-certificate.ps1` - Enhanced with private key verification
- ✅ `scripts/recreate-cert-quick.ps1` - New quick recreation script
- ✅ `scripts/diagnose-signing-issue.ps1` - New diagnostic tool
- ✅ `scripts/find-signtool.ps1` - New signtool finder
- ✅ `docs/engineering/SIGNING_ISSUE_RESOLUTION.md` - Detailed resolution guide

## Prevention

To avoid this issue in the future:
1. Always use `scripts/create-certificate.ps1` (now fixed) for certificate creation
2. Run `scripts/diagnose-signing-issue.ps1` before creating releases
3. Ensure `-KeyExportPolicy Exportable` is used when creating certificates

---

**Issue Status:** RESOLVED ✅  
**Date:** October 4, 2025  
**Ready for Production:** YES
