# MSIX Packaging Guide

Create signed MSIX installers so Windows users can install, update, and uninstall Differ with a familiar experience. This guide consolidates the previous packaging notes in one place.

## Why ship an MSIX?

- Seamless installation with Start Menu integration
- Built-in integrity checks and clean uninstall
- Automatic update support when paired with an `.appinstaller` feed

## Prerequisites

- Windows 10 2004 (build 19041) or later
- Visual Studio 2022 with the **Universal Windows Platform development** workload *or* the Windows SDK tools (`makeappx.exe`, `signtool.exe`)
- A code-signing certificate (`differ-signing-cert.pfx` for self-signed builds)
- Published Differ binaries (`dotnet publish`) ready for packaging

## Option A – Visual Studio packaging project (recommended)

The solution includes `Differ.Package`, a Windows Application Packaging Project.

1. Open `Differ.sln` in Visual Studio.
2. Update `src/Differ.Package/Package.appxmanifest` with the new version, name, and publisher before every release.
3. Replace icons in `src/Differ.Package/Images/` (see [branding guide](../branding/icons.md)).
4. Switch to the `Release | x64` configuration.
5. Choose **Build → Publish → Create App Packages…** and follow the wizard.
6. Let Visual Studio sign the package or sign it later using `signtool`.

## Option B – Command line using published output

1. Publish the WPF app (framework-dependent for MSIX):
   ```cmd
   dotnet publish src\Differ.App\Differ.App.csproj -c Release -r win-x64 --self-contained false
   ```
2. Prepare an `AppxManifest.xml`. Start from `docs/assets/AppxManifest.sample.xml`, copy it into your staging folder, and update identity, version, and asset paths.
3. Package the MSIX:
   ```cmd
   makeappx.exe pack /d publish /p Differ_1.2.0.0_x64.msix /l
   ```
4. Sign the package:
   ```cmd
   signtool.exe sign /tr http://timestamp.digicert.com /td SHA256 ^
     /fd SHA256 /a Differ_1.2.0.0_x64.msix
   ```
5. Verify the signature:
   ```cmd
   signtool.exe verify /pa Differ_1.2.0.0_x64.msix
   ```

## Option C – Automation script (`scripts/create-msix.ps1`)

The repository ships with a helper script that wraps option B.

```powershell
# Run after dotnet publish
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

Add `-Sign -CertificatePath "differ-signing-cert.pfx" [-CertificatePassword ...]` to sign during packaging. The script copies published binaries into a staging folder, ensures icons are present, tokenises the manifest, builds the MSIX, and optionally calls `signtool`.

## Certificate requirements

MSIX installers **must** be signed. Differ uses a self-signed certificate; see the [Certificates & signing guide](certificates.md) for creation, rotation, and user installation instructions. Remember to increment the manifest version whenever you rebuild the MSIX.

## Testing checklist

1. Install the package on a clean machine (or VM). The installer should display Differ branding and the `csseeker` publisher.
2. Launch the app and run the smoke tests from the [Release playbook](release-playbook.md#quality-gates).
3. Verify Start Menu entry, uninstaller entry (`Settings → Apps`), and launch time.
4. Uninstall the package and confirm cleanup.
5. Reinstall or update to ensure upgrades succeed without manual cleanup.

## Publishing the installer

- Upload the `.msix` (or `.msixbundle`) with the ZIP and scripts to each GitHub release.
- Reference the [Installing Differ guide](../user-guide/installing-differ.md) in release notes so users know how to trust the certificate.
- Consider hosting an `.appinstaller` for automatic updates once the distribution flow stabilises.

## Version alignment

- Keep `Package.appxmanifest`'s `<Identity Version="...">` in sync with the application version.
- Update `<Publisher>` to match the signing certificate subject (currently `CN=csseeker`).
- Include the version bump in `CHANGELOG.md` and the release notes template.

## Next steps

- Automate MSIX creation in CI using the same PowerShell script and GitHub Actions secrets.
- Add install/upgrade smoke tests to the QA matrix.
- Evaluate purchasing a trusted CA certificate when preparing for broader distribution.
