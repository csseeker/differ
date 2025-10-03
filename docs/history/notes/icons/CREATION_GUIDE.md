# Icon Creation Guide for Differ

## Quick Start: Get Your Icon in 5 Minutes

### Method 1: Download a Free Icon (RECOMMENDED - Fastest)

#### Step 1: Visit Icons8
1. Go to: https://icons8.com/icons/set/compare-files
2. Search for: "compare files" or "diff" or "folder compare"
3. Find an icon you like

#### Step 2: Download
1. Select the icon
2. Choose "Free" download
3. Download as PNG, 256x256 size
4. Save to: `d:\repos\differ\temp-icon.png`

#### Step 3: Convert to ICO
1. Go to: https://convertio.co/png-ico/
2. Upload your PNG file
3. In the settings, select these sizes:
   - ✅ 16x16
   - ✅ 32x32
   - ✅ 48x48
   - ✅ 256x256
4. Download as: `differ.ico`
5. Save to: `d:\repos\differ\src\Differ.App\Resources\differ.ico`

---

### Method 2: Use Windows Built-in Icons (Quick & Easy)

Windows has built-in icons we can extract and use:

#### Extract from System Files
```powershell
# Run this PowerShell script to view available icons
# (For reference only - don't extract copyrighted icons for distribution)
```

---

### Method 3: Create Simple Icon with PowerShell (DIY)

I'll provide a script below that creates a simple colored icon.

---

## Icon Design Suggestions

### Concept 1: Split Square
```
┌──────┬──────┐
│      │      │
│  D   │  D   │  Two halves showing comparison
│      │      │
└──────┴──────┘
```

### Concept 2: Overlapping Folders
```
  ┌────┐
  │    │
┌─┼────┤     Two folders overlapping
│ │    │     showing comparison
└─┴────┘
```

### Concept 3: Arrows
```
    ←  D  →        Letter D with comparison arrows
```

---

## Recommended Icons for Differ

Based on the app's purpose, search for these concepts:

1. **"compare folders"** - Two folders side by side
2. **"diff tool"** - Document with split view
3. **"file comparison"** - Two documents with arrows
4. **"synchronize"** - Circular arrows with files
5. **"contrast"** - Split design showing difference

### Color Schemes
- **Professional Blue:** #0078D4 (Windows accent blue)
- **Tech Gray:** #5E5E5E with blue accent
- **Vibrant:** #FF6B35 (orange) + #004E89 (blue)

---

## Where to Find Free Icons

### Icons8 (Recommended)
- URL: https://icons8.com
- Free with attribution
- High quality
- Many styles available
- Easy to customize colors

### Flaticon
- URL: https://www.flaticon.com
- Free with attribution
- Huge library
- Multiple file formats

### The Noun Project
- URL: https://thenounproject.com
- Free with attribution
- Simple, clean designs
- Perfect for professional apps

### Iconduck
- URL: https://iconduck.com
- Completely free
- No attribution required
- Open source icons

---

## PowerShell Script: Create Simple Icon

Save this as `create-icon.ps1` and run it:

```powershell
# Create a simple icon using .NET Drawing libraries
# This creates a basic placeholder icon

Add-Type -AssemblyName System.Drawing

function Create-SimpleIcon {
    param(
        [string]$OutputPath = "differ-temp.png",
        [int]$Size = 256
    )
    
    # Create bitmap
    $bitmap = New-Object System.Drawing.Bitmap($Size, $Size)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    
    # Colors
    $bgColor = [System.Drawing.Color]::FromArgb(0, 120, 212)  # Windows blue
    $fgColor = [System.Drawing.Color]::White
    
    # Fill background
    $brush = New-Object System.Drawing.SolidBrush($bgColor)
    $graphics.FillRectangle($brush, 0, 0, $Size, $Size)
    
    # Draw letter "D"
    $font = New-Object System.Drawing.Font("Segoe UI", [int]($Size * 0.5), [System.Drawing.FontStyle]::Bold)
    $textBrush = New-Object System.Drawing.SolidBrush($fgColor)
    $format = New-Object System.Drawing.StringFormat
    $format.Alignment = [System.Drawing.StringAlignment]::Center
    $format.LineAlignment = [System.Drawing.StringAlignment]::Center
    
    $rect = New-Object System.Drawing.RectangleF(0, 0, $Size, $Size)
    $graphics.DrawString("D", $font, $textBrush, $rect, $format)
    
    # Save
    $bitmap.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    
    # Cleanup
    $graphics.Dispose()
    $bitmap.Dispose()
    $brush.Dispose()
    $textBrush.Dispose()
    
    Write-Host "Icon created: $OutputPath" -ForegroundColor Green
}

# Create icons at multiple sizes
$outputDir = ".\icon-temp"
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

Create-SimpleIcon -OutputPath "$outputDir\icon-256.png" -Size 256
Create-SimpleIcon -OutputPath "$outputDir\icon-48.png" -Size 48
Create-SimpleIcon -OutputPath "$outputDir\icon-32.png" -Size 32
Create-SimpleIcon -OutputPath "$outputDir\icon-16.png" -Size 16

Write-Host "`nNow convert these PNG files to ICO format using:" -ForegroundColor Cyan
Write-Host "https://convertio.co/png-ico/" -ForegroundColor Yellow
Write-Host "`nUpload all 4 PNG files and download as differ.ico" -ForegroundColor Cyan
```

---

## Manual Icon Creation with Windows Paint

If you prefer to create it manually:

### Step 1: Open Paint
- Press Win+R, type `mspaint`, press Enter

### Step 2: Set Canvas Size
- Click Resize
- Pixels: 256 x 256
- Uncheck "Maintain aspect ratio"

### Step 3: Draw Your Icon
- Use shapes, text, colors
- Keep it simple!
- Recommended: Large letter "D" on blue background

### Step 4: Save
- Save as PNG
- Name: `differ-icon-256.png`

### Step 5: Convert
- Go to https://convertio.co/png-ico/
- Upload and convert to ICO with multiple sizes

---

## What to Do Next

1. **Choose your method** (I recommend Method 1 - Icons8)
2. **Get your icon file** (differ.ico)
3. **Tell me when ready**, and I'll integrate it into the project!

---

## Need Help?

If you're stuck, you can:
1. Use a simple colored square as a temporary placeholder
2. Extract an icon from existing Windows apps (for testing only)
3. Use the PowerShell script above for a basic "D" icon
4. Ask a designer friend to create one quickly

---

**The icon doesn't need to be perfect - even a simple, clean icon is much better than no icon!**
