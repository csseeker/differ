# Create ICO file from PNG files
# This script combines multiple PNG sizes into a single .ico file

$outputPath = "src\Differ.App\Resources\differ.ico"

# Ensure the Resources directory exists
$resourcesDir = "src\Differ.App\Resources"
if (-not (Test-Path $resourcesDir)) {
    New-Item -ItemType Directory -Path $resourcesDir -Force | Out-Null
    Write-Host "Created directory: $resourcesDir" -ForegroundColor Green
}

# Check if we have ImageMagick (magick.exe)
$hasMagick = Get-Command magick -ErrorAction SilentlyContinue

if ($hasMagick) {
    Write-Host "Using ImageMagick to create ICO file..." -ForegroundColor Cyan
    
    # Use the 256px icon as source
    $sourceIcon = "icon-output\differ-256.png"
    
    if (Test-Path $sourceIcon) {
        magick convert $sourceIcon -define icon:auto-resize=256,48,32,16 $outputPath
        
        if (Test-Path $outputPath) {
            Write-Host "SUCCESS: Created $outputPath" -ForegroundColor Green
            Write-Host "ICO file contains sizes: 16x16, 32x32, 48x48, 256x256" -ForegroundColor Green
        } else {
            Write-Host "ERROR: Failed to create ICO file" -ForegroundColor Red
        }
    } else {
        Write-Host "ERROR: Source icon not found: $sourceIcon" -ForegroundColor Red
    }
} else {
    Write-Host "ImageMagick not found. Using .NET to create basic ICO..." -ForegroundColor Yellow
    
    # Use .NET to create a basic ICO file
    Add-Type -AssemblyName System.Drawing
    
    $iconSizes = @(16, 32, 48)
    $sourceFiles = @(
        "icon-output\differ-16.png",
        "icon-output\differ-32.png",
        "icon-output\differ-48.png"
    )
    
    # Check if all source files exist
    $allExist = $true
    foreach ($file in $sourceFiles) {
        if (-not (Test-Path $file)) {
            Write-Host "ERROR: Missing file: $file" -ForegroundColor Red
            $allExist = $false
        }
    }
    
    if ($allExist) {
        try {
            # Load images
            $images = @()
            foreach ($file in $sourceFiles) {
                $img = [System.Drawing.Image]::FromFile((Resolve-Path $file).Path)
                $images += $img
            }
            
            # Create icon from multiple images
            $iconStream = New-Object System.IO.MemoryStream
            $iconWriter = New-Object System.IO.BinaryWriter($iconStream)
            
            # ICO header
            $iconWriter.Write([UInt16]0)  # Reserved
            $iconWriter.Write([UInt16]1)  # Type (1 = ICO)
            $iconWriter.Write([UInt16]$images.Count)  # Number of images
            
            # Calculate offsets
            $headerSize = 6 + ($images.Count * 16)
            $offset = $headerSize
            
            # Write directory entries
            $imageData = @()
            for ($i = 0; $i -lt $images.Count; $i++) {
                $img = $images[$i]
                $size = $iconSizes[$i]
                
                # Convert to PNG in memory
                $pngStream = New-Object System.IO.MemoryStream
                $img.Save($pngStream, [System.Drawing.Imaging.ImageFormat]::Png)
                $pngBytes = $pngStream.ToArray()
                $imageData += $pngBytes
                
                # Directory entry
                $iconWriter.Write([Byte]$size)           # Width
                $iconWriter.Write([Byte]$size)           # Height
                $iconWriter.Write([Byte]0)               # Colors (0 = no palette)
                $iconWriter.Write([Byte]0)               # Reserved
                $iconWriter.Write([UInt16]1)             # Planes
                $iconWriter.Write([UInt16]32)            # Bits per pixel
                $iconWriter.Write([UInt32]$pngBytes.Length)  # Size
                $iconWriter.Write([UInt32]$offset)       # Offset
                
                $offset += $pngBytes.Length
                $pngStream.Dispose()
            }
            
            # Write image data
            foreach ($data in $imageData) {
                $iconWriter.Write($data)
            }
            
            # Save to file
            $iconBytes = $iconStream.ToArray()
            [System.IO.File]::WriteAllBytes((Join-Path (Get-Location) $outputPath), $iconBytes)
            
            # Cleanup
            $iconWriter.Dispose()
            $iconStream.Dispose()
            foreach ($img in $images) {
                $img.Dispose()
            }
            
            if (Test-Path $outputPath) {
                Write-Host "SUCCESS: Created $outputPath using .NET" -ForegroundColor Green
                Write-Host "ICO file contains sizes: 16x16, 32x32, 48x48" -ForegroundColor Green
            }
        } catch {
            Write-Host "ERROR: Failed to create ICO file: $_" -ForegroundColor Red
        }
    }
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Update Differ.App.csproj to add: <ApplicationIcon>Resources\differ.ico</ApplicationIcon>"
Write-Host "2. Rebuild the solution: dotnet build"
Write-Host "3. Run the app to see the new icon!"
