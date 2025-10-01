param(
    [string]$PublishDir = "src/Differ.App/bin/Release/net8.0/win-x64/publish",
    [string]$OutputDir = "artifacts",
    [string]$PackageName = "csseeker.Differ",
    [string]$Publisher = "CN=Your Company",
    [string]$PublisherDisplayName = "Your Company",
    [string]$DisplayName = "Differ",
    [string]$Description = "Compare directories and files quickly.",
    [string]$ApplicationId = "Differ",
    [string]$Executable = "DifferApp.exe",
    [string]$Version = "1.0.0.0",
    [ValidateSet("x86", "x64", "arm64")]
    [string]$Architecture = "x64",
    [string]$MakeAppxPath = "makeappx.exe",
    [switch]$Sign,
    [string]$CertificatePath,
    [string]$CertificatePassword,
    [string]$SigntoolPath = "signtool.exe",
    [string]$TimestampUrl = "http://timestamp.digicert.com"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Resolve-Tool {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ToolName,
        [string]$HintPath
    )

    # 1. Check if explicit path was provided
    if (-not [string]::IsNullOrWhiteSpace($HintPath) -and (Test-Path $HintPath)) {
        return (Resolve-Path $HintPath).Path
    }

    # 2. Check if tool is in PATH
    $resolved = Get-Command $ToolName -ErrorAction SilentlyContinue
    if ($resolved) {
        return $resolved.Source
    }

    # 3. For makeappx.exe, search in common Windows SDK locations across all drives
    if ($ToolName -eq "makeappx.exe") {
        Write-Host "  Searching for makeappx.exe in Windows SDK locations..." -ForegroundColor Gray
        
        # Get all fixed drives (C:, D:, etc.)
        $drives = Get-PSDrive -PSProvider FileSystem | Where-Object { $_.Root -match '^[A-Z]:\\$' }
        
        # Common Windows SDK paths (relative to drive root)
        $commonPaths = @(
            "Program Files (x86)\Windows Kits\10\bin\*\x64\makeappx.exe",
            "Program Files\Windows Kits\10\bin\*\x64\makeappx.exe",
            "Windows Kits\10\bin\*\x64\makeappx.exe"
        )
        
        foreach ($drive in $drives) {
            foreach ($pathPattern in $commonPaths) {
                $fullPath = Join-Path $drive.Root $pathPattern
                $found = Get-ChildItem -Path $fullPath -ErrorAction SilentlyContinue | Select-Object -First 1
                if ($found) {
                    Write-Host "  [OK] Found makeappx.exe at: $($found.FullName)" -ForegroundColor Green
                    return $found.FullName
                }
            }
        }
    }

    throw "Unable to locate $ToolName. Ensure it is installed and in PATH, or provide an explicit path via script parameters."
}

if (-not (Test-Path $PublishDir)) {
    throw "Publish directory '$PublishDir' not found. Run 'dotnet publish -c Release -r win-x64 --self-contained false' first."
}

if ($Version -notmatch '^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$') {
    throw "Version '$Version' is not in the required four-part format (Major.Minor.Build.Revision)."
}

# Prepare directories
$createdOutputDir = New-Item -ItemType Directory -Path $OutputDir -Force
$resolvedOutputDir = (Resolve-Path -Path $createdOutputDir.FullName).ProviderPath
$stagingDir = Join-Path $resolvedOutputDir 'msix-staging'
if (Test-Path $stagingDir) {
    Remove-Item $stagingDir -Recurse -Force
}
New-Item -ItemType Directory -Path $stagingDir | Out-Null

# Copy published output
Write-Host "Copying published files to staging..."
Copy-Item -Path (Join-Path $PublishDir '*') -Destination $stagingDir -Recurse -Force

# Copy icons from source (single source of truth)
$assetsDir = Join-Path $stagingDir 'Assets'
if (-not (Test-Path $assetsDir)) {
    New-Item -ItemType Directory -Path $assetsDir | Out-Null
}

$sourceIconsDir = Join-Path $PSScriptRoot '..\src\Differ.Package\Images'
if (Test-Path $sourceIconsDir) {
    Write-Host "Copying icons from source: $sourceIconsDir"
    $iconFiles = @('Square150x150Logo.png', 'Square44x44Logo.png', 'Wide310x150Logo.png', 'StoreLogo.png')
    
    foreach ($iconFile in $iconFiles) {
        $sourcePath = Join-Path $sourceIconsDir $iconFile
        $destPath = Join-Path $assetsDir $iconFile
        
        if (Test-Path $sourcePath) {
            Copy-Item -Path $sourcePath -Destination $destPath -Force
            Write-Host "  [OK] Copied $iconFile" -ForegroundColor Green
        } else {
            Write-Host "  [!] Source icon not found: $sourcePath (will generate placeholder)" -ForegroundColor Yellow
        }
    }
} else {
    Write-Host "Source icons directory not found at $sourceIconsDir" -ForegroundColor Yellow
}

