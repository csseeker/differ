# Fixing the MSIX Certificate Issue - Action Plan

## ✅ Completed

I've created the necessary tools and documentation to fix the MSIX certificate verification error.

## 📋 What Was Created

### Scripts
1. **`scripts/create-certificate.ps1`** - Generate a self-signed code signing certificate
2. **`scripts/install-certificate.ps1`** - Users run this to install the certificate
3. **Updated `scripts/create-msix.ps1`** - Existing script now supports signing
4. **Updated `scripts/create-release.ps1`** - Automatically includes certificate in releases

### Documentation
1. **`docs/CERTIFICATE_GUIDE.md`** - Comprehensive technical documentation
2. **`docs/INSTALLATION_GUIDE.md`** - Step-by-step user instructions
3. **`docs/CERTIFICATE_QUICK_REF.md`** - Quick reference card
4. **Updated `README.md`** - Installation instructions
5. **Updated `.gitignore`** - Protect private certificate files

## 🚀 Next Steps (For Next Release)

### 1. Create the Signing Certificate

**Run this command once:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\create-certificate.ps1
```

This will:
- Prompt you to set a password (remember it!)
- Create `differ-signing-cert.pfx` (private key - **keep secure!**)
- Create `differ-signing-cert.cer` (public certificate - distribute with releases)
- Optionally install the certificate to your local machine for testing

**Important:**
- ✅ Commit `differ-signing-cert.cer` to Git (public certificate is safe)
- ❌ **NEVER** commit `differ-signing-cert.pfx` (already in .gitignore)
- 💾 Store the PFX password securely (use a password manager)

### 2. Test the Certificate Locally

**Verify it was created correctly:**

```powershell
# Check certificate details
Get-PfxCertificate .\differ-signing-cert.pfx

# Should show:
# Subject: CN=csseeker
# Issuer: CN=csseeker
```

**Test building a signed MSIX:**

```powershell
# Build and publish
dotnet publish src\Differ.App\Differ.App.csproj -c Release -r win-x64 --self-contained true

# Create signed MSIX
powershell scripts\create-msix.ps1 `
   -Version "0.2.0.0" `
   -Publisher "CN=csseeker" `
   -Sign `
   -CertificatePath "differ-signing-cert.pfx" `
   -CertificatePassword "YourPasswordHere"
```

**Verify the signature:**

```powershell
# Check signature
signtool verify /pa artifacts\Differ_0.2.0.0_x64.msix

# Should show: "Successfully verified"
```

### 3. Test Installation on a Clean Machine

**Option A: Test on same machine**

1. Uninstall any existing Differ installation:
   ```powershell
   Get-AppxPackage *Differ* | Remove-AppxPackage
   ```

2. Remove the certificate (if you installed it earlier):
   ```powershell
   # Run as Administrator
   $cert = Get-ChildItem -Path Cert:\CurrentUser\Root | Where-Object { $_.Subject -like "*csseeker*" }
   $cert | Remove-Item
   ```

3. Try installing the MSIX (should fail with certificate error) ✓

4. Install the certificate:
   ```powershell
   powershell -ExecutionPolicy Bypass -File scripts\install-certificate.ps1
   ```

5. Try installing the MSIX again (should succeed) ✓

**Option B: Test on a VM or different PC**

1. Copy these files to the test machine:
   - `artifacts\Differ_0.2.0.0_x64.msix`
   - `differ-signing-cert.cer`
   - `scripts\install-certificate.ps1`

2. Follow the user installation instructions in `docs\INSTALLATION_GUIDE.md`

3. Verify it installs successfully

### 4. Create the Next Release

**When you're ready for the next release (e.g., v0.2.1):**

```powershell
# Run the release script with certificate
powershell scripts\create-release.ps1 `
   -Version "0.2.1" `
   -CertificatePfxPath "differ-signing-cert.pfx" `
   -CertificatePassword "YourPassword"
