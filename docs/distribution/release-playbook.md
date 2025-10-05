# Release Playbook

This playbook consolidates release preparation, packaging, QA, and coverage expectations. Use it every time you publish a new version.

## 1. Versioning & branching

- Follow semantic versioning (`vMAJOR.MINOR.PATCH`).
- Keep `master` releasable; create a release branch only when stabilising.
- Update `<Version>` in the following locations **before building**:
  - `src/Differ.App/Differ.App.csproj` - `<AssemblyVersion>` and `<FileVersion>`
  - `src/Differ.Package/Package.appxmanifest` - `<Identity Version="...">`
  - Ensure all versions match to avoid confusion (e.g., all should be `0.2.0.0` for v0.2.0 release)
- Maintain `CHANGELOG.md`. Move items from **Unreleased** to the new version section during release.
- Update `README.md` to reference the new version as "Latest Release"

## 2. Pre-release checklist

- [ ] CI is green on `master`.
- [ ] Version numbers updated **and aligned** in all locations (see section 1).
- [ ] Changelog entry written (use [release notes template](../RELEASE_NOTES_TEMPLATE.md)).
- [ ] Documentation and screenshots reviewed.
- [ ] Targeted GitHub issues closed or deferred.
- [ ] `README.md` references the new version as latest.

**Version Alignment Check:**
```powershell
# Quick verification script - all should show the same version
Select-String -Path "src/Differ.App/Differ.App.csproj" -Pattern "AssemblyVersion"
Select-String -Path "src/Differ.Package/Package.appxmanifest" -Pattern "Version="
```

## 3. Local validation

```cmd
:: Clean and restore
dotnet clean
dotnet restore

:: Build and test
Dotnet build -c Release
dotnet test -c Release --collect:"XPlat Code Coverage"

:: Publish the app (portable ZIP inputs)
dotnet publish src\Differ.App\Differ.App.csproj -c Release -r win-x64 --self-contained true ^
  /p:PublishSingleFile=true ^
  /p:IncludeNativeLibrariesForSelfExtract=true ^
  /p:EnableCompressionInSingleFile=true
```

The publish step outputs to `src/Differ.App/bin/Release/net8.0/win-x64/publish/`.

## 4. Package artifacts

### Portable ZIP

1. Copy the publish output into `artifacts/portable`.
2. Zip the folder:
   ```powershell
   Compress-Archive -Path artifacts/portable/* -DestinationPath artifacts/Differ-v<version>-portable-win-x64.zip -Force
   ```

### MSIX

1. Ensure publish output exists (`--self-contained false` is recommended for MSIX).
2. Run `scripts/create-msix.ps1` with the correct metadata and signing options (see [MSIX packaging](msix-packaging.md)).
3. Verify the installer on a test machine.

### One-click installer assets

The release script (`scripts/create-release.ps1`) copies:

- `Install-Differ.bat`
- `install-differ.ps1`
- `differ-signing-cert.cer`
- MSIX + portable ZIP
- `README.md` for artifact context (optional)

## 5. QA & quality gates

### Automated

- ✅ `dotnet test -c Release` (required)
- ✅ Code coverage report collected (target >40% overall, >90% for `Differ.Core`)

### Manual smoke tests

Follow the condensed checklist (derived from the historical `docs/history/notes/quality/QA.md`):

1. Launch portable build on Windows 11 and Windows 10 test machines.
2. Compare sample directories under `tests/Differ.Tests/TestData/`.
3. Cancel an in-flight comparison.
4. Verify settings persistence (if applicable).
5. Install MSIX, confirm Start Menu entry, uninstall, confirm cleanup.
6. Run high-DPI (125%+) and high-contrast accessibility checks.

Log results in the release issue or PR.

## 6. Tagging & GitHub release

```cmd
git commit -am "Release v<version>"
git tag v<version>
git push origin master --tags
```

1. Draft release notes using `docs/RELEASE_NOTES_TEMPLATE.md`.
2. Upload artifacts from `artifacts/`:
   - `Differ-v<version>-portable-win-x64.zip`
   - `Differ_<version>_x64.msix`
   - `differ-signing-cert.cer`
   - `Install-Differ.bat`
   - `install-differ.ps1`
   - Any additional documentation or hashes
3. Publish the release once validation is complete.

## 7. Post-release follow-up

- Announce the release (GitHub Discussions, social media, internal channels).
- Create the next milestone and triage remaining issues.
- Monitor feedback, crashes, or installation problems.
- Update marketing assets/screenshots if needed.

## 8. Coverage expectations (snapshot)

From the latest review (Oct 2025):

- Total tests: 80 (all passing)
- Overall line coverage: ~40%
- `Differ.Core` line coverage: ~78% (target ≥90%)
- `Differ.Core` branch coverage: ~70%
- Gaps: `Differ.Common.Logging` helpers, `Differ.UI` view models, `DifferLogLevelManager`

Track these metrics when adding new features and list exceptions in release notes if coverage drops.

## 9. References

- [MSIX packaging guide](msix-packaging.md)
- [Certificates & signing](certificates.md)
- [QA checklist](../QA.md) – full matrix if you need the long form
- [Installation guide](../user-guide/installing-differ.md) – link in release notes for end users
- [Design guidelines](../DESIGN_GUIDELINES.md) – for contributor consistency
