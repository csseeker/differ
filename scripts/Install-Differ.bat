@echo off
REM Differ Complete Installation Launcher
REM This batch file launches the PowerShell installation script with Administrator privileges

echo ================================================================
echo  Differ Installation Launcher
echo ================================================================
echo.
echo This will install Differ on your computer.
echo.
echo You will be asked for Administrator permission.
echo.
pause

REM Get the directory where this batch file is located
set SCRIPT_DIR=%~dp0

echo.
echo Launching installer with Administrator privileges...
echo.
echo IMPORTANT:
echo  - Click "Yes" when Windows asks for permission
echo  - A new PowerShell window will open (look for it!)
echo  - If you don't see it, check behind this window or in your taskbar
echo.

REM Launch PowerShell script as Administrator with WindowStyle parameter
powershell -Command "Start-Process powershell -Verb RunAs -ArgumentList '-NoExit','-ExecutionPolicy','Bypass','-File','\"%SCRIPT_DIR%install-differ.ps1\"' -WindowStyle Normal"

echo.
echo Installation script launched!
echo.
echo Look for the blue PowerShell window that just opened.
echo If you don't see it, check your taskbar or behind other windows.
echo.
echo You can close this window now.
echo.
pause
