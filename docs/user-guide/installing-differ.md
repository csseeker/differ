# Installing Differ

Use this guide for everything from the one-click MSIX experience to manual certificate setup and troubleshooting. If you only need the quick version, start with the [getting started guide](../overview/getting-started.md).

## Supported platforms

- Windows 10 version 1809 (build 17763) or later
- Windows 11 (all editions)
- Administrator permission is required the first time you install the signing certificate

## Files in each release

| File | Purpose |
| --- | --- |
| `Differ_*.msix` | Signed MSIX installer with Start Menu integration |
| `differ-signing-cert.cer` | Public certificate required to trust the MSIX |
| `install-certificate.ps1` | PowerShell script to install the certificate |
| `Differ-v*-portable-win-x64.zip` | Portable build – unzip and run `Differ.App.exe` (no install) |

## Option A – MSIX installation (recommended)

**First-time users:** You must install the signing certificate before installing the MSIX.

1. Download these files from the [latest release](https://github.com/csseeker/differ/releases):
   - `Differ_*_x64.msix` - The installer
   - `differ-signing-cert.cer` - The signing certificate
   - `install-certificate.ps1` - The installation script

2. **Install the certificate (one-time setup):**
   - Right-click PowerShell and select "Run as Administrator"
   - Navigate to your downloads folder
   - Run: `powershell -ExecutionPolicy Bypass -File install-certificate.ps1`
   - Review and confirm the certificate installation

3. **Install Differ:**
   - Double-click `Differ_*_x64.msix`
   - Click "Install"

4. **Updating to newer versions:**
   - No need to reinstall the certificate
   - Simply download and install the new MSIX

5. Launch Differ from the Start Menu.

## Option B – Portable ZIP

1. Download `Differ-v*-portable-win-x64.zip`.
2. Extract it to any folder you control (no admin rights required).
3. Run `Differ.App.exe`.
4. Create a shortcut if you want Start Menu or desktop access.

Portable builds do not modify the system or require the certificate. Use them when you cannot elevate or when testing new releases side-by-side.

## Advanced: Manual certificate installation

If you prefer to install the certificate manually without using the script:

1. Install the signing certificate (once per machine):
   ```powershell
   # Run PowerShell as Administrator
   cd <folder-with-downloads>
   Import-Certificate -FilePath "differ-signing-cert.cer" -CertStoreLocation Cert:\LocalMachine\Root
   Import-Certificate -FilePath "differ-signing-cert.cer" -CertStoreLocation Cert:\LocalMachine\TrustedPeople
   ```
   > **Important:** Both stores are required. Skipping `TrustedPeople` causes error `0x800B0100`.

2. Double-click the `.msix` file and follow the installer prompts.
3. For future updates, just install the new MSIX – the certificate remains trusted until it expires.

More background on certificates lives in the [Certificates & signing guide](../distribution/certificates.md).

## Verification checklist

After installing the MSIX:

- ✅ **Start Menu** entry named *Differ – Directory Comparison Tool*.
- ✅ App Launches within a few seconds and shows the main window.
- ✅ No SmartScreen warnings (after the certificate is trusted).
- ✅ Optional: `Get-AppxPackage *Differ*` in PowerShell lists the installed package.

## Troubleshooting

| Symptom | Likely cause | Fix |
| --- | --- | --- |
| "This app package's publisher certificate could not be verified" | Certificate missing from Trusted People | Re-run the one-click installer or import the cert into both stores. See [Certificates guide](../distribution/certificates.md#install-the-public-certificate). |
| "This app can't run on your PC" | Unsupported Windows build or wrong architecture | Confirm you downloaded the x64 package and are on Windows 10 1809+. |
| SmartScreen blocks the PowerShell script | Execution policy or SmartScreen warning | Use the `-ExecutionPolicy Bypass` flag when running the script, or choose **More info → Run anyway**. |
| Installation still fails after cert installation | Cached certificate or stale state | Restart Windows, then rerun the installer. As a fallback, remove older certs (see certificates guide) before reimporting. |
| Need to clean uninstall | Using portable build or MSIX leftovers | For MSIX: `Settings → Apps → Installed apps → Differ → Uninstall` or `Get-AppxPackage *Differ* | Remove-AppxPackage`. Portable builds simply delete the extracted folder. |

## Advanced notes

- The installer script writes verbose logs to the console; copy them if you need to report an issue.
- Certificates expire periodically. New releases include an updated `.cer`; import it to refresh trust.
- To validate the signature manually:
  ```powershell
  Get-AuthenticodeSignature "Differ_*.msix" | Format-List *
  ```

## Still stuck?

- Review the deep-dive in the [Certificates & signing guide](../distribution/certificates.md).
- Open an issue at [github.com/csseeker/differ/issues](https://github.com/csseeker/differ/issues) with the error output.
- As a temporary workaround, switch to the portable build while I investigate.