# Generate placeholder logos for any missing icons
try {
    Add-Type -AssemblyName System.Drawing
} catch {
    throw "System.Drawing assembly not available. Install the .NET Desktop Runtime or run the script on Windows PowerShell/PowerShell 7 with the Windows compatibility pack."
}

function New-Logo {
    param(
        [string]$Path,
        [int]$Width,
        [int]$Height,
        [string]$Text
    )

    $bitmap = New-Object System.Drawing.Bitmap $Width, $Height
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.Clear([System.Drawing.ColorTranslator]::FromHtml('#0078D4'))

    $fontSize = [Math]::Max([Math]::Min($Width, $Height) / 2, 12)
    $font = New-Object System.Drawing.Font('Segoe UI', $fontSize, [System.Drawing.FontStyle]::Bold)
    $brush = [System.Drawing.Brushes]::White

    $stringFormat = New-Object System.Drawing.StringFormat
    $stringFormat.Alignment = [System.Drawing.StringAlignment]::Center
    $stringFormat.LineAlignment = [System.Drawing.StringAlignment]::Center

    $graphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAlias
    $graphics.DrawString($Text, $font, $brush, (New-Object System.Drawing.RectangleF(0, 0, $Width, $Height)), $stringFormat)

    $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
    $graphics.Dispose()
    $bitmap.Dispose()
}

function Ensure-Logo {
    param(
        [string]$LogoName,
        [int]$Width,
        [int]$Height
    )

    $logoPath = Join-Path $assetsDir $LogoName
    if (-not (Test-Path $logoPath)) {
        Write-Host "  [!] Generating placeholder $LogoName ($Width x $Height)." -ForegroundColor Yellow
        $initialsSource = if ($DisplayName) { $DisplayName } else { 'DI' }
        $text = ($initialsSource.Substring(0, [Math]::Min(2, $initialsSource.Length))).ToUpper()
        New-Logo -Path $logoPath -Width $Width -Height $Height -Text $text
    }
}

Ensure-Logo -LogoName 'Square150x150Logo.png' -Width 150 -Height 150
Ensure-Logo -LogoName 'Square44x44Logo.png' -Width 44 -Height 44
Ensure-Logo -LogoName 'Wide310x150Logo.png' -Width 310 -Height 150
Ensure-Logo -LogoName 'StoreLogo.png' -Width 50 -Height 50

# Generate AppxManifest
$templatePath = Join-Path $PSScriptRoot '..\docs\assets\AppxManifest.sample.xml'
if (-not (Test-Path $templatePath)) {
    throw "Manifest template not found at '$templatePath'."
}

$manifestContent = Get-Content -Path $templatePath -Raw
$replacements = @{
    '{{PackageName}}'          = $PackageName
    '{{Architecture}}'         = $Architecture
    '{{Publisher}}'            = $Publisher
    '{{Version}}'              = $Version
    '{{DisplayName}}'          = $DisplayName
    '{{PublisherDisplayName}}' = $PublisherDisplayName
    '{{ApplicationId}}'        = $ApplicationId
    '{{Executable}}'           = $Executable
    '{{Description}}'          = $Description
}

foreach ($key in $replacements.Keys) {
    $manifestContent = $manifestContent.Replace($key, $replacements[$key])
}

$manifestPath = Join-Path $stagingDir 'AppxManifest.xml'
Set-Content -Path $manifestPath -Value $manifestContent -Encoding UTF8

# Locate tooling
$makeAppxExe = Resolve-Tool -ToolName 'makeappx.exe' -HintPath $MakeAppxPath

# Pack MSIX
$msixName = "{0}_{1}_{2}.msix" -f $ApplicationId, $Version, $Architecture
$msixPath = Join-Path $resolvedOutputDir $msixName
Write-Host "Packing MSIX to $msixPath"

& $makeAppxExe pack /d $stagingDir /p $msixPath /l /o | Write-Host

if ($Sign) {
    $signtoolExe = Resolve-Tool -ToolName 'signtool.exe' -HintPath $SigntoolPath
    if (-not $CertificatePath) {
        throw "CertificatePath is required when -Sign is specified."
    }
    if (-not (Test-Path $CertificatePath)) {
        throw "Certificate '$CertificatePath' not found."
    }

    $signArgs = @('sign', '/tr', $TimestampUrl, '/td', 'SHA256', '/fd', 'SHA256', '/f', $CertificatePath, $msixPath)
    if ($CertificatePassword) {
        $signArgs += '/p'
        $signArgs += $CertificatePassword
    }

    Write-Host "Signing MSIX package..."
    & $signtoolExe @signArgs | Write-Host

    Write-Host "Verifying signature..."
    & $signtoolExe verify /pa $msixPath | Write-Host
}

Write-Host "MSIX package created at $msixPath"
