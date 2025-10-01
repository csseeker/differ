# Test Icon Copy Functionality
# This script tests the icon copying logic without requiring a full build

param(
    [string]$SourceDir = "src/Differ.Package/Images",
    [string]$DestDir = "artifacts/test-icon-staging/Assets"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Write-Host "=== Testing Icon Copy Functionality ===" -ForegroundColor Cyan
Write-Host ""

# Create destination directory
if (Test-Path $DestDir) {
    Remove-Item $DestDir -Recurse -Force
}
New-Item -ItemType Directory -Path $DestDir -Force | Out-Null
Write-Host "[SETUP] Created test destination: $DestDir" -ForegroundColor Gray
Write-Host ""

# Test icon copying
$sourceIconsDir = $SourceDir
if (Test-Path $sourceIconsDir) {
    Write-Host "Copying icons from source: $sourceIconsDir" -ForegroundColor White
    $iconFiles = @('Square150x150Logo.png', 'Square44x44Logo.png', 'Wide310x150Logo.png', 'StoreLogo.png')
    
    $copiedCount = 0
    $missingCount = 0
    
    foreach ($iconFile in $iconFiles) {
        $sourcePath = Join-Path $sourceIconsDir $iconFile
        $destPath = Join-Path $DestDir $iconFile
        
        if (Test-Path $sourcePath) {
            Copy-Item -Path $sourcePath -Destination $destPath -Force
            Write-Host "  [OK] Copied $iconFile" -ForegroundColor Green
            $copiedCount++
        } else {
            Write-Host "  [!] Source icon not found: $iconFile" -ForegroundColor Yellow
            $missingCount++
        }
    }
    
    Write-Host ""
    Write-Host "=== Summary ===" -ForegroundColor Cyan
    Write-Host "  Copied: $copiedCount icons" -ForegroundColor Green
    Write-Host "  Missing: $missingCount icons" -ForegroundColor $(if ($missingCount -gt 0) { "Yellow" } else { "Gray" })
    Write-Host ""
    
    if ($copiedCount -gt 0) {
        Write-Host "Destination files:" -ForegroundColor White
        Get-ChildItem $DestDir | ForEach-Object {
            Write-Host "  - $($_.Name) ($([math]::Round($_.Length / 1KB, 2)) KB)" -ForegroundColor Gray
        }
    }
    
    Write-Host ""
    Write-Host "[SUCCESS] Icon copy test completed!" -ForegroundColor Green
    
} else {
    Write-Host "[ERROR] Source icons directory not found at $sourceIconsDir" -ForegroundColor Red
    exit 1
}
