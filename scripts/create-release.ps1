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

# =================================================================
#  LOGGING SETUP
# =================================================================
$ProjectRoot = $PSScriptRoot | Split-Path -Parent
$LogDir = Join-Path $ProjectRoot "logs"
if (-not (Test-Path $LogDir)) {
    New-Item -ItemType Directory -Path $LogDir -Force | Out-Null
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$LogFile = Join-Path $LogDir "release-$timestamp.log"

# Function to write messages with timestamps
function Write-LogMessage {
    param(
        [string]$Message,
        [string]$ForegroundColor = "White"
    )
    
    $timePrefix = "[$(Get-Date -Format 'MM/dd HH:mm:ss')]"
    $fullMessage = "$timePrefix $Message"
    
    if ($ForegroundColor -ne "White") {
        Write-Host $fullMessage -ForegroundColor $ForegroundColor
    } else {
        Write-Host $fullMessage
    }
}

# Start transcript to capture all output
Start-Transcript -Path $LogFile -Append

Write-LogMessage ""
Write-LogMessage "================================================" "Cyan"
Write-LogMessage "  Differ Release Script Execution" "Cyan"
Write-LogMessage "  Started: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" "Cyan"
Write-LogMessage "  Version: $Version" "Cyan"
Write-LogMessage "  Log file: $LogFile" "Cyan"
Write-LogMessage "================================================" "Cyan"
Write-LogMessage ""

# Define paths
$ProjectFile = Join-Path $ProjectRoot "src\Differ.App\Differ.App.csproj"
$OutputPath = Join-Path $ProjectRoot $OutputDir
$PublishDir = Join-Path $OutputPath "release-v$Version"
$ZipFileName = "Differ-v$Version-portable-win-x64.zip"
$ZipFilePath = Join-Path $OutputPath $ZipFileName
$CreateMsixScript = Join-Path $ProjectRoot "scripts\create-msix.ps1"
$RefreshAssetsScript = Join-Path $ProjectRoot "scripts\refresh-packaging-assets.ps1"
$ReleaseNotesDir = Join-Path $ProjectRoot "docs\Releases"
$ReleaseNotesFile = Join-Path $ReleaseNotesDir "v$Version-alpha.md"

Write-LogMessage ""
Write-LogMessage "================================================" "Cyan"
Write-LogMessage "  Differ Complete Release Build - v$Version" "Cyan"
Write-LogMessage "================================================" "Cyan"
Write-LogMessage ""

# STEP 1: Publish and Create ZIP
Write-LogMessage "[Step 1/5] Publishing and creating ZIP package..." "Yellow"
Write-LogMessage ""

if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
}

if (Test-Path $PublishDir) {
    Remove-Item -Path $PublishDir -Recurse -Force
    Write-LogMessage "  [+] Cleaned previous release folder" "Gray"
}

Write-LogMessage "  Publishing application..." "Gray"
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
Write-LogMessage "  [+] Published $($publishedFiles.Count) files" "Green"

# Give the file system a moment to settle
Start-Sleep -Milliseconds 500

Write-LogMessage "  Creating ZIP archive..." "Gray"
if (Test-Path $ZipFilePath) {
    Remove-Item -Path $ZipFilePath -Force
}

Compress-Archive -Path "$PublishDir\*" -DestinationPath $ZipFilePath -CompressionLevel Optimal -Force

$zipSize = (Get-Item $ZipFilePath).Length
$zipSizeMB = [math]::Round($zipSize / 1MB, 1)
Write-LogMessage "  [+] Created ZIP: $ZipFileName (Size: $zipSizeMB MB)" "Green"
Write-LogMessage ""

