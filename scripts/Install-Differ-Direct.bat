@echo off
REM Differ Installation Launcher (Direct Mode)
REM This version runs directly in the current window - easier to see what's happening

echo ================================================================
echo  Differ Installation Launcher (Direct Mode)
echo ================================================================
echo.
echo This will install Differ on your computer.
echo.
echo IMPORTANT: This script requires Administrator privileges!
echo.
echo If you haven't run this Command Prompt as Administrator:
echo   1. Close this window
echo   2. Right-click Command Prompt
echo   3. Select "Run as Administrator"
echo   4. Run this script again
echo.
pause

REM Get the directory where this batch file is located
set SCRIPT_DIR=%~dp0

echo.
echo Starting installation...
echo.

REM Run PowerShell script directly in this window
powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_DIR%install-differ.ps1"

echo.
echo ================================================================
echo.
pause
