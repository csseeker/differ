# Find and configure signtool.exe
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "Searching for signtool.exe on your system..." -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host ""

# Search common locations across all drives
$searchPatterns = @(
    "Program Files (x86)\Windows Kits\10\bin\*\x64\signtool.exe",
    "Program Files\Windows Kits\10\bin\*\x64\signtool.exe",
    "Program Files (x86)\Windows Kits\10\App Certification Kit\signtool.exe",
    "Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\*\x64\signtool.exe",
    "Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\signtool.exe"
)

Write-Host "Searching across all fixed drives..." -ForegroundColor Gray
$drives = Get-PSDrive -PSProvider FileSystem | Where-Object { $_.Root -match '^[A-Z]:\\$' }
Write-Host "Drives to search: $($drives.Name -join ', ')" -ForegroundColor Gray
Write-Host ""

$foundTools = @()
foreach ($drive in $drives) {
    foreach ($pattern in $searchPatterns) {
        $fullPath = Join-Path $drive.Root $pattern
        $found = Get-ChildItem -Path $fullPath -File -ErrorAction SilentlyContinue |
            Sort-Object LastWriteTime -Descending
        if ($found) {
            $foundTools += $found
        }
    }
}

if ($foundTools.Count -eq 0) {
    Write-Host "[!] signtool.exe NOT FOUND on your system" -ForegroundColor Red
    Write-Host ""
    Write-Host "You need to install Windows 10 SDK:" -ForegroundColor Yellow
    Write-Host "  1. Visit: https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/" -ForegroundColor White
    Write-Host "  2. Download and run the installer" -ForegroundColor White
    Write-Host "  3. Select: 'Windows SDK Signing Tools for Desktop Apps'" -ForegroundColor White
    Write-Host ""
    Write-Host "Or install via winget:" -ForegroundColor Yellow
    Write-Host "  winget install Microsoft.WindowsSDK" -ForegroundColor Gray
    Write-Host ""
    exit 1
}

Write-Host "[OK] Found $($foundTools.Count) signtool.exe installation(s):" -ForegroundColor Green
Write-Host ""

for ($i = 0; $i -lt $foundTools.Count; $i++) {
    $tool = $foundTools[$i]
    $versionInfo = $tool.VersionInfo
    Write-Host "[$($i + 1)] $($tool.FullName)" -ForegroundColor Cyan
    Write-Host "    Version: $($versionInfo.FileVersion)" -ForegroundColor Gray
    Write-Host "    Modified: $($tool.LastWriteTime)" -ForegroundColor Gray
    Write-Host ""
}

# Select the most recent one
$selectedTool = $foundTools[0]
Write-Host "[Recommended] Using most recent version:" -ForegroundColor Yellow
Write-Host "  $($selectedTool.FullName)" -ForegroundColor White
Write-Host ""

# Check if already in PATH
$signtoolInPath = Get-Command signtool.exe -ErrorAction SilentlyContinue
if ($signtoolInPath) {
    Write-Host "[OK] signtool.exe is already in your PATH" -ForegroundColor Green
    Write-Host "    Location: $($signtoolInPath.Source)" -ForegroundColor Gray
    Write-Host ""
} else {
    Write-Host "[INFO] signtool.exe is NOT in your PATH" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "OPTIONS:" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Option 1: Use -SigntoolPath parameter (Recommended)" -ForegroundColor White
    Write-Host "  When running create-msix.ps1, use:" -ForegroundColor Gray
    Write-Host "  -SigntoolPath '$($selectedTool.FullName)'" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "Option 2: Add to PATH permanently" -ForegroundColor White
    Write-Host "  Run this command as Administrator:" -ForegroundColor Gray
    $toolDir = Split-Path $selectedTool.FullName
    Write-Host "  [Environment]::SetEnvironmentVariable('Path', `$env:Path + ';$toolDir', 'Machine')" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "Option 3: Add to PATH for current session" -ForegroundColor White
    Write-Host "  Run this command:" -ForegroundColor Gray
    Write-Host "  `$env:Path += ';$toolDir'" -ForegroundColor DarkGray
    Write-Host ""
}

Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "To proceed with MSIX signing:" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host ""
Write-Host "Run your release script with:" -ForegroundColor White
Write-Host "  .\scripts\create-release.ps1 -Version '0.1.1'" -ForegroundColor Gray
Write-Host ""
Write-Host "The script will automatically find signtool.exe" -ForegroundColor Gray
Write-Host ""
