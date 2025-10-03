# Getting Started with Differ

This guide gets you from download to your first comparison in a couple of minutes. Keep the [installing Differ](../user-guide/installing-differ.md) handbook nearby if you need the deep dive.

## 1. Choose your install path

| Option | Recommended for | Highlight |
| --- | --- | --- |
| **One-click MSIX installer** | Anyone who wants Start Menu integration and automatic updates | Run `Install-Differ.bat` and follow the prompts. |
| **Portable ZIP** | Locked-down environments or quick evaluations | Extract the ZIP and run `Differ.App.exe` – no admin rights required. |

## 2. One-click install (TL;DR)

1. Download these four files from the [latest release](https://github.com/csseeker/differ/releases):
   - `Differ_*.msix`
   - `differ-signing-cert.cer`
   - `install-differ.ps1`
   - `Install-Differ.bat`
2. Put all four files in the same folder.
3. Double-click `Install-Differ.bat` and consent to the Administrator prompt.
4. Read the plan, press **Y**, and let the script finish the certificate and MSIX setup.
5. Press **Windows**, type **Differ**, and launch the app.

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
- Curious about how the app is put together? Review the [architecture overview](architecture.md) and the full [`DESIGN_GUIDELINES.md`](../../DESIGN_GUIDELINES.md).
- Updating icons or branding? See the [branding guide](../branding/icons.md).

---

Happy diffing! If anything is unclear, open an issue so I can improve this guide.
