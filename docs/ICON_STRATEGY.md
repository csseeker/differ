# Icon Strategy - Unified Source Approach

**Last Updated:** October 1, 2025  
**Status:** ✅ Implemented - Option A (Unified Source)

---

## 📋 Overview

Differ uses a **unified source approach** for icon management:
- **Single source of truth:** `src/Differ.Package/Images/`
- **Automated copying:** Build scripts copy icons to staging
- **Version control friendly:** Only source icons are committed
- **Clear data flow:** Source → Build Output (never reversed)

---

## 📁 Icon File Locations

### 🎯 Source Icons (Version Controlled)
**Location:** `src/Differ.Package/Images/`

This is the **ONLY** location you should manually edit icons:

```
src/Differ.Package/Images/
├── Square44x44Logo.png      (44x44)   - App list icon
├── Square150x150Logo.png    (150x150) - Start menu medium tile
├── Wide310x150Logo.png      (310x150) - Start menu wide tile
└── StoreLogo.png            (50x50)   - Microsoft Store listing
```

**✅ DO:**
- Replace icons here when updating branding
- Commit changes to version control
- Use these as reference for creating ICO files

**❌ DON'T:**
- Manually edit icons in `artifacts/` or staging folders
- Copy icons from build output back to source

---

### 🏗️ Build Output (Auto-Generated)
**Location:** `artifacts/msix-staging/Assets/`

These files are **automatically copied** during MSIX packaging:

```
artifacts/msix-staging/Assets/
├── Square44x44Logo.png      (copied from source)
├── Square150x150Logo.png    (copied from source)
├── Wide310x150Logo.png      (copied from source)
└── StoreLogo.png            (copied from source)
```

**Behavior:**
- Created by `scripts/create-msix.ps1`
- Copied from `src/Differ.Package/Images/` during build
- If source missing, generates placeholder (blue "Df" logo)
- Should NOT be committed to version control (add to `.gitignore`)

---

### 🪟 Application Resources
**Location:** `src/Differ.App/Resources/`

Additional icon files for standalone application:

```
src/Differ.App/Resources/
├── differ.ico        (multi-res: 16,32,48,256) - Windows .exe icon
└── icon-256.png      (256x256)                  - WPF window icon
```

**Purpose:**
- `differ.ico` - Embedded in .exe for taskbar/title bar/Alt+Tab
- `icon-256.png` - Runtime window icon for WPF

---

## 🔄 Data Flow

```
┌─────────────────────────────────────┐
│  Source Icons (Manual Edits)       │
│  src/Differ.Package/Images/*.png   │
│  ✏️ EDIT THESE TO UPDATE BRANDING  │
└──────────────┬──────────────────────┘
               │
               │ create-msix.ps1 copies during build
               ↓
┌─────────────────────────────────────┐
│  Build Output (Auto-Generated)     │
│  artifacts/msix-staging/Assets/    │
│  🚫 DO NOT EDIT MANUALLY            │
└─────────────────────────────────────┘
```

---

## 🛠️ How to Update Icons

### Option 1: Replace Existing Icons (Recommended)
1. Create or download new icon designs
2. Resize to required dimensions:
   - 44x44, 150x150, 310x150, 50x50
3. **Replace files in:** `src/Differ.Package/Images/`
4. Run build script - icons automatically copied to staging
5. Commit changes to version control

### Option 2: Use Refresh Script
```powershell
# Generate placeholder icons (for testing only)
.\scripts\refresh-packaging-assets.ps1

# Creates blue "Df" logos in src/Differ.Package/Images/
```

### Option 3: Use Design Tools
```bash
# ImageMagick example - resize from master 256x256 icon
magick convert icon-256.png -resize 44x44 src/Differ.Package/Images/Square44x44Logo.png
magick convert icon-256.png -resize 150x150 src/Differ.Package/Images/Square150x150Logo.png
magick convert icon-256.png -resize 310x150 src/Differ.Package/Images/Wide310x150Logo.png
magick convert icon-256.png -resize 50x50 src/Differ.Package/Images/StoreLogo.png
```

---

## 📦 Icon Usage by Publishing Type

### MSIX Package (Windows Store/Sideload)
**Icons Used:**
- All 4 PNG files from `src/Differ.Package/Images/`
- Automatically copied to staging during `create-msix.ps1`

**References:**
- `artifacts/msix-staging/AppxManifest.xml`
- `src/Differ.Package/Package.appxmanifest`

