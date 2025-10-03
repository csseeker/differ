# MSIX Certificate - Quick Reference

## 🎯 Quick Summary

**Problem**: MSIX won't install - "publisher certificate could not be verified"  
**Solution**: Install the signing certificate before installing the MSIX  
**Why**: Differ uses a self-signed certificate (standard for open-source projects)

---

## 📥 For Users: Installing Differ MSIX

### One-Time Setup (5 minutes)

1. **Download 3 files from the release:**
   - `Differ_X.X.X.X_x64.msix`
   - `differ-signing-cert.cer`
   - `install-certificate.ps1`

2. **Install certificate (as Administrator):**
   ```powershell
   # Right-click PowerShell → Run as Administrator
   cd Downloads
   powershell -ExecutionPolicy Bypass -File install-certificate.ps1
   ```

3. **Install Differ:**
   - Double-click the `.msix` file
   - Click "Install"

### Future Updates
- Just download and install the new MSIX
- No need to reinstall the certificate ✅

---

## 🔧 For Developers: Creating Signed MSIX

### One-Time Setup

Create your signing certificate:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\create-certificate.ps1
```

This creates:
- `differ-signing-cert.pfx` (private key - **DO NOT commit to Git!**)
- `differ-signing-cert.cer` (public certificate - distribute with releases)

### Every Release

Build and sign the MSIX:

```powershell
# 1. Publish the app
dotnet publish src/Differ.App -c Release -r win-x64

# 2. Create signed MSIX
powershell scripts\create-msix.ps1 `
   -Version "0.2.0.0" `
   -Sign `
   -CertificatePath "differ-signing-cert.pfx" `
   -CertificatePassword "YourPassword"
```

Or use the all-in-one release script:

```powershell
powershell scripts\create-release.ps1 -Version "0.2.0" `
   -CertificatePfxPath "differ-signing-cert.pfx" `
   -CertificatePassword "YourPassword"
```

### Distribution Checklist

Include in every GitHub Release:
- ✅ `Differ_X.X.X.X_x64.msix` (signed MSIX)
- ✅ `Differ-vX.X.X-portable-win-x64.zip` (portable version)
- ✅ `differ-signing-cert.cer` (public certificate)
- ✅ `install-certificate.ps1` (installation helper)

---

## 🔐 Security Notes

### What Gets Committed to Git
- ✅ `differ-signing-cert.cer` (public certificate) - Safe to commit
- ✅ `install-certificate.ps1` (installation script) - Safe to commit
- ✅ `create-certificate.ps1` (certificate generator) - Safe to commit
- ❌ `differ-signing-cert.pfx` (private key) - **NEVER commit!**
- ❌ Certificate passwords - **Use environment variables or secrets!**

### .gitignore Entries
```gitignore
# Code signing certificates (SECURITY: Never commit private keys!)
*.pfx
*.p12
differ-signing-cert.*
!differ-signing-cert.cer  # Allow public certificate only
```

---

## 📚 Detailed Documentation

- **[Certificate Guide](CERTIFICATE_GUIDE.md)** - Complete technical documentation
- **[Installation Guide](INSTALLATION_GUIDE.md)** - Detailed user instructions
- **[MSIX Packaging Guide](MSIX_PACKAGING.md)** - Building MSIX packages

---

## ❓ Common Issues

### "Certificate could not be verified"
→ Install certificate as Administrator using `install-certificate.ps1`

### "Publisher name doesn't match"
→ Certificate Subject must match Package.appxmanifest Publisher
```xml
<Identity Publisher="CN=csseeker" ... />
```
```powershell
# Certificate must have: Subject: CN=csseeker
```

### "Certificate has expired"
→ Create a new certificate (they expire every 1-3 years)

### Script won't run
→ Use: `powershell -ExecutionPolicy Bypass -File script.ps1`

---

## 🎓 Understanding the Certificate

**What it is:**
- A self-signed code signing certificate
- Proves the MSIX hasn't been tampered with
- Standard practice for open-source projects

**What it's NOT:**
- NOT a security risk (you can verify it matches the release)
- NOT a virus or malware
- NOT required for the portable ZIP version

**Why we need it:**
- Windows requires **all MSIX packages to be signed**
- Trusted CA certificates cost $200-400/year
- Self-signed is free and works perfectly for open-source

---

## 💡 Alternative: Use Portable Version

Don't want to deal with certificates?

1. Download `Differ-vX.X.X-portable-win-x64.zip`
2. Extract and run `Differ.App.exe`
3. No certificates, no installation, no problems! 🎉

---

## 📞 Need Help?

- 📖 [Full Installation Guide](INSTALLATION_GUIDE.md)
- 🐛 [Report Issues](https://github.com/csseeker/differ/issues)
- 💬 [Ask Questions](https://github.com/csseeker/differ/discussions)
