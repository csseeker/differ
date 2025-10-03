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
| `install-differ.ps1` | PowerShell automation that installs the cert and the MSIX |
| `Install-Differ.bat` | Double-click launcher that elevates and runs the PowerShell script |
| `Differ-v*-portable-win-x64.zip` | Portable build – unzip and run `Differ.App.exe` (no install) |

## Option A – One-click install (recommended)

1. Download the four files listed above from the [latest release](https://github.com/csseeker/differ/releases) and place them in the same folder.
2. Double-click `Install-Differ.bat`. Windows will request elevation; choose **Yes**.
3. Review the summary in the PowerShell window. Press **Y** to continue.
4. The script will:
   - Install the certificate into **Trusted Root Certification Authorities** and **Trusted People**.
   - Install or upgrade the MSIX package.
   - Verify the installation and show next steps.
5. Press **Enter** to close the window when prompted.
6. Launch Differ from the Start Menu.

The batch file is safe to rerun – it will detect existing installations and offer to update them.

## Option B – Manual MSIX installation

If you prefer to handle the steps yourself:

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

## Option C – Portable ZIP

1. Download `Differ-v*-portable-win-x64.zip`.
2. Extract it to any folder you control (no admin rights required).
3. Run `Differ.App.exe`.
4. Create a shortcut if you want Start Menu or desktop access.

Portable builds do not modify the system or require the certificate. Use them when you cannot elevate or when testing new releases side-by-side.

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
| SmartScreen blocks the batch file or script | Execution policy or SmartScreen warning | Choose **More info → Run anyway**, or run `Install-Differ-Direct.bat` from an elevated PowerShell session. |
| PowerShell window closes too quickly | UAC prompt denied or script aborted | Right-click `Install-Differ.bat` → **Run as administrator** and watch the console for prompts. |
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