**Status:** ✅ Fully automated

---

### Portable ZIP (Standalone .exe)
**Icons Used:**
- `src/Differ.App/Resources/differ.ico` (embedded in .exe)
- `src/Differ.App/Resources/icon-256.png` (WPF window)

**References:**
- `src/Differ.App/Differ.App.csproj` → `<ApplicationIcon>`
- `src/Differ.UI/Views/MainWindow.xaml` → `Icon` attribute

**Status:** ⚠️ Requires manual ICO creation (see instructions below)

---

### MSI Installer (Future)
**Icons Used:**
- Same as Portable ZIP
- Possibly additional installer UI icons

**Status:** 🔮 Not yet implemented

---

## 🎨 Creating the ICO File

The ICO file is **not** auto-generated. You must create it separately:

### Method 1: Online Converter (Easiest)
1. Go to: https://convertio.co/png-ico/
2. Upload your 256x256 PNG icon
3. Configure to include: 16x16, 32x32, 48x48, 256x256
4. Download as `differ.ico`
5. Place in: `src/Differ.App/Resources/differ.ico`

### Method 2: ImageMagick (Command Line)
```powershell
# Convert from 256x256 PNG to multi-resolution ICO
magick convert src/Differ.App/Resources/icon-256.png `
  -define icon:auto-resize=256,48,32,16 `
  src/Differ.App/Resources/differ.ico
```

### Method 3: GIMP (Professional)
1. Open your PNG in GIMP
2. File → Export As
3. Change extension to `.ico`
4. Select all sizes: 16, 32, 48, 256
5. Export

---

## 🔍 Build Script Behavior

### `create-msix.ps1` Flow:
1. **Copy published app files** → staging
2. **Copy icons from source:**
   - Looks for: `src/Differ.Package/Images/*.png`
   - Copies to: `artifacts/msix-staging/Assets/`
   - Prints: `✓ Copied [filename]`
3. **Generate placeholders** (if source missing):
   - Creates blue "Df" placeholder
   - Prints: `⚠ Generating placeholder`
4. **Package MSIX** with copied/generated icons

### `refresh-packaging-assets.ps1` Flow:
1. **Generates placeholder PNGs**
2. **Saves to:** `src/Differ.Package/Images/`
3. **Use only for:** Initial setup or testing

---

## ✅ Best Practices

### Version Control
```gitignore
# Commit source icons
src/Differ.Package/Images/*.png

# Do NOT commit build output
artifacts/msix-staging/
```

### Icon Design Guidelines
- **Format:** PNG with transparent background
- **Style:** Consistent visual design across all sizes
- **Testing:** Test on light/dark Windows themes
- **Scaling:** Design at 256x256, scale down (not up)

### Workflow
1. Edit icons in `src/Differ.Package/Images/`
2. Run build/package scripts
3. Commit only source changes
4. Build output regenerated on every build

---

## 🧪 Testing Icon Changes

### Test MSIX Icons:
```powershell
# Build MSIX with new icons
.\scripts\create-msix.ps1

# Install and check:
# - Start menu tile appearance
# - App list icon
# - Taskbar icon (from ICO, not PNG)
```

### Test Standalone .exe Icons:
```cmd
# Build portable version
.\buildandrun.bat

# Check:
# - Window title bar icon
# - Taskbar icon (uses differ.ico)
# - Alt+Tab icon
# - File Explorer thumbnail
```

---

## 📚 Related Documentation

- **Full icon reference:** `docs/ICON_LOCATIONS.md`
- **Icon creation guide:** `docs/ICON_CREATION_GUIDE.md`
- **MSIX packaging:** `docs/MSIX_PACKAGING.md`
- **App resources:** `src/Differ.App/Resources/README.md`

---

## 🎯 Quick Reference

| Icon Type | Location | Usage | Auto-Generated? |
|-----------|----------|-------|-----------------|
| **MSIX PNGs** | `src/Differ.Package/Images/` | Windows Store tiles | No (source) |
| **Staging PNGs** | `artifacts/msix-staging/Assets/` | MSIX build | Yes (copied) |
| **Windows ICO** | `src/Differ.App/Resources/differ.ico` | .exe taskbar/title | No (manual) |
| **WPF PNG** | `src/Differ.App/Resources/icon-256.png` | Window icon | No (manual) |

---

**Summary:** Edit icons in `src/Differ.Package/Images/`, run build scripts, and let automation handle the rest. Only the ICO file requires manual creation using conversion tools.
