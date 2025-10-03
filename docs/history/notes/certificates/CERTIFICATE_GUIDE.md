# MSIX Certificate Guide

## Problem: "This app package's publisher certificate could not be verified"

When users download and try to install your MSIX package, they may see this error:

```
This app package's publisher certificate could not be verified. Contact your 
system administrator or the app developer to obtain a new app package with 
verified certificates. The root certificate and all immediate certificates of 
the signature in the app package must be verified (0x800B010A)
```

This happens because **the MSIX is either unsigned or signed with a certificate that's not trusted** on the user's machine.

## Understanding Code Signing Certificates

MSIX packages **must be signed** with a code signing certificate. Windows validates:

1. ✅ The package is digitally signed
2. ✅ The certificate's Subject matches the `Publisher` in `Package.appxmanifest`
3. ✅ The certificate chain is trusted (installed in Trusted Root or Trusted People store)

Without a properly signed and trusted MSIX, Windows blocks installation.

## Solution Options

### **Option 1: Self-Signed Certificate (Recommended for Open Source)**

✅ **Best for**: Testing, open-source projects, small-scale distribution  
⚠️ **Limitation**: Users must manually install the certificate first

#### Step 1: Create a Self-Signed Certificate

Run this script **once** to generate your signing certificate:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\create-certificate.ps1
```

This will:
- Create a code signing certificate matching your Publisher (`CN=csseeker`)
- Export it as `differ-signing-cert.pfx` (private key - **keep secure!**)
- Export it as `differ-signing-cert.cer` (public certificate - distribute this)
- Optionally install it to your local Trusted Root store

The script will prompt you to:
1. Set a password for the PFX file (remember this!)
2. Choose whether to install the certificate locally

#### Step 2: Sign Your MSIX Package

When building the MSIX, use the certificate:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\create-msix.ps1 `
   -PublishDir "src/Differ.App/bin/Release/net8.0/win-x64/publish" `
   -OutputDir "artifacts" `
   -PackageName "csseeker.Differ" `
   -Publisher "CN=csseeker" `
   -DisplayName "Differ" `
   -Version "0.2.0.0" `
   -Sign `
   -CertificatePath "differ-signing-cert.pfx" `
   -CertificatePassword "YourPasswordHere"
```

#### Step 3: Distribute Both MSIX and Certificate

In your GitHub Release, include:
1. **The MSIX file** (`Differ_0.2.0.0_x64.msix`)
2. **The public certificate** (`differ-signing-cert.cer`)
3. **Installation instructions** (see below)

#### Step 4: User Installation Instructions

Create a README for users:

```markdown
## Installing Differ MSIX

### First Time Setup (Required)

Before installing Differ, you must install the signing certificate:

1. Download `differ-signing-cert.cer` from the release
2. Right-click PowerShell and select "Run as Administrator"
3. Run: `powershell -ExecutionPolicy Bypass -File install-certificate.ps1`
4. Verify the certificate details when prompted
5. The certificate will be installed to your Trusted Root store

### Install Differ

After the certificate is installed:

1. Download `Differ_0.2.0.0_x64.msix`
2. Double-click the MSIX file
3. Click "Install"
4. Differ will appear in your Start Menu
```

You can also provide the installation script with your releases.

---

### **Option 2: Trusted Certificate Authority (Recommended for Production)**

✅ **Best for**: Commercial distribution, professional products  
💰 **Cost**: $75-$500/year depending on CA

Purchase a code signing certificate from a trusted Certificate Authority:
- **DigiCert** (recommended, widely trusted)
- **Sectigo** (formerly Comodo)
- **GlobalSign**
- **IdenTrust**

#### Advantages:
- Users can install immediately without extra steps
- Professional appearance (verified publisher)
- Better trust indicators in Windows
- Required for Microsoft Store submission

#### Process:
1. Purchase certificate from CA (~$200-400/year)
2. Complete identity verification (may take 3-7 days)
3. Receive certificate file (PFX/P12 format)
4. Use it to sign your MSIX:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\create-msix.ps1 `
   -Sign `
   -CertificatePath "path\to\purchased-cert.pfx" `
   -CertificatePassword "YourPassword"
