# MSIX Packaging Guide

This document explains how to create an MSIX installer for Differ so Windows users can install, update, and uninstall the app just like any other desktop application.

## Why MSIX?

- Seamless installation with Start menu integration
- Automatic updates when hosting newer packages
- Built-in integrity checks and easy uninstallation

## Prerequisites

- Windows 10 2004 (build 19041) or later
- Visual Studio 2022 with the **Universal Windows Platform development** workload _or_ the standalone MSIX Packaging Tool
- `makeappx.exe` and `signtool.exe` (bundled with the Windows 10/11 SDK)
- Authenticode code-signing certificate (self-signed for testing, trusted certificate for production)

## Option A: Visual Studio Packaging Project (recommended)

The repository already includes `Differ.Package`, a Windows Application Packaging Project wired to the WPF app. Simply open `Differ.sln` in Visual Studio and you are ready to create MSIX bundles.

1. **Review metadata**: open `src/Differ.Package/Package.appxmanifest` and update the Identity (`Name`, `Publisher`, `Version`) before each release.
2. **Assets**: customize the logos in `src/Differ.Package/Images/`. Run `scripts/refresh-packaging-assets.ps1` if you need to regenerate placeholders.
3. **Build configuration**: switch to `Release | x64` (or the platform you plan to distribute).
4. **Publish**: choose `Build > Publish > Create App Packages…` and follow the wizard to produce an `.msixbundle`.
5. **Sign**: let Visual Studio sign with your certificate, or sign the generated MSIX manually (see below).

## Option B: Command-line Packaging from Published Output

This path avoids adding a packaging project and uses the existing publish folder.

1. Publish the WPF app as framework-dependent (MSIX supplies required components):

   ```cmd
   dotnet publish src/Differ.App/Differ.App.csproj -c Release -r win-x64 --self-contained false
   ```

2. Create `AppxManifest.xml` describing the package (copy `docs/assets/AppxManifest.sample.xml` once created and adjust identity, version, and logo paths).
3. Run `makeappx.exe`:

   ```cmd
   makeappx.exe pack /d publish /p Differ_1.2.0.0_x64.msix /l
   ```

4. Sign the MSIX:

   ```cmd
   signtool.exe sign /tr http://timestamp.digicert.com /td SHA256 ^
     /fd SHA256 /a Differ_1.2.0.0_x64.msix
   ```

5. Verify signature:

   ```cmd
   signtool.exe verify /pa Differ_1.2.0.0_x64.msix
   ```

## Option C: Use the helper script (`scripts/create-msix.ps1`)

The repository includes a PowerShell script that wraps Option B and handles manifest generation, placeholder logos, and optional code signing.

1. Ensure `dotnet publish` has been executed (framework-dependent for MSIX):

   ```cmd
   dotnet publish src/Differ.App/Differ.App.csproj -c Release -r win-x64 --self-contained false
   ```

2. Run the helper script (PowerShell or Windows Terminal):

    ```powershell
    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/create-msix.ps1 `
       -PublishDir "src/Differ.App/bin/Release/net8.0/win-x64/publish" `
       -OutputDir "artifacts" `
       -PackageName "csseeker.Differ" `
   -Publisher "CN=csseeker" `
       -PublisherDisplayName "csseeker" `
       -DisplayName "Differ" `
       -Description "Compare directories and files quickly." `
       -ApplicationId "Differ" `
       -Version "1.0.0.0" `
       -Architecture "x64"
    ```

   Adjust the metadata to match your signing certificate and desired identity. The script:

   - Copies the publish output into a staging folder.
   - Generates placeholder tile/logo assets if they are missing.
   - Emits an `AppxManifest.xml` by tokenising `docs/assets/AppxManifest.sample.xml`.
   - Calls `makeappx.exe` to produce `Differ_1.0.0.0_x64.msix` under the output directory. It could be on D drive.
   - (Optional) Signs and verifies the package when `-Sign` is provided.

   > **Note:** The script depends on the Windows SDK tools (`makeappx.exe`, `signtool.exe`) and the .NET `System.Drawing` assembly. Run it on Windows with the Desktop Runtime installed.

3. Optional signing: supply `-Sign -CertificatePath "path\to\certificate.pfx" [-CertificatePassword "secret"]` to sign the MSIX. You can also override `-SigntoolPath` or `-MakeAppxPath` when the tools are not in `PATH`.

4. Verify the resulting package by double-clicking the `.msix` on a test machine (see QA checklist).

## Distributing the MSIX

- Attach the `.msix` (or `.msixbundle`) file to the GitHub Release alongside the ZIP.
- Mention MSIX installation steps in the release notes (see `docs/RELEASE_NOTES_TEMPLATE.md`).
- Optionally host the MSIX on a web server and provide an `.appinstaller` file for automatic update checks.

## Testing the Package

1. Double-click the MSIX on a clean test machine. Windows Installer should show the Differ branding and publisher.
2. Validate Start menu entry, uninstaller entry, and default install location (`%ProgramFiles%\WindowsApps`).
3. Launch Differ and run the smoke tests from `docs/QA.md`.
4. Uninstall the app and confirm no files remain in user folders.

## Keeping Versions in Sync

- The MSIX `Package.appxmanifest` version must match the version in `Differ.App`.
- Update the `<Identity Version="..."/>` field every release.
- The MSIX build pipeline should run after the ZIP packaging so you can upload both artifacts together.

## Next Steps

- Automate package creation with a build script once the manual workflow is stable.
- Integrate MSIX-specific tests (installation, upgrade, uninstall) into the QA checklist.