# STEP 2: Create MSIX Package
if (-not $SkipMsix) {
    Write-LogMessage "[Step 2/5] Creating MSIX package..." "Yellow"
    Write-LogMessage ""
    
    if (Test-Path $RefreshAssetsScript) {
        Write-LogMessage "  Refreshing packaging assets..." "Gray"
        & $RefreshAssetsScript 2>&1 | Out-Null
    }
    
    if (Test-Path $CreateMsixScript) {
        Write-LogMessage "  Creating MSIX package..." "Gray"
        
        # Convert version to 4-part format (e.g., 0.0.2 -> 0.0.2.0)
        # Also strip any suffix like -alpha, -beta, -test
        $cleanVersion = $Version -replace '-.*$', ''
        $versionParts = $cleanVersion.Split('.')
        $msixVersion = "$cleanVersion.0"
        if ($versionParts.Count -eq 4) {
            $msixVersion = $cleanVersion
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
            Write-LogMessage "  [+] Created MSIX: $msixFileName (Size: $msixSizeMB MB)" "Green"
        } else {
            Write-LogMessage "  [!] Warning: MSIX file not found at expected location" "Yellow"
        }
    } else {
        Write-LogMessage "  [!] Warning: create-msix.ps1 not found, skipping MSIX creation" "Yellow"
    }
    Write-LogMessage ""
} else {
    Write-LogMessage "[Step 2/5] Skipping MSIX (use -SkipMsix:`$false to enable)" "Gray"
    Write-LogMessage ""
}

# STEP 3: Create/Update Release Notes
Write-LogMessage "[Step 3/5] Creating release notes..." "Yellow"
Write-LogMessage ""

if (-not (Test-Path $ReleaseNotesDir)) {
    New-Item -ItemType Directory -Path $ReleaseNotesDir -Force | Out-Null
}

if (Test-Path $ReleaseNotesFile) {
    Write-LogMessage "  [i] Release notes already exist: v$Version-alpha.md" "Cyan"
    Write-LogMessage "  [i] You may want to update package sizes manually" "Cyan"
} else {
    Write-LogMessage "  Creating new release notes template..." "Gray"
    
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
    Write-LogMessage "  [+] Created release notes: v$Version-alpha.md" "Green"
    Write-LogMessage "  [!] Please update the release notes with actual changes!" "Yellow"
}
Write-LogMessage ""

# STEP 4: Cleanup
Write-LogMessage "[Step 4/5] Cleaning up..." "Yellow"
if (Test-Path $PublishDir) {
    Remove-Item -Path $PublishDir -Recurse -Force
    Write-LogMessage "  [+] Removed temporary publish directory" "Green"
}

# Clean up msix-staging if it exists
$msixStagingDir = Join-Path $OutputPath "msix-staging"
if (Test-Path $msixStagingDir) {
    Remove-Item -Path $msixStagingDir -Recurse -Force
    Write-LogMessage "  [+] Removed MSIX staging directory" "Green"
}
Write-LogMessage ""

# STEP 5: Summary
Write-LogMessage "[Step 5/5] Summary" "Yellow"
Write-LogMessage ""
Write-LogMessage "================================================" "Green"
Write-LogMessage "  Release Build Complete!" "Green"
Write-LogMessage "================================================" "Green"
Write-LogMessage ""
Write-LogMessage "Release Artifacts:" "Cyan"

# List all created artifacts
$artifacts = Get-ChildItem -Path $OutputPath -File | Where-Object { $_.Name -like "Differ*" }
foreach ($artifact in $artifacts) {
    $size = [math]::Round($artifact.Length / 1MB, 1)
    Write-LogMessage "  - $($artifact.Name) ($size MB)" "White"
}

Write-LogMessage ""
Write-LogMessage "Release Notes:" "Cyan"
Write-LogMessage "  - docs\Releases\v$Version-alpha.md" "White"
Write-LogMessage ""
Write-LogMessage "Location: $OutputPath" "Cyan"
Write-LogMessage ""
Write-LogMessage "Next Steps:" "Cyan"
Write-LogMessage "  1. Review and update release notes:" "Yellow"
Write-LogMessage "     $ReleaseNotesFile" "Gray"
Write-LogMessage "  2. Test the application by extracting and running Differ.App.exe" "Yellow"
Write-LogMessage "  3. Create and push Git tag:" "Yellow"
Write-LogMessage "     git tag v$Version" "Gray"
Write-LogMessage "     git push origin v$Version" "Gray"
Write-LogMessage "  4. Create GitHub Release at:" "Yellow"
Write-LogMessage "     https://github.com/csseeker/differ/releases/new" "Gray"
Write-LogMessage "  5. Upload all artifacts from: $OutputPath" "Yellow"
Write-LogMessage ""

# Stop transcript logging
Write-LogMessage ""
Write-LogMessage "================================================" "Green"
Write-LogMessage "  Release script completed successfully!" "Green"
Write-LogMessage "  Log saved to: $LogFile" "Cyan"
Write-LogMessage "================================================" "Green"
Write-LogMessage ""
Stop-Transcript
