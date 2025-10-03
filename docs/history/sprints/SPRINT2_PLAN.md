# Sprint 2: Icons & Visuals - Implementation Plan

**Date:** October 1, 2025  
**Status:** 🔄 IN PROGRESS

## Overview

This sprint focuses on adding professional icons to the Differ application, improving visual identity and brand recognition across Windows.

## Phase 2.1: Icon Design & Creation

### Option A: Simple Text-Based Icon (Quick - 30 min)
Create a simple but professional icon using the letter "D" with comparison symbolism.

**Concept:**
- Large "D" for Differ
- Subtle comparison arrows (⇄) or split design
- Professional color scheme (blues/grays or brand colors)
- Clean, modern look

### Option B: AI-Generated Icon (Medium - 1-2 hours)
Use AI tools to generate icon concepts, then convert to .ico format.

**Tools:**
- DALL-E, Midjourney, or Stable Diffusion
- Prompt: "A simple, modern app icon for a file comparison tool, featuring two overlapping folders or documents with a comparison symbol, professional blue and gray color scheme, flat design, suitable for Windows application"

### Option C: Professional Design Service (Long - 24-48 hours)
Commission a professional designer on Fiverr or similar platform.

**Cost:** $10-50  
**Turnaround:** 1-3 days

### Option D: Use Free Icon Templates (Quick - 15 min)
Download from icon libraries with proper attribution.

**Sources:**
- Icons8 (https://icons8.com)
- Flaticon (https://www.flaticon.com)
- The Noun Project (https://thenounproject.com)

**Search terms:** "compare files", "diff tool", "folder comparison"

---

## Phase 2.2: Icon File Creation

### Required Icon Formats

#### For Windows Application (.ico)
Must include these sizes in one .ico file:
- 16x16 (taskbar, title bar when small)
- 32x32 (title bar, small icons)
- 48x48 (Windows Explorer, large icons)
- 256x256 (Windows 7+ high DPI, thumbnails)

#### For MSIX Package (.png)
Already exist, may need updating:
- Square44x44Logo.png (44x44)
- Square150x150Logo.png (150x150)
- Wide310x150Logo.png (310x150)
- StoreLogo.png (50x50)

### Tools for Creating .ico Files

**Windows:**
- GIMP (free) - https://www.gimp.org
- IcoFX (paid) - https://icofx.ro
- Online converter - https://convertio.co/png-ico/

**Steps with GIMP:**
1. Create or import 256x256 PNG
2. Export As → choose .ico format
3. Select all required sizes (16, 32, 48, 256)
4. Save as `differ.ico`

---

## Phase 2.3: Integration

### Task 1: Create Resources Directory
```
src/Differ.App/
  Resources/
    differ.ico           # Main application icon
    icon-256.png         # Optional: high-res source
```

### Task 2: Update Differ.App.csproj
Add ApplicationIcon property:
```xml
<PropertyGroup>
  <ApplicationIcon>Resources\differ.ico</ApplicationIcon>
</PropertyGroup>
```

### Task 3: Update MainWindow.xaml
Add Icon property:
```xaml
<Window x:Class="Differ.UI.Views.MainWindow"
        ...
        Icon="/Differ.App;component/Resources/differ.ico"
        Title="Differ - Directory Comparison Tool">
```

### Task 4: Update FileDiffWindow.xaml
Add Icon property:
```xaml
<Window x:Class="Differ.UI.Views.FileDiffWindow"
        ...
        Icon="/Differ.App;component/Resources/differ.ico"
        Title="{Binding Title}">
```

---

## Phase 2.4: Package Assets (Optional Enhancement)

Update the MSIX package images with brand-consistent designs:

### Current Assets Status
Located in: `src/Differ.Package/Images/`
- ✅ Square44x44Logo.png (exists - may be placeholder)
- ✅ Square150x150Logo.png (exists - may be placeholder)
- ✅ Wide310x150Logo.png (exists - may be placeholder)
- ✅ StoreLogo.png (exists - may be placeholder)

### Enhancement Options
1. **Keep placeholders** - They work for now
2. **Update with brand colors** - Modify existing to match app icon
3. **Professional redesign** - Create cohesive tile set

---

## Phase 2.5: Testing

### Visual Testing Checklist
- [ ] Icon appears in window title bar (both windows)
- [ ] Icon appears in Windows taskbar
- [ ] Icon appears in Alt+Tab switcher
- [ ] Icon appears in Windows Explorer (for .exe)
- [ ] Icon looks good at all sizes (no pixelation)
- [ ] Icon is visually distinct and recognizable
- [ ] Icon matches brand identity

### Technical Testing
- [ ] Application builds successfully
- [ ] No errors or warnings about icon
- [ ] Icon resource is properly embedded
- [ ] File size is reasonable (<100KB for .ico)

---

## Implementation Steps for This Session

I'll implement Option A (Simple Icon) and Option D (Free Template) in parallel:

### Step 1: Quick Implementation (15 min)
1. Create a simple SVG-based icon concept
2. Convert to PNG at multiple sizes
3. Combine into .ico file using online converter
4. Integrate into project

### Step 2: Update Project Files (10 min)
1. Create Resources directory
2. Add icon file
3. Update .csproj
4. Update XAML files

### Step 3: Build & Test (5 min)
1. Build application
2. Run and verify icon appears
3. Check all windows

---

## Deliverables

By end of Sprint 2:
- ✅ Professional .ico file created
- ✅ Icon integrated into application
- ✅ Icon appears in all windows
- ✅ Icon visible in taskbar and Alt+Tab
- ✅ Documentation updated
- ✅ Testing checklist completed

---

## Next Steps After Sprint 2

Once icons are complete, we can move to **Sprint 3: Enhanced UX** which includes:
- About dialog
- Help menu
- Tooltips
- Dialog service
- Additional polish

---

## Icon Design Guidelines

### Do's ✅
- Keep it simple and recognizable
- Use 2-3 colors maximum
- Ensure visibility at small sizes (16x16)
- Test on both light and dark backgrounds
- Make it unique and memorable

### Don'ts ❌
- Don't use thin lines (won't show at small sizes)
- Don't use too many colors
- Don't make it too complex
- Don't use gradients that don't scale well
- Don't copy existing app icons

---

**Ready to proceed with implementation!**
