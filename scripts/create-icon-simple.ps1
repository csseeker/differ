Add-Type -AssemblyName System.Drawing
$outputDir = ".\icon-output"
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
$sizes = @(256, 48, 32, 16)
foreach ($size in $sizes) {
    $bitmap = New-Object System.Drawing.Bitmap($size, $size)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = 'AntiAlias'
    $bgColor = [System.Drawing.Color]::FromArgb(0, 120, 212)
    $fgColor = [System.Drawing.Color]::White
    $brush = New-Object System.Drawing.SolidBrush($bgColor)
    $graphics.FillRectangle($brush, 0, 0, $size, $size)
    $fontSize = [int]($size * 0.5)
    $font = New-Object System.Drawing.Font('Segoe UI', $fontSize, 'Bold')
    $textBrush = New-Object System.Drawing.SolidBrush($fgColor)
    $format = New-Object System.Drawing.StringFormat
    $format.Alignment = 'Center'
    $format.LineAlignment = 'Center'
    $rect = New-Object System.Drawing.RectangleF(0, 0, $size, $size)
    $graphics.DrawString('D', $font, $textBrush, $rect, $format)
    $outputPath = Join-Path $outputDir "differ-$size.png"
    $bitmap.Save($outputPath, 'Png')
    $graphics.Dispose()
    $bitmap.Dispose()
    Write-Host "Created: differ-$size.png"
}
Write-Host "Done! Files are in: $outputDir"
Start-Process $outputDir
