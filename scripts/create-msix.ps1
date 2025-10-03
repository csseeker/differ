param(
    [string]$PublishDir = "src/Differ.App/bin/Release/net8.0/win-x64/publish",
    [string]$OutputDir = "artifacts",
    [string]$PackageName = "csseeker.Differ",
    [string]$Publisher = "CN=Your Company",
    [string]$PublisherDisplayName = "Your Company",
    [string]$DisplayName = "Differ",
    [string]$Description = "Compare directories and files quickly.",
    [string]$ApplicationId = "Differ",
    [string]$Executable = "Differ.App.exe",
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

    # 3. Search common Windows SDK locations across all drives for known tools
    $searchPatterns = @()
    switch ($ToolName.ToLowerInvariant()) {
        'makeappx.exe' {
            $searchPatterns = @(
                "Program Files (x86)\Windows Kits\10\bin\*\x64\makeappx.exe",
                "Program Files\Windows Kits\10\bin\*\x64\makeappx.exe",
                "Windows Kits\10\bin\*\x64\makeappx.exe"
            )
        }
        'signtool.exe' {
            $searchPatterns = @(
                # ONLY search Windows Kits 10 (modern versions that work)
                "Program Files (x86)\Windows Kits\10\bin\*\x64\signtool.exe",
                "Program Files\Windows Kits\10\bin\*\x64\signtool.exe",
                "Windows Kits\10\bin\*\x64\signtool.exe",
                "Program Files (x86)\Windows Kits\10\App Certification Kit\signtool.exe",
                "Program Files\Windows Kits\10\App Certification Kit\signtool.exe",
                "Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\*\x64\signtool.exe",
                "Program Files\Microsoft SDKs\Windows\v10.0A\bin\*\x64\signtool.exe"
                # NOTE: Explicitly NOT searching ClickOnce\SignTool - version 4.00 is too old and broken
            )
        }
    }

    if ($searchPatterns.Count -gt 0) {
        Write-Host "  Searching for $ToolName in Windows SDK locations..." -ForegroundColor Gray

        # Get all fixed drives (C:, D:, etc.)
        $drives = Get-PSDrive -PSProvider FileSystem | Where-Object { $_.Root -match '^[A-Z]:\\$' }

        foreach ($drive in $drives) {
            foreach ($pathPattern in $searchPatterns) {
                $fullPath = Join-Path $drive.Root $pathPattern
                $found = Get-ChildItem -Path $fullPath -File -ErrorAction SilentlyContinue |
                    Sort-Object LastWriteTime -Descending |
                    Select-Object -First 1
                if ($found) {
                    Write-Host "  [OK] Found $ToolName at: $($found.FullName)" -ForegroundColor Green
                    
                    # Check signtool version and warn if too old
                    if ($ToolName.ToLowerInvariant() -eq 'signtool.exe') {
                        $versionInfo = $found.VersionInfo
                        $version = $versionInfo.FileVersion
                        Write-Host "  Using signtool version $version" -ForegroundColor Gray
                        
                        # Extract SDK version from path (e.g., 10.0.26100.0)
                        if ($found.FullName -match '\\(\d+\.\d+\.\d+\.\d+)\\') {
                            $sdkVersion = $matches[1]
                            Write-Host "  SDK Version: $sdkVersion" -ForegroundColor Gray
                        }
                        
                        # Warn if it's the old ClickOnce version
                        if ($found.FullName -like "*ClickOnce*") {
                            Write-Host "  [WARNING] This is an OLD ClickOnce signtool (2015-2016) that may not work!" -ForegroundColor Yellow
                            Write-Host "            Continuing search for newer version..." -ForegroundColor Yellow
                            continue  # Keep searching for a better version
                        }
                    }
                    
                    return $found.FullName
                }
            }
        }
    }

    throw "Unable to locate $ToolName. Ensure it is installed and in PATH, or provide an explicit path via script parameters."
}

function Test-SigntoolVersion {
    param(
        [string]$SigntoolPath
    )
    
    $versionInfo = (Get-Item $SigntoolPath).VersionInfo
    $version = $versionInfo.FileVersion
    
    # Reject old versions that don't work with modern certificates
    if ($version -like "4.*" -or $version -like "10.0.10*") {
        return $false
    }
    
    return $true
}

function Get-VersionFromFileVersionInfo {
    param(
        [System.Diagnostics.FileVersionInfo]$VersionInfo
    )

    if ($null -eq $VersionInfo) {
        return $null
    }

    $numericCandidates = @(
        @{ Major = $VersionInfo.ProductMajorPart; Minor = $VersionInfo.ProductMinorPart; Build = $VersionInfo.ProductBuildPart; Private = $VersionInfo.ProductPrivatePart },
        @{ Major = $VersionInfo.FileMajorPart; Minor = $VersionInfo.FileMinorPart; Build = $VersionInfo.FileBuildPart; Private = $VersionInfo.FilePrivatePart }
    )

    foreach ($candidate in $numericCandidates) {
        if ($candidate.Major -gt 0) {
            $parts = @(
                [Math]::Max($candidate.Major, 0),
                [Math]::Max($candidate.Minor, 0),
                [Math]::Max($candidate.Build, 0),
                [Math]::Max($candidate.Private, 0)
            )
            return [Version]::new($parts[0], $parts[1], $parts[2], $parts[3])
        }
    }

    $stringCandidates = @($VersionInfo.ProductVersion, $VersionInfo.FileVersion) |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

    foreach ($candidate in $stringCandidates) {
        $match = [regex]::Match($candidate, '\d+(\.\d+){1,3}')
        if ($match.Success) {
            $parsed = $null
            if ([Version]::TryParse($match.Value, [ref]$parsed)) {
                return $parsed
            }
        }
    }

    return $null
}

