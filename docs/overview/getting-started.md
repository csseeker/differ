# Getting Started with Differ

This guide gets you from download to your first comparison in a couple of minutes. Keep the [installing Differ](../user-guide/installing-differ.md) handbook nearby if you need the deep dive.

## 1. Choose your install path

| Option | Recommended for | Highlight |
| --- | --- | --- |
| **MSIX installer** | Anyone who wants Start Menu integration | Install certificate, then double-click the MSIX. |
| **Portable ZIP** | Locked-down environments or quick evaluations | Extract the ZIP and run `Differ.App.exe` – no admin rights required. |

## 2. MSIX installation (TL;DR)

**First-time users:** You must install the signing certificate before installing the MSIX.

1. Download these files from the [latest release](https://github.com/csseeker/differ/releases):
   - `Differ_*_x64.msix`
   - `differ-signing-cert.cer`
   - `install-certificate.ps1`
2. **Install the certificate (one-time setup):**
   - Right-click PowerShell and select "Run as Administrator"
   - Navigate to your downloads folder
   - Run: `powershell -ExecutionPolicy Bypass -File install-certificate.ps1`
   - Review and confirm the certificate installation
3. **Install Differ:**
   - Double-click `Differ_*_x64.msix`
   - Click "Install"
4. Press **Windows**, type **Differ**, and launch the app.

If Windows blocks the install or SmartScreen complains, head to the [troubleshooting section](../user-guide/installing-differ.md#troubleshooting) for quick fixes.

## 3. Portable quick start

1. Download `Differ-v*-portable-win-x64.zip` from the release page.
2. Extract it anywhere you have write access.
3. Launch `Differ.App.exe`.
4. Pin the executable or create a shortcut if you want it on your Start Menu or taskbar.

Portable builds do not require certificates or Administrator privileges.

## 4. Run your first comparison

1. Launch Differ and select two directories with the **Browse** buttons.
2. Click **Compare directories**.
3. Use the status filter or search to focus on differences.
4. Double-click a file to view its inline diff.

Large comparisons run asynchronously; you can cancel anytime with the **Stop** button.

## 5. Next steps

- Need more install detail or troubleshooting? Jump to the [Installing Differ guide](../user-guide/installing-differ.md).
- Want to build from source? Follow the instructions in [`README.md`](../../README.md#building-from-source).
- Curious about how the app is put together? Review the [architecture overview](architecture.md) and the full [`DESIGN_GUIDELINES.md`](../DESIGN_GUIDELINES.md).
- Updating icons or branding? See the [branding guide](../branding/icons.md).

---

Happy diffing! If anything is unclear, open an issue so I can improve this guide.
