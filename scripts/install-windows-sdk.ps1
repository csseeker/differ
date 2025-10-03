# Install Windows 10 SDK with modern signtool
# This script helps you install the latest Windows SDK

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "Windows SDK Installation Helper" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host ""

Write-Host "PROBLEM:" -ForegroundColor Yellow
Write-Host "  The signtool.exe on your system is too old (version 4.00 from 2016)" -ForegroundColor White
Write-Host "  It cannot sign modern certificates with SHA256 properly" -ForegroundColor White
Write-Host ""

Write-Host "SOLUTION:" -ForegroundColor Green
Write-Host "  Install the latest Windows 10 SDK" -ForegroundColor White
Write-Host ""

Write-Host "INSTALLATION OPTIONS:" -ForegroundColor Cyan
Write-Host ""

Write-Host "[Option 1] Install via winget (Recommended - Fastest)" -ForegroundColor Yellow
Write-Host "  Run this command:" -ForegroundColor White
Write-Host "    winget install --id Microsoft.WindowsSDK" -ForegroundColor Gray
Write-Host ""

Write-Host "[Option 2] Install via web download" -ForegroundColor Yellow
Write-Host "  1. Visit: https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/" -ForegroundColor White
Write-Host "  2. Click 'Download the installer'" -ForegroundColor White
Write-Host "  3. Run the installer" -ForegroundColor White
Write-Host "  4. Select these components:" -ForegroundColor White
Write-Host "     - Windows SDK Signing Tools for Desktop Apps" -ForegroundColor Gray
Write-Host "     - Windows App Certification Kit" -ForegroundColor Gray
Write-Host ""

Write-Host "[Option 3] Install Visual Studio Build Tools (includes SDK)" -ForegroundColor Yellow
Write-Host "  1. Visit: https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2022" -ForegroundColor White
Write-Host "  2. Download 'Build Tools for Visual Studio 2022'" -ForegroundColor White
Write-Host "  3. Run installer and select '.NET desktop build tools'" -ForegroundColor White
Write-Host ""

Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host ""

$answer = Read-Host "Do you want to try installing via winget now? (y/n)"
if ($answer -eq 'y' -or $answer -eq 'Y') {
    Write-Host ""
    Write-Host "Checking if winget is available..." -ForegroundColor Cyan
    
    $winget = Get-Command winget -ErrorAction SilentlyContinue
    if ($winget) {
        Write-Host "[OK] winget found" -ForegroundColor Green
        Write-Host ""
        Write-Host "Installing Windows SDK..." -ForegroundColor Yellow
        Write-Host "This may take several minutes..." -ForegroundColor Gray
        Write-Host ""
        
        try {
            & winget install --id Microsoft.WindowsSDK --accept-source-agreements --accept-package-agreements
            
            Write-Host ""
            Write-Host "[OK] Installation complete!" -ForegroundColor Green
            Write-Host ""
            Write-Host "Now run:" -ForegroundColor Cyan
            Write-Host "  .\scripts\find-signtool.ps1" -ForegroundColor Gray
            Write-Host ""
            Write-Host "Then try your release again:" -ForegroundColor Cyan
            Write-Host "  .\scripts\create-release.ps1 -Version '0.1.2'" -ForegroundColor Gray
            Write-Host ""
        } catch {
            Write-Host "[ERROR] Installation failed: $($_.Exception.Message)" -ForegroundColor Red
            Write-Host ""
            Write-Host "Try Option 2 or 3 instead (manual download)" -ForegroundColor Yellow
        }
    } else {
        Write-Host "[ERROR] winget not found on your system" -ForegroundColor Red
        Write-Host ""
        Write-Host "Use Option 2 or 3 instead (manual download)" -ForegroundColor Yellow
    }
} else {
    Write-Host ""
    Write-Host "Please install Windows SDK manually using Option 2 or 3 above" -ForegroundColor Yellow
    Write-Host ""
}

Write-Host "After installation, verify with:" -ForegroundColor Cyan
Write-Host "  .\scripts\find-signtool.ps1" -ForegroundColor Gray
Write-Host "  .\scripts\test-signtool-compatibility.ps1" -ForegroundColor Gray
Write-Host ""
