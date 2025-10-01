# MSIX Package Icons - Source of Truth

**⚠️ IMPORTANT:** This directory is the **single source of truth** for all MSIX package icons.

---

## 📍 Location
`src/Differ.Package/Images/`

---

## 🎯 Purpose

These icons are used for:
- Windows Start menu tiles (square and wide)
- App list in Settings
- Microsoft Store listing
- Windows taskbar (when packaged as MSIX)

---

## 📦 Required Files

| Filename | Size | Usage |
|----------|------|-------|
| `Square44x44Logo.png` | 44×44 | App list icon in Settings |
| `Square150x150Logo.png` | 150×150 | Start menu medium tile |
| `Wide310x150Logo.png` | 310×150 | Start menu wide tile (optional) |
| `StoreLogo.png` | 50×50 | Microsoft Store listing |

---

## 🔄 Automated Build Process

When you run `scripts/create-msix.ps1`:

1. **Copies these icons** → `artifacts/msix-staging/Assets/`
2. **Includes in MSIX package**
3. **Generates placeholders** if any icon is missing

**You should NEVER manually edit files in `artifacts/` - always edit here!**

---

## ✏️ How to Update Icons

### Step 1: Create New Icons
- Design or download new icon assets
- Ensure transparent background (PNG format)
- Resize to exact dimensions listed above

### Step 2: Replace Files
- Replace the 4 PNG files in this directory
- Keep the exact same filenames

### Step 3: Build & Test
```powershell
# Build MSIX package (copies icons automatically)
.\scripts\create-msix.ps1

# Test in Windows:
# - Check Start menu tile
# - Check app list icon
# - Verify on light/dark themes
```

### Step 4: Commit Changes
```bash
# Only commit source icons (this directory)
git add src/Differ.Package/Images/
git commit -m "Update MSIX package icons"

# Build output (artifacts/) is auto-generated and ignored
```

---

## 🎨 Design Guidelines

### Format
- **Type:** PNG with transparency
- **Color:** Should work on light AND dark backgrounds
- **Style:** Consistent with app branding

### Sizes
| Icon | Recommended Design Size | Output Size |
|------|------------------------|-------------|
| Square44x44 | 88×88 @ 2x | 44×44 |
| Square150x150 | 300×300 @ 2x | 150×150 |
| Wide310x150 | 620×300 @ 2x | 310×150 |
| StoreLogo | 100×100 @ 2x | 50×50 |

**Tip:** Design at 2x size for crisp rendering on high-DPI displays, then scale down.

---

## 🛠️ Quick Generation Scripts

### Using ImageMagick (if you have a master 256×256 icon):
```powershell
# From project root
$master = "src/Differ.App/Resources/icon-256.png"
$out = "src/Differ.Package/Images"

magick convert $master -resize 44x44 "$out/Square44x44Logo.png"
magick convert $master -resize 150x150 "$out/Square150x150Logo.png"
magick convert $master -resize 310x150 "$out/Wide310x150Logo.png"
magick convert $master -resize 50x50 "$out/StoreLogo.png"
```

### Using PowerShell Script (generates placeholder):
```powershell
# Generates blue "Df" placeholder icons
.\scripts\refresh-packaging-assets.ps1
```

---

## 📚 Related Documentation

- **Icon Strategy Overview:** `docs/ICON_STRATEGY.md`
- **Complete Icon Guide:** `docs/ICON_LOCATIONS.md`
- **MSIX Packaging:** `docs/MSIX_PACKAGING.md`

---

## ✅ Current Status

**Icons:** Placeholder blue squares with "Df" text  
**Status:** Ready for custom branding  
**Action Required:** Replace with actual designed icons

---

**Remember:** Always edit icons HERE, never in the `artifacts/` folder!