function ConvertFrom-SecureStringToPlainText {
    param(
        [System.Security.SecureString]$SecureString
    )

    if ($null -eq $SecureString) {
        return $null
    }

    $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecureString)
    try {
        return [Runtime.InteropServices.Marshal]::PtrToStringUni($bstr)
    }
    finally {
        if ($bstr -ne [IntPtr]::Zero) {
            [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
        }
    }
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

# Suppress verbose "Its path in the package will be..." messages by filtering output
& $makeAppxExe pack /d $stagingDir /p $msixPath /l /o 2>&1 | Where-Object { 
    $_ -notmatch "Its path in the package will be" -and 
    $_ -notmatch "^\s*$" 
} | Write-Host

if ($Sign) {
    $signtoolExe = Resolve-Tool -ToolName 'signtool.exe' -HintPath $SigntoolPath
    try {
        $signtoolItem = Get-Item -Path $signtoolExe -ErrorAction Stop
    } catch {
        throw "Unable to retrieve signtool metadata from '$signtoolExe'. Install the Windows 10 SDK and try again. Details: $($_.Exception.Message)"
    }

    $versionInfo = $signtoolItem.VersionInfo
    $signtoolVersion = Get-VersionFromFileVersionInfo -VersionInfo $versionInfo
    if ($null -eq $signtoolVersion) {
        $reportedVersion = $versionInfo.FileVersion
        throw "Signtool at '$signtoolExe' does not expose a parsable version ('$reportedVersion'). Install the Windows 10 SDK (10.0.17763 or newer)."
    }
    Write-Host "  Using signtool version $signtoolVersion" -ForegroundColor Gray

    if ($signtoolVersion.Major -lt 10) {
        throw "Signtool version $signtoolVersion is too old for MSIX signing. Install the Windows 10 SDK (10.0.17763 or newer)."
    }
    if (-not $CertificatePath) {
        throw "CertificatePath is required when -Sign is specified."
    }
    if (-not (Test-Path $CertificatePath)) {
        throw "Certificate '$CertificatePath' not found."
    }

    $passwordToUse = $CertificatePassword
    if (-not $passwordToUse) {
        $envPassword = $env:DIFFER_CERT_PASSWORD
        if (-not [string]::IsNullOrWhiteSpace($envPassword)) {
            $passwordToUse = $envPassword
        }
    }

    # Try to load certificate without password first
    $needsPassword = $false
    try {
        $testCert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($CertificatePath, "")
        Write-Host "  Certificate loaded (no password required)" -ForegroundColor Gray
    } catch {
        $needsPassword = $true
    }

    # Only prompt for password if certificate actually needs one and we're interactive
    if ($needsPassword -and -not $passwordToUse -and [Environment]::UserInteractive) {
        Write-Host "Certificate password required." -ForegroundColor Gray
        $securePwd = Read-Host -Prompt 'Certificate password' -AsSecureString
        $plainPwd = ConvertFrom-SecureStringToPlainText -SecureString $securePwd
        if (-not [string]::IsNullOrWhiteSpace($plainPwd)) {
            $passwordToUse = $plainPwd
        }
    }

    Write-Host "Signing MSIX package..."
    
    $signArgs = @('sign', '/tr', $TimestampUrl, '/td', 'SHA256', '/fd', 'SHA256', '/f', $CertificatePath, $msixPath)
    if (-not [string]::IsNullOrWhiteSpace($passwordToUse)) {
        $signArgs += '/p'
        $signArgs += $passwordToUse
    }
    
    & $signtoolExe @signArgs | Write-Host
    
    if ($LASTEXITCODE -ne 0) {
        $errorMsg = "signtool failed to sign the package (Exit code: $LASTEXITCODE)"
        
        # Provide specific guidance based on common error codes
        if ($LASTEXITCODE -eq 1) {
            $errorMsg += "`n`n[COMMON CAUSES]"
            $errorMsg += "`n  1. Old signtool version (check version above)"
            $errorMsg += "`n     Solution: Install latest Windows 10 SDK"
            $errorMsg += "`n     Run: .\scripts\install-windows-sdk.ps1"
            $errorMsg += "`n`n  2. Certificate missing private key"
            $errorMsg += "`n     Solution: Recreate certificate"
            $errorMsg += "`n     Run: .\scripts\recreate-cert-quick.ps1"
            $errorMsg += "`n`n  3. Incorrect password (certificate has no password)"
            $errorMsg += "`n     Solution: Press ENTER when prompted for password"
        }
        
        throw $errorMsg
    }
    
    Write-Host "  [OK] Package signed successfully" -ForegroundColor Green
    
    # Verify signature
    Write-Host "Verifying signature..."
    & $signtoolExe verify /pa $msixPath | Write-Host
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  [OK] Signature verified" -ForegroundColor Green
    } else {
        Write-Host "  [WARNING] Signature verification returned exit code: $LASTEXITCODE" -ForegroundColor Yellow
        Write-Host "            The package may still be signed correctly." -ForegroundColor Gray
    }
}

Write-Host "MSIX package created at $msixPath"
