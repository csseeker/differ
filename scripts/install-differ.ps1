# Differ MSIX Complete Installation Script
# This script installs the certificate and then the MSIX package in one step
# Requires Administrator privileges

param(
    [string]$MsixPath = "",
    [string]$CertPath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ASCII Art Banner
$banner = @"
╔══════════════════════════════════════════════════════════════╗
║                                                              ║
║   ██████╗ ██╗███████╗███████╗███████╗██████╗               ║
║   ██╔══██╗██║██╔════╝██╔════╝██╔════╝██╔══██╗              ║
║   ██║  ██║██║█████╗  █████╗  █████╗  ██████╔╝              ║
║   ██║  ██║██║██╔══╝  ██╔══╝  ██╔══╝  ██╔══██╗              ║
║   ██████╔╝██║██║     ██║     ███████╗██║  ██║              ║
║   ╚═════╝ ╚═╝╚═╝     ╚═╝     ╚══════╝╚═╝  ╚═╝              ║
║                                                              ║
║           Complete Installation Script v1.0                  ║
║                                                              ║
╚══════════════════════════════════════════════════════════════╝
"@

Write-Host $banner -ForegroundColor Cyan
Write-Host ""

# Check for Administrator privileges
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Red
    Write-Host " ERROR: This script must be run as Administrator" -ForegroundColor Red
    Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Red
    Write-Host ""
    Write-Host "How to run as Administrator:" -ForegroundColor Yellow
    Write-Host "  1. Right-click PowerShell" -ForegroundColor White
    Write-Host "  2. Select 'Run as Administrator'" -ForegroundColor White
    Write-Host "  3. Navigate to this folder and run the script again" -ForegroundColor White
    Write-Host ""
    Write-Host "Quick command:" -ForegroundColor Yellow
    Write-Host "  Start-Process powershell -Verb RunAs -ArgumentList `"-ExecutionPolicy Bypass -File `"$PSCommandPath`"`"" -ForegroundColor Gray
    Write-Host ""
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "[✓] Running with Administrator privileges" -ForegroundColor Green
Write-Host ""

# Discover files if not specified
$scriptDir = Split-Path -Parent $PSCommandPath

if ([string]::IsNullOrWhiteSpace($CertPath)) {
    Write-Host "[*] Looking for certificate file..." -ForegroundColor Cyan
    $certFiles = Get-ChildItem -Path $scriptDir -Filter "*.cer" -ErrorAction SilentlyContinue
    if ($certFiles) {
        $CertPath = $certFiles[0].FullName
        Write-Host "[✓] Found: $($certFiles[0].Name)" -ForegroundColor Green
    } else {
        Write-Host "[!] No .cer certificate file found in current directory" -ForegroundColor Red
        $CertPath = Read-Host "Please enter the full path to the certificate file"
    }
}

if ([string]::IsNullOrWhiteSpace($MsixPath)) {
    Write-Host "[*] Looking for MSIX package..." -ForegroundColor Cyan
    $msixFiles = Get-ChildItem -Path $scriptDir -Filter "*.msix" -ErrorAction SilentlyContinue
    if ($msixFiles) {
        $MsixPath = $msixFiles[0].FullName
        Write-Host "[✓] Found: $($msixFiles[0].Name)" -ForegroundColor Green
    } else {
        Write-Host "[!] No .msix package file found in current directory" -ForegroundColor Red
        $MsixPath = Read-Host "Please enter the full path to the MSIX file"
    }
}

Write-Host ""

# Validate files exist
if (-not (Test-Path $CertPath)) {
    Write-Host "[✗] Certificate file not found: $CertPath" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

if (-not (Test-Path $MsixPath)) {
    Write-Host "[✗] MSIX package not found: $MsixPath" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " Installation Plan" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Certificate: " -NoNewline -ForegroundColor White
Write-Host (Split-Path -Leaf $CertPath) -ForegroundColor Yellow
Write-Host "  MSIX Package: " -NoNewline -ForegroundColor White
Write-Host (Split-Path -Leaf $MsixPath) -ForegroundColor Yellow
Write-Host ""
Write-Host "  This script will:" -ForegroundColor White
Write-Host "    1. Install the code signing certificate to Trusted Root" -ForegroundColor Gray
Write-Host "    2. Install the code signing certificate to Trusted People" -ForegroundColor Gray
Write-Host "    3. Install the Differ MSIX package" -ForegroundColor Gray
Write-Host "    4. Verify the installation" -ForegroundColor Gray
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$response = Read-Host "Do you want to continue? (Y/N)"
if ($response -ne 'Y' -and $response -ne 'y') {
    Write-Host "[*] Installation cancelled by user" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 0
}

Write-Host ""

# ============================================================================
# STEP 1: Install Certificate
# ============================================================================

Write-Host "────────────────────────────────────────────────────────────────" -ForegroundColor DarkCyan
Write-Host " Step 1/3: Installing Certificate" -ForegroundColor Cyan
Write-Host "────────────────────────────────────────────────────────────────" -ForegroundColor DarkCyan
Write-Host ""

try {
    # Load the certificate
    $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($CertPath)
    
    Write-Host "[*] Certificate Details:" -ForegroundColor Cyan
    Write-Host "    Subject:    $($cert.Subject)" -ForegroundColor Gray
    Write-Host "    Issuer:     $($cert.Issuer)" -ForegroundColor Gray
    Write-Host "    Valid From: $($cert.NotBefore)" -ForegroundColor Gray
    Write-Host "    Valid To:   $($cert.NotAfter)" -ForegroundColor Gray
    Write-Host "    Thumbprint: $($cert.Thumbprint)" -ForegroundColor Gray
    Write-Host ""
    
    # Check if already installed
    $existingCert = Get-ChildItem -Path Cert:\LocalMachine\Root | Where-Object { $_.Thumbprint -eq $cert.Thumbprint }
    
    if ($existingCert) {
        Write-Host "[*] Certificate already installed in Trusted Root" -ForegroundColor Yellow
    } else {
        Write-Host "[*] Installing to Trusted Root Certificate Authorities..." -ForegroundColor Cyan
        $rootStore = New-Object System.Security.Cryptography.X509Certificates.X509Store(
            [System.Security.Cryptography.X509Certificates.StoreName]::Root,
            [System.Security.Cryptography.X509Certificates.StoreLocation]::LocalMachine
        )
        $rootStore.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
        $rootStore.Add($cert)
        $rootStore.Close()
        Write-Host "[✓] Certificate installed to Trusted Root" -ForegroundColor Green
    }
    
    # Also install to Trusted People
    $existingPeopleCert = Get-ChildItem -Path Cert:\LocalMachine\TrustedPeople | Where-Object { $_.Thumbprint -eq $cert.Thumbprint }
    
    if ($existingPeopleCert) {
        Write-Host "[*] Certificate already installed in Trusted People" -ForegroundColor Yellow
    } else {
        Write-Host "[*] Installing to Trusted People..." -ForegroundColor Cyan
        $peopleStore = New-Object System.Security.Cryptography.X509Certificates.X509Store(
            [System.Security.Cryptography.X509Certificates.StoreName]::TrustedPeople,
            [System.Security.Cryptography.X509Certificates.StoreLocation]::LocalMachine
        )
        $peopleStore.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
        $peopleStore.Add($cert)
        $peopleStore.Close()
        Write-Host "[✓] Certificate installed to Trusted People" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "[✓] Certificate installation complete" -ForegroundColor Green
    
} catch {
    Write-Host ""
    Write-Host "[✗] Failed to install certificate" -ForegroundColor Red
    Write-Host "    Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host ""

# ============================================================================
# STEP 2: Install MSIX Package
# ============================================================================

Write-Host "────────────────────────────────────────────────────────────────" -ForegroundColor DarkCyan
Write-Host " Step 2/3: Installing MSIX Package" -ForegroundColor Cyan
Write-Host "────────────────────────────────────────────────────────────────" -ForegroundColor DarkCyan
Write-Host ""

try {
    # Check if already installed
    $packageName = (Split-Path -Leaf $MsixPath) -replace '_.*\.msix$', ''
    $existingPackage = Get-AppxPackage -Name "*$packageName*" -ErrorAction SilentlyContinue
    
    if ($existingPackage) {
        Write-Host "[*] Differ is already installed (version $($existingPackage.Version))" -ForegroundColor Yellow
        Write-Host ""
        $updateResponse = Read-Host "Do you want to update/reinstall? (Y/N)"
        if ($updateResponse -ne 'Y' -and $updateResponse -ne 'y') {
            Write-Host "[*] Skipping MSIX installation" -ForegroundColor Yellow
        } else {
            Write-Host "[*] Removing existing installation..." -ForegroundColor Cyan
            Remove-AppxPackage -Package $existingPackage.PackageFullName
            Write-Host "[✓] Removed existing installation" -ForegroundColor Green
            Write-Host ""
            Write-Host "[*] Installing MSIX package..." -ForegroundColor Cyan
            Add-AppxPackage -Path $MsixPath
            Write-Host "[✓] MSIX package installed successfully" -ForegroundColor Green
        }
    } else {
        Write-Host "[*] Installing MSIX package..." -ForegroundColor Cyan
        Add-AppxPackage -Path $MsixPath
        Write-Host "[✓] MSIX package installed successfully" -ForegroundColor Green
    }
    
} catch {
    Write-Host ""
    Write-Host "[✗] Failed to install MSIX package" -ForegroundColor Red
    Write-Host "    Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Possible reasons:" -ForegroundColor Yellow
    Write-Host "  - The certificate might not be properly trusted yet (restart may be needed)" -ForegroundColor Gray
    Write-Host "  - The MSIX package might be corrupted" -ForegroundColor Gray
    Write-Host "  - Windows version might not support MSIX packages" -ForegroundColor Gray
    Write-Host ""
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host ""

# ============================================================================
# STEP 3: Verify Installation
# ============================================================================

Write-Host "────────────────────────────────────────────────────────────────" -ForegroundColor DarkCyan
Write-Host " Step 3/3: Verifying Installation" -ForegroundColor Cyan
Write-Host "────────────────────────────────────────────────────────────────" -ForegroundColor DarkCyan
Write-Host ""

try {
    $installedPackage = Get-AppxPackage -Name "*Differ*" -ErrorAction SilentlyContinue
    
    if ($installedPackage) {
        Write-Host "[✓] Differ is installed" -ForegroundColor Green
        Write-Host ""
        Write-Host "    Package Name:    $($installedPackage.Name)" -ForegroundColor Gray
        Write-Host "    Version:         $($installedPackage.Version)" -ForegroundColor Gray
        Write-Host "    Install Location: $($installedPackage.InstallLocation)" -ForegroundColor Gray
        Write-Host ""
    } else {
        Write-Host "[!] Could not verify installation" -ForegroundColor Yellow
    }
    
} catch {
    Write-Host "[!] Verification failed: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host ""

# ============================================================================
# Success!
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host " ✓ Installation Complete!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""
Write-Host "Differ has been installed successfully!" -ForegroundColor White
Write-Host ""
Write-Host "How to launch Differ:" -ForegroundColor Cyan
Write-Host "  • Press Windows key and type 'Differ'" -ForegroundColor White
Write-Host "  • Or run 'differ' from Command Prompt/PowerShell" -ForegroundColor White
Write-Host ""
Write-Host "Need help?" -ForegroundColor Cyan
Write-Host "  • Documentation: https://github.com/csseeker/differ" -ForegroundColor White
Write-Host "  • Report issues: https://github.com/csseeker/differ/issues" -ForegroundColor White
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""

Read-Host "Press Enter to exit"