```

---

### **Option 3: Azure Code Signing (Modern Cloud Solution)**

✅ **Best for**: Organizations using Azure, modern DevOps  
💰 **Cost**: Varies by Azure usage

Microsoft's new cloud-based code signing service:
- Certificate stored securely in Azure Key Vault
- No local certificate files to manage
- Built-in timestamping and compliance
- Integrates with CI/CD pipelines

See: https://learn.microsoft.com/en-us/azure/code-signing/

---

## Verifying Your Signed MSIX

After signing, verify the package:

```powershell
# Check signature
signtool verify /pa artifacts\Differ_0.2.0.0_x64.msix

# View certificate details
Get-AuthenticodeSignature artifacts\Differ_0.2.0.0_x64.msix | Format-List *
```

Expected output:
```
SignerCertificate      : [Subject]
                           CN=csseeker
                         [Issuer]
                           CN=csseeker
                         [Not Before]
                           1/1/2025 12:00:00 AM
                         [Not After]
                           1/1/2028 12:00:00 AM
Status                 : Valid
StatusMessage          : Signature verified.
```

---

## Certificate Security Best Practices

### Protecting Your Private Key (.pfx file)

🔒 **CRITICAL**: The `.pfx` file contains your private key. If compromised, attackers could sign malware as you.

**Security checklist:**
- ✅ **Never commit PFX files to Git** (add to `.gitignore`)
- ✅ **Use a strong password** (15+ characters)
- ✅ **Store in password manager** or encrypted storage
- ✅ **Limit access** - only CI/CD and release maintainers
- ✅ **Use CI/CD secrets** for automated builds (GitHub Secrets, Azure Key Vault)
- ✅ **Rotate certificates** every 1-3 years

### Recommended .gitignore entries:

```gitignore
# Code signing certificates
*.pfx
*.p12
*signing-cert*
!*-cert.cer  # Allow public certificates only
```

---

## Troubleshooting

### Error: "Certificate's Publisher doesn't match manifest"

The `Subject` field in your certificate must **exactly match** the `Publisher` in `Package.appxmanifest`:

**Manifest:**
```xml
<Identity Publisher="CN=csseeker" ... />
```

**Certificate:**
```
Subject: CN=csseeker
```

They must match character-for-character. If they don't, recreate the certificate with the correct subject:

```powershell
powershell scripts\create-certificate.ps1 -Publisher "CN=csseeker"
```

### Error: "The certificate has expired"

Self-signed certificates typically last 1-3 years. To extend:

1. Create a new certificate
2. Sign the MSIX with the new certificate
3. Distribute the updated `.cer` file
4. Users must install the new certificate

### Error: "Certificate not in Trusted Root"

Users must install the `.cer` file to their Trusted Root or Trusted People store. Ensure they:
1. Run PowerShell **as Administrator**
2. Use the `install-certificate.ps1` script

---

## Summary: Quick Start

### For Developers:

```powershell
# 1. Create certificate (once)
powershell scripts\create-certificate.ps1

# 2. Build and publish app
dotnet publish src/Differ.App -c Release -r win-x64

# 3. Create signed MSIX
powershell scripts\create-msix.ps1 -Sign -CertificatePath "differ-signing-cert.pfx" -CertificatePassword "YourPassword"
```

### For Users:

```powershell
# 1. Install certificate (first time only, as Admin)
powershell -ExecutionPolicy Bypass -File install-certificate.ps1

# 2. Install MSIX
# Double-click Differ_0.2.0.0_x64.msix
```

---

## Next Steps

1. **Add to `.gitignore`**: Ensure PFX files are excluded
2. **Update Release Process**: Include certificate in releases
3. **Document in README**: Add user installation instructions
4. **Consider CI/CD**: Store PFX password in GitHub Secrets for automated signing
5. **Plan for Production**: Budget for trusted CA certificate if going commercial

For questions or issues, see: https://learn.microsoft.com/en-us/windows/msix/package/signing-package-overview
