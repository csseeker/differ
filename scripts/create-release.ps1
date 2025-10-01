param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$OutputDir = "artifacts",
    [string]$Publisher = "CN=csseeker",
    [string]$PublisherDisplayName = "csseeker",
    [switch]$SkipMsix
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Define paths
$ProjectRoot = $PSScriptRoot | Split-Path -Parent
$ProjectFile = Join-Path $ProjectRoot "src\Differ.App\Differ.App.csproj"
$OutputPath = Join-Path $ProjectRoot $OutputDir
$PublishDir = Join-Path $OutputPath "release-v$Version"
$ZipFileName = "Differ-v$Version-portable-win-x64.zip"
$ZipFilePath = Join-Path $OutputPath $ZipFileName
$CreateMsixScript = Join-Path $ProjectRoot "scripts\create-msix.ps1"
$RefreshAssetsScript = Join-Path $ProjectRoot "scripts\refresh-packaging-assets.ps1"
$ReleaseNotesDir = Join-Path $ProjectRoot "docs\Releases"
$ReleaseNotesFile = Join-Path $ReleaseNotesDir "v$Version-alpha.md"

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Differ Complete Release Build - v$Version" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# STEP 1: Publish and Create ZIP
Write-Host "[Step 1/5] Publishing and creating ZIP package..." -ForegroundColor Yellow
Write-Host ""

if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
}

if (Test-Path $PublishDir) {
    Remove-Item -Path $PublishDir -Recurse -Force
    Write-Host "  [+] Cleaned previous release folder" -ForegroundColor Gray
}

Write-Host "  Publishing application..." -ForegroundColor Gray
dotnet publish $ProjectFile `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -o $PublishDir `
    /p:PublishSingleFile=false `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    --nologo `
    -v quiet

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}

$publishedFiles = Get-ChildItem -Path $PublishDir -File
Write-Host "  [+] Published $($publishedFiles.Count) files" -ForegroundColor Green

# Give the file system a moment to settle
Start-Sleep -Milliseconds 500

Write-Host "  Creating ZIP archive..." -ForegroundColor Gray
if (Test-Path $ZipFilePath) {
    Remove-Item -Path $ZipFilePath -Force
}

Compress-Archive -Path "$PublishDir\*" -DestinationPath $ZipFilePath -CompressionLevel Optimal -Force

$zipSize = (Get-Item $ZipFilePath).Length
$zipSizeMB = [math]::Round($zipSize / 1MB, 1)
Write-Host "  [+] Created ZIP: $ZipFileName (Size: $zipSizeMB MB)" -ForegroundColor Green
Write-Host ""

# STEP 2: Create MSIX Package
if (-not $SkipMsix) {
    Write-Host "[Step 2/5] Creating MSIX package..." -ForegroundColor Yellow
    Write-Host ""
    
    if (Test-Path $RefreshAssetsScript) {
        Write-Host "  Refreshing packaging assets..." -ForegroundColor Gray
        & $RefreshAssetsScript 2>&1 | Out-Null
    }
    
    if (Test-Path $CreateMsixScript) {
        Write-Host "  Creating MSIX package..." -ForegroundColor Gray
        
        # Convert version to 4-part format (e.g., 0.0.2 -> 0.0.2.0)
        $versionParts = $Version.Split('.')
        $msixVersion = "$Version.0"
        if ($versionParts.Count -eq 4) {
            $msixVersion = $Version
        }
        
        & $CreateMsixScript `
            -Version $msixVersion `
            -PackageName "csseeker.Differ" `
            -Publisher $Publisher `
            -PublisherDisplayName $PublisherDisplayName `
            -PublishDir $PublishDir 2>&1 | Out-Null
        
        $msixFileName = "Differ_$($msixVersion)_x64.msix"
        $msixFilePath = Join-Path $OutputPath $msixFileName
        
        if (Test-Path $msixFilePath) {
            $msixSize = (Get-Item $msixFilePath).Length
            $msixSizeMB = [math]::Round($msixSize / 1MB, 1)
            Write-Host "  [+] Created MSIX: $msixFileName (Size: $msixSizeMB MB)" -ForegroundColor Green
        } else {
            Write-Host "  [!] Warning: MSIX file not found at expected location" -ForegroundColor Yellow
        }
    } else {
        Write-Host "  [!] Warning: create-msix.ps1 not found, skipping MSIX creation" -ForegroundColor Yellow
    }
    Write-Host ""
} else {
    Write-Host "[Step 2/5] Skipping MSIX (use -SkipMsix:`$false to enable)" -ForegroundColor Gray
    Write-Host ""
}

# STEP 3: Create/Update Release Notes
Write-Host "[Step 3/5] Creating release notes..." -ForegroundColor Yellow
Write-Host ""