```

This will automatically:
- ✅ Build the portable ZIP
- ✅ Create and sign the MSIX
- ✅ Copy the certificate to artifacts
- ✅ Copy the installation script to artifacts
- ✅ Generate release notes with installation instructions

### 5. Create GitHub Release

1. **Tag the release:**
   ```bash
   git tag v0.2.1
   git push origin v0.2.1
   ```

2. **Create GitHub Release:**
   - Go to: https://github.com/csseeker/differ/releases/new
   - Select tag: `v0.2.1`
   - Title: `v0.2.1 - <Brief Description>`
   - Copy content from `docs\Releases\v0.2.1-alpha.md`

3. **Upload these files:**
   - `Differ-v0.2.1-portable-win-x64.zip`
   - `Differ_0.2.1.0_x64.msix`
   - `differ-signing-cert.cer` ⭐ **NEW**
   - `install-certificate.ps1` ⭐ **NEW**

4. **Add installation instructions to release notes:**
   - Link to `docs/INSTALLATION_GUIDE.md`
   - Emphasize that first-time MSIX users need to install the certificate
   - Mention the portable version as an alternative

### 6. Update Existing v0.1.0 Release (Optional)

If you want to fix the current release:

1. Build a signed MSIX for v0.1.0:
   ```powershell
   # Check out the v0.1.0 tag
   git checkout v0.1.0
   
   # Publish
   dotnet publish src\Differ.App\Differ.App.csproj -c Release -r win-x64 --self-contained true
   
   # Create signed MSIX
   powershell scripts\create-msix.ps1 `
      -Version "0.1.0.0" `
      -Publisher "CN=csseeker" `
      -Sign `
      -CertificatePath "differ-signing-cert.pfx" `
      -CertificatePassword "YourPassword"
   ```

2. Edit the v0.1.0 release on GitHub
3. Replace the MSIX and add the certificate files
4. Update the release notes with installation instructions

## 📝 Communication to Users

### For Existing Users

Add a note to the README or create an announcement:

```markdown
## 📢 Important: MSIX Installation Update

**If you downloaded v0.1.0 or earlier MSIX packages**, you may have encountered 
an installation error. I've fixed this issue in v0.2.1 and later.

**To install MSIX packages:**
1. Download the certificate file (`.cer`) from the release
2. Run the installation script as Administrator
3. Then install the MSIX

**Alternatively**, use the portable ZIP version - no installation needed!

See the [Installation Guide](docs/INSTALLATION_GUIDE.md) for details.
```

### In Release Notes

Include this section:

```markdown
## 🆕 MSIX Installation Change

Starting with this release, the MSIX package is properly signed with a code 
signing certificate. This ensures the package integrity and security.

**First-time MSIX users must:**
1. Install the signing certificate (one-time setup)
2. See [Installation Guide](docs/INSTALLATION_GUIDE.md) for step-by-step instructions

**Portable ZIP users:** No changes - just extract and run as before!
```

## 🎯 Success Criteria

You'll know everything is working when:

- ✅ MSIX is signed and verified: `signtool verify /pa` succeeds
- ✅ Certificate installs without errors on a clean machine
- ✅ MSIX installs successfully after certificate installation
- ✅ Application launches and runs correctly
- ✅ Users can see "Differ" in their Start Menu
- ✅ Future updates install without reinstalling the certificate

## 🔒 Security Reminders

1. **Never commit `differ-signing-cert.pfx` to Git**
   - Already protected by .gitignore
   - This is your private key!

2. **Store the PFX password securely**
   - Use a password manager
   - Consider GitHub Secrets for CI/CD

3. **The public certificate (`.cer`) is safe to commit**
   - Users need this to install your MSIX
   - Include it in every release

4. **Certificate expires in 3 years**
   - Set a reminder to regenerate before expiration
   - Users will need to install the new certificate

## ❓ Questions or Issues?

If you encounter any problems:

1. Check the detailed guides:
   - `docs/CERTIFICATE_GUIDE.md` - Technical details
   - `docs/INSTALLATION_GUIDE.md` - User instructions
   - `docs/CERTIFICATE_QUICK_REF.md` - Quick reference

2. Verify certificate matches manifest:
   - Certificate: `Subject: CN=csseeker`
   - Manifest: `Publisher="CN=csseeker"`

3. Test on a clean environment to reproduce user experience

## 📚 Additional Resources

- [Microsoft Docs: Signing MSIX Packages](https://learn.microsoft.com/en-us/windows/msix/package/signing-package-overview)
- [Microsoft Docs: Create Self-Signed Certificate](https://learn.microsoft.com/en-us/windows/msix/package/create-certificate-package-signing)
- [Microsoft Docs: Install Test Certificate](https://learn.microsoft.com/en-us/windows-hardware/drivers/install/installing-test-certificates)

---

**Ready to proceed?** Start with Step 1: Create the signing certificate! 🚀
