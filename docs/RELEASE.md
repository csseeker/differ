# Release Playbook

This checklist explains how to package Differ for non-technical users and publish it on GitHub Releases.

## Versioning & Branching

- Use semantic versioning (e.g., `v1.2.0`).
- Keep `master` releasable at all times; cut release branches only when needed for stabilization.
- Update the `<Version>` property in each project file when bumping the version.
- Maintain a `CHANGELOG.md` entry for every change. Move items from the **Unreleased** section under the new version when publishing.

## Pre-release Checklist

1. Ensure `master` is green in CI.
2. Update all version numbers in `.csproj` files and `package.appxmanifest` (when an installer is added).
3. Update the changelog and draft release notes using `docs/RELEASE_NOTES_TEMPLATE.md`.
4. Review documentation and screenshots for accuracy.
5. Validate that GitHub issues targeted for the milestone are closed.

## Local Validation

Run these commands before tagging:

```cmd
:: Clean and restore
dotnet clean
dotnet restore

:: Build and test
dotnet build -c Release
dotnet test -c Release --collect:"XPlat Code Coverage"

:: Publish the app as a self-contained single file
dotnet publish src/Differ.App/Differ.App.csproj -c Release -r win-x64 --self-contained true ^
  /p:PublishSingleFile=true ^
  /p:IncludeNativeLibrariesForSelfExtract=true ^
  /p:EnableCompressionInSingleFile=true
```

The publish step emits `DifferApp.exe` under `src/Differ.App/bin/Release/net8.0/win-x64/publish/`.

## Tagging & Release Creation

1. Commit all changes and push to `master`.
2. Tag the release commit:

   ```cmd
   git tag v1.2.0
   git push origin v1.2.0
   ```

3. Package assets locally:
   - Copy everything from `src/Differ.App/bin/Release/net8.0/win-x64/publish/` into a staging folder.
   - (Optional) Sign `DifferApp.exe` before the next step.
   - Create the distribution archive:

     ```cmd
     mkdir artifacts
     robocopy src\Differ.App\bin\Release\net8.0\win-x64\publish artifacts /E
     powershell -Command "Compress-Archive -Path artifacts/* -DestinationPath Differ-v1.2.0-win-x64.zip -Force"
     ```

     Replace `v1.2.0` with the version you are shipping.
   - To generate an MSIX, run the helper script:

     ```powershell
     pwsh ./scripts/create-msix.ps1 -Version "1.2.0.0" -PackageName "csseeker.Differ" -Publisher "CN=Your Company" -PublisherDisplayName "Your Company"
     ```

     Supply additional parameters (e.g., `-Sign`) as needed. See `docs/MSIX_PACKAGING.md` for full options.
   - Collect supporting files (e.g., `coverage.cobertura.xml`, screenshots) into the same folder for upload.

4. Create a GitHub Release manually:
   - Draft release notes using `docs/RELEASE_NOTES_TEMPLATE.md`.
   - Upload `Differ-v<version>-win-x64.zip` and any supplementary files.
   - Publish the release when verification is complete.

## User-Facing Assets

- **ZIP naming**: `Differ-v<version>-win-x64.zip`
- **README**: Link to the latest release and provide simple unzip & run steps.
- **Screenshots**: Optional but recommended—store under `docs/media` and attach to releases.

## Code Signing (Optional for v1)**

If you have an Authenticode certificate, sign the executable before packaging:

```cmd
signtool.exe sign /tr http://timestamp.digicert.com /td SHA256 ^
  /fd SHA256 /a DifferApp.exe
```

Document the certificate location and password management in a secure, private channel.

## Post-release Follow-up

- Announce the release (GitHub Discussions, social channels).
- Create a milestone for the next iteration and triage leftover issues.
- Monitor crash logs and user feedback.

## MSIX Packaging (Optional)

To distribute an installer with Start menu integration and automatic updates, follow the guide in `docs/MSIX_PACKAGING.md`. Once the MSIX package is built:

- Sign the package.
- Attach the `.msix` (or `.msixbundle`) file alongside the ZIP in the GitHub Release.
- Update the release notes with installation guidance.

## Future Enhancements

- Integrate automatic update checks pointing to the GitHub Releases feed.
- Automate code signing with secure certificate storage in GitHub Secrets.
