param(
    [string]$OutputDirectory = "src/Differ.Package/Images",
    [string]$DisplayText = "Df"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Drawing

if (-not (Test-Path $OutputDirectory)) {
    New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
}

function New-Logo {
    param(
        [string]$Name,
        [int]$Width,
        [int]$Height
    )

    $path = Join-Path $OutputDirectory $Name
    $bitmap = New-Object System.Drawing.Bitmap($Width, $Height)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.Clear([System.Drawing.Color]::FromArgb(0, 120, 212))

    $fontSize = [Math]::Max([Math]::Min($Width, $Height) / 2, 12)
    $font = New-Object System.Drawing.Font('Segoe UI', $fontSize, [System.Drawing.FontStyle]::Bold)

    $stringFormat = New-Object System.Drawing.StringFormat
    $stringFormat.Alignment = [System.Drawing.StringAlignment]::Center
    $stringFormat.LineAlignment = [System.Drawing.StringAlignment]::Center

    $graphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAlias
    $graphics.DrawString($DisplayText, $font, [System.Drawing.Brushes]::White, (New-Object System.Drawing.RectangleF(0, 0, $Width, $Height)), $stringFormat)

    $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)

    $graphics.Dispose()
    $bitmap.Dispose()
}

New-Logo -Name 'Square150x150Logo.png' -Width 150 -Height 150
New-Logo -Name 'Square44x44Logo.png' -Width 44 -Height 44
New-Logo -Name 'Wide310x150Logo.png' -Width 310 -Height 150
New-Logo -Name 'StoreLogo.png' -Width 50 -Height 50
