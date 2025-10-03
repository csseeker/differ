# Installing Differ MSIX Package

This guide explains how to install Differ on Windows using the MSIX installer.

## Prerequisites

- Windows 10 version 1809 (build 17763) or later
- Windows 11 (any version)
- Administrator access (required for certificate installation only)

---

## Installation Steps

### 🚀 Quick Install (Easiest Method)

**For users who want the simplest experience:**

1. Download these files from the [latest release](https://github.com/csseeker/differ/releases):
   - `Differ_x.x.x.x_x64.msix` (the installer)
   - `differ-signing-cert.cer` (the certificate)
   - `install-differ.ps1` (the installation script)
   - `Install-Differ.bat` (the launcher - **start here!**)

2. **Put all files in the same folder**

3. **Double-click `Install-Differ.bat`**

4. **Click "Yes"** when Windows asks for Administrator permission

5. **Follow the on-screen prompts** in the PowerShell window

That's it! The script will:
- ✅ Install the certificate automatically
- ✅ Install the MSIX package
- ✅ Verify everything worked
- ✅ Tell you how to launch Differ

---

### Step 1: Install the Signing Certificate (Manual Method)

⚠️ **Note**: If you used the Quick Install above, skip this section!

#### Option A: Automated Installation (Recommended)

1. Download **both files** from the [latest release](https://github.com/csseeker/differ/releases):
   - `Differ_x.x.x.x_x64.msix` (the installer)
   - `differ-signing-cert.cer` (the certificate)
   - `install-certificate.ps1` (the installation script)

2. **Right-click PowerShell** and select **"Run as Administrator"**

3. Navigate to your downloads folder:
   ```powershell
   cd Downloads
   ```

4. Run the installation script:
   ```powershell
   powershell -ExecutionPolicy Bypass -File install-certificate.ps1
   ```

5. Review the certificate details and confirm installation

#### Option B: Manual Installation

1. Download `differ-signing-cert.cer` from the release

2. **Right-click** the `.cer` file and select **"Install Certificate"**

3. Select **"Local Machine"** (requires Administrator) and click **Next**

4. Select **"Place all certificates in the following store"**

5. Click **"Browse"** and select **"Trusted Root Certification Authorities"**

6. Click **Next**, then **Finish**

7. Confirm the security warning by clicking **"Yes"**

8. Repeat steps 2-7, but in step 5 select **"Trusted People"** instead

---

### Step 2: Install Differ

After the certificate is installed:

1. **Double-click** `Differ_x.x.x.x_x64.msix`

2. Click **"Install"** in the installer dialog

3. Wait for installation to complete (typically 5-10 seconds)

4. Click **"Launch"** or find **"Differ"** in your Start Menu

---

## Verification

After installation, you should see:

✅ **Start Menu**: "Differ - Directory Comparison Tool" in your apps list  
✅ **Desktop**: No desktop shortcut (this is normal for MSIX apps)  
✅ **Terminal Access**: Type `differ` in Command Prompt or PowerShell (if configured)

---

## Updating Differ

When a new version is released:

1. **No need to reinstall the certificate** (it's already trusted)
2. Download the new MSIX file
3. Double-click to install - it will update automatically
4. Existing settings are preserved

---

## Uninstalling Differ

### Via Settings

1. Open **Settings** → **Apps** → **Installed apps**
2. Find **"Differ - Directory Comparison Tool"**
3. Click the **three dots** → **"Uninstall"**

### Via PowerShell

```powershell
Get-AppxPackage *Differ* | Remove-AppxPackage
```

---

## Troubleshooting

### "This app package's publisher certificate could not be verified"

**Cause**: The certificate is not installed or installation failed.

**Solution**:
1. Verify you ran PowerShell **as Administrator**
2. Re-run `install-certificate.ps1` (see Step 1 above)
3. Check that the certificate is installed:
   ```powershell
   Get-ChildItem -Path Cert:\LocalMachine\Root | Where-Object { $_.Subject -like "*csseeker*" }
   Get-ChildItem -Path Cert:\LocalMachine\TrustedPeople | Where-Object { $_.Subject -like "*csseeker*" }
   ```

### "The app didn't start" or Installation Fails

**Cause**: .NET Desktop Runtime may not be installed.

**Solution**:
1. Download and install [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Choose the **"Desktop Runtime"** (x64)
3. Restart the installation

### "This app can't run on your PC"

**Cause**: Wrong architecture or Windows version.

**Solution**:
- Ensure you downloaded the **x64** version (not ARM64)
- Verify Windows version: `winver` (must be 1809/17763 or later)

### Certificate Installation Script Won't Run

**Cause**: Execution policy blocked the script.

**Solution**:
```powershell
# Temporarily bypass execution policy
powershell -ExecutionPolicy Bypass -File install-certificate.ps1
```

### "This app was blocked by your administrator"

**Cause**: Group Policy or Windows Defender SmartScreen.

**Solution**:
- **For SmartScreen**: Click "More info" → "Run anyway"
- **For Group Policy**: Contact your IT administrator
- **Alternative**: Use the portable ZIP version instead

---

## Security Notice

The Differ MSIX is signed with a **self-signed certificate** because this is an open-source project. This is why you need to manually install the certificate.

**Is this safe?**
- ✅ You can verify the certificate matches the release
- ✅ The certificate ensures the package hasn't been tampered with
- ✅ This is a standard practice for open-source Windows apps
- ⚠️ Only download from the official GitHub releases page

**Verifying the Certificate**:
```powershell
# View certificate details
Get-AuthenticodeSignature "Differ_x.x.x.x_x64.msix" | Format-List *

# Expected: Subject = CN=csseeker
```

---

## Why MSIX?

MSIX packages offer several advantages:
- ✅ **Clean installation**: No registry pollution
- ✅ **Easy updates**: Install new version over old
- ✅ **Complete removal**: Uninstall leaves no traces
- ✅ **Start Menu integration**: Automatic shortcuts
- ✅ **Sandboxed**: Better security and isolation

---

## Troubleshooting

### Installation Fails with Certificate Error

**Error Message:**
```
error 0x800B0100: The app package must be digitally signed for signature validation.
```

**Solution:**
The certificate must be installed in **TWO** certificate stores:
1. Trusted Root Certification Authorities
2. Trusted People (**critical for MSIX**)

**Fix it manually:**
```powershell
# Run PowerShell as Administrator, then:

# Install to Trusted Root
Import-Certificate -FilePath "differ-signing-cert.cer" `
    -CertStoreLocation "Cert:\LocalMachine\Root"

# Install to Trusted People (REQUIRED!)
Import-Certificate -FilePath "differ-signing-cert.cer" `
    -CertStoreLocation "Cert:\LocalMachine\TrustedPeople"
```

**Verify both stores:**
```powershell
# Check Trusted Root
Get-ChildItem Cert:\LocalMachine\Root | Where-Object { $_.Subject -like '*csseeker*' }

# Check Trusted People  
Get-ChildItem Cert:\LocalMachine\TrustedPeople | Where-Object { $_.Subject -like '*csseeker*' }
```

Both commands should show the certificate with thumbprint `4397B8F5AB16B21A83F1691E11DFA68C91C75E6C`.

### PowerShell Window Closes Immediately

If the installer window closes before you can read it:

1. Open PowerShell as Administrator manually:
   - Right-click Start → Windows PowerShell (Admin)

2. Navigate to the folder with the files:
   ```powershell
   cd "C:\path\to\downloaded\files"
   ```

3. Run the installer directly:
   ```powershell
   .\install-differ.ps1
   ```

### UAC Prompt Doesn't Appear

If clicking the batch file does nothing:

1. Right-click `Install-Differ.bat`
2. Select **"Run as administrator"**
3. Or use `Install-Differ-Direct.bat` which runs in the current window

### MSIX Installation Still Fails

If you've installed the certificate correctly but MSIX still won't install:

1. **Check Windows version:**
   ```powershell
   [System.Environment]::OSVersion.Version
   ```
   Must be build 17763 or higher.

2. **Try the portable version instead:**
   - No certificate needed
   - No installation required
   - Works on any Windows version

3. **Check Event Viewer:**
   - Look for the ActivityID mentioned in the error
   - `Get-AppPackageLog -ActivityID <id>` for details

### Certificate Already Installed but Still Fails

Sometimes Windows caches certificate information. Try:

1. **Restart your computer** (seriously, this often fixes it)
2. **Reimport the certificate:**
   ```powershell
   # Remove old certificates
   Get-ChildItem Cert:\LocalMachine\Root | 
       Where-Object { $_.Subject -like '*csseeker*' } | 
       Remove-Item
   
   Get-ChildItem Cert:\LocalMachine\TrustedPeople | 
       Where-Object { $_.Subject -like '*csseeker*' } | 
       Remove-Item
   
   # Import fresh
   Import-Certificate -FilePath "differ-signing-cert.cer" `
       -CertStoreLocation Cert:\LocalMachine\Root
   Import-Certificate -FilePath "differ-signing-cert.cer" `
       -CertStoreLocation Cert:\LocalMachine\TrustedPeople
   ```

---

## Alternative Installation

If you prefer not to install certificates or use MSIX, download the **portable ZIP version**:

1. Download `Differ-vX.X.X-portable-win-x64.zip`
2. Extract to any folder
3. Run `Differ.App.exe`
4. No installation or certificates needed

---

## Need Help?

- 📖 [Documentation](https://github.com/csseeker/differ/tree/master/docs)
- 🐛 [Report Issues](https://github.com/csseeker/differ/issues)
- 💬 [Discussions](https://github.com/csseeker/differ/discussions)

---

## For Developers

Interested in how the certificate works? See:
- [Certificate Guide](CERTIFICATE_GUIDE.md) - Detailed technical documentation
- [MSIX Packaging Guide](MSIX_PACKAGING.md) - Building MSIX packages