if (-not (Test-Path $ReleaseNotesDir)) {
    New-Item -ItemType Directory -Path $ReleaseNotesDir -Force | Out-Null
}

if (Test-Path $ReleaseNotesFile) {
    Write-Host "  [i] Release notes already exist: v$Version-alpha.md" -ForegroundColor Cyan
    Write-Host "  [i] You may want to update package sizes manually" -ForegroundColor Cyan
} else {
    Write-Host "  Creating new release notes template..." -ForegroundColor Gray
    
    $releaseDate = Get-Date -Format "MMMM d, yyyy"
    
    $releaseNotesTemplate = @"
# Release Notes - v$Version-alpha

**Release Date:** $releaseDate  
**Tag:** ``v$Version-alpha``

## Summary

[Add a brief summary of what's in this release]

## New Features

[List new features added in this release]

## Improvements

[List improvements and enhancements]

## Bug Fixes

[List bug fixes]

## Installation

### Portable Version (Recommended)

1. Download ``$ZipFileName`` ($zipSizeMB MB) from the release assets
2. Extract to any writable folder on your system
3. Run ``Differ.App.exe``
4. Windows SmartScreen may prompt you to confirm the first time - click "More info" then "Run anyway"

### MSIX Package (Windows Store Integration)

1. Download ``Differ_$($Version).0_x64.msix`` from the release assets
2. Double-click to install (requires developer mode or certificate installation)
3. Provides Start menu integration and automatic updates
4. See ``docs/MSIX_PACKAGING.md`` for detailed installation instructions

## System Requirements

- **OS:** Windows 10 version 1809 or later (Windows 11 recommended)
- **Architecture:** 64-bit (x64)
- **.NET Runtime:** Not required (self-contained)
- **Disk Space:** ~200 MB for extraction
- **Memory:** 2 GB RAM minimum, 4 GB recommended

## Known Issues

[List any known issues or limitations]

## Links

- [GitHub Repository](https://github.com/csseeker/differ)
- [Report an Issue](https://github.com/csseeker/differ/issues)
- [Documentation](https://github.com/csseeker/differ/tree/master/docs)
"@
    
    Set-Content -Path $ReleaseNotesFile -Value $releaseNotesTemplate
    Write-Host "  [+] Created release notes: v$Version-alpha.md" -ForegroundColor Green
    Write-Host "  [!] Please update the release notes with actual changes!" -ForegroundColor Yellow
}
Write-Host ""

# STEP 4: Cleanup
Write-Host "[Step 4/5] Cleaning up..." -ForegroundColor Yellow
if (Test-Path $PublishDir) {
    Remove-Item -Path $PublishDir -Recurse -Force
    Write-Host "  [+] Removed temporary publish directory" -ForegroundColor Green
}

# Clean up msix-staging if it exists
$msixStagingDir = Join-Path $OutputPath "msix-staging"
if (Test-Path $msixStagingDir) {
    Remove-Item -Path $msixStagingDir -Recurse -Force
    Write-Host "  [+] Removed MSIX staging directory" -ForegroundColor Green
}
Write-Host ""

# STEP 5: Summary
Write-Host "[Step 5/5] Summary" -ForegroundColor Yellow
Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "  Release Build Complete!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""
Write-Host "Release Artifacts:" -ForegroundColor Cyan

# List all created artifacts
$artifacts = Get-ChildItem -Path $OutputPath -File | Where-Object { $_.Name -like "Differ*" }
foreach ($artifact in $artifacts) {
    $size = [math]::Round($artifact.Length / 1MB, 1)
    Write-Host "  - $($artifact.Name) ($size MB)" -ForegroundColor White
}

Write-Host ""
Write-Host "Release Notes:" -ForegroundColor Cyan
Write-Host "  - docs\Releases\v$Version-alpha.md" -ForegroundColor White
Write-Host ""
Write-Host "Location: $OutputPath" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "  1. Review and update release notes:" -ForegroundColor Yellow
Write-Host "     $ReleaseNotesFile" -ForegroundColor Gray
Write-Host "  2. Test the application by extracting and running Differ.App.exe" -ForegroundColor Yellow
Write-Host "  3. Create and push Git tag:" -ForegroundColor Yellow
Write-Host "     git tag v$Version" -ForegroundColor Gray
Write-Host "     git push origin v$Version" -ForegroundColor Gray
Write-Host "  4. Create GitHub Release at:" -ForegroundColor Yellow
Write-Host "     https://github.com/csseeker/differ/releases/new" -ForegroundColor Gray
Write-Host "  5. Upload all artifacts from: $OutputPath" -ForegroundColor Yellow
Write-Host ""
