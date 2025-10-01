# Sprint 3: Enhanced UX Features - Implementation Plan

**Date:** October 1, 2025  
**Status:** 🔄 IN PROGRESS  
**Dependencies:** Sprint 1 ✅ Complete | Sprint 2 ⏸️ On Hold (icon pending)

## Overview

This sprint focuses on enhancing the user experience with professional features like an About dialog, Help menu, improved tooltips, and a reusable dialog service.

---

## Phase 3.1: Dialog Service (Foundation)

### Purpose
Create a centralized, testable service for showing dialogs instead of direct MessageBox.Show calls.

### Benefits
- ✅ Consistent dialog styling and branding
- ✅ Easier to test (mockable interface)
- ✅ Future-proof for custom styled dialogs
- ✅ DRY principle - one place to manage dialogs

### Implementation

#### Files to Create:
1. `src/Differ.UI/Services/IDialogService.cs` - Interface
2. `src/Differ.UI/Services/DialogService.cs` - Implementation

#### Files to Update:
1. `src/Differ.App/App.xaml.cs` - Register service in DI
2. `src/Differ.UI/ViewModels/MainViewModel.cs` - Use service instead of MessageBox
3. `src/Differ.UI/ViewModels/FileDiffViewModel.cs` - Use service instead of MessageBox

### Interface Design:
```csharp
public interface IDialogService
{
    void ShowError(string message, string? title = null);
    void ShowWarning(string message, string? title = null);
    void ShowInformation(string message, string? title = null);
    bool ShowConfirmation(string message, string? title = null);
    Task ShowErrorAsync(string message, string? title = null);
}
```

---

## Phase 3.2: About Dialog

### Purpose
Provide application information, version, copyright, and links.

### Features
- Application name and description
- Version number (from assembly)
- Copyright information
- GitHub repository link (clickable)
- Professional layout with icon (when available)
- Close button

### Implementation

#### Files to Create:
1. `src/Differ.UI/Views/AboutWindow.xaml` - XAML view
2. `src/Differ.UI/Views/AboutWindow.xaml.cs` - Code-behind
3. `src/Differ.UI/ViewModels/AboutViewModel.cs` - ViewModel

#### Files to Update:
1. `src/Differ.UI/Views/MainWindow.xaml` - Add Help menu
2. `src/Differ.UI/ViewModels/MainViewModel.cs` - Add ShowAbout command

### Layout Design:
```
┌─────────────────────────────────────┐
│ About Differ                    × │
├─────────────────────────────────────┤
│                                     │
│      [Icon]    Differ               │
│                                     │
│  Directory Comparison Tool          │
│                                     │
│  Version: 0.1.0                     │
│  Copyright © 2025 csseeker          │
│                                     │
│  [GitHub Repository]                │
│                                     │
│              [ Close ]              │
└─────────────────────────────────────┘
```

---

## Phase 3.3: Help Menu

### Purpose
Provide easy access to help, documentation, and about information.

### Menu Items:
- **View Documentation** - Opens GitHub README (if available)
- **Report Issue** - Opens GitHub Issues page
- **Separator**
- **About Differ** - Opens About dialog

### Implementation

#### Files to Update:
1. `src/Differ.UI/Views/MainWindow.xaml` - Add Menu bar
2. `src/Differ.UI/ViewModels/MainViewModel.cs` - Add menu commands

---

## Phase 3.4: Enhanced Tooltips

### Purpose
Provide helpful context for UI elements without cluttering the interface.

### Elements to Enhance:

#### Main Window:
- **Left Directory Browse Button** - "Select the first directory to compare"
- **Right Directory Browse Button** - "Select the second directory to compare"
- **Compare Button** - "Start comparing the selected directories"
- **Cancel Button** - "Stop the current comparison operation"
- **View Diff Button** - "View detailed differences for the selected file"
- **Filter Buttons** - "Show only [status] items"
- **Tree View** - "Double-click to expand/collapse folders"

#### File Diff Window:
- **Ignore Whitespace** - "Ignore differences in whitespace when comparing"
- **Ignore Case** - "Ignore character case differences"
- **Show Whitespace** - "Display whitespace characters (spaces, tabs)"
- **Navigation** - "Jump to previous/next difference"

### Implementation

#### Files to Update:
1. `src/Differ.UI/Views/MainWindow.xaml` - Add ToolTip properties
2. `src/Differ.UI/Views/FileDiffWindow.xaml` - Add ToolTip properties

---

## Phase 3.5: Progress Message Improvements

### Purpose
Make progress messages more descriptive and professional.

### Messages to Enhance:

#### Directory Scanning:
- Before: "Scanning directories..."
- After: "Scanning source directory..."
- After: "Scanning target directory..."
- After: "Analyzing file differences..."

#### File Operations:
- Before: "Processing files..."
- After: "Computing file hashes..."
- After: "Building comparison results..."

### Implementation

#### Files to Update:
1. `src/Differ.UI/Resources/AppMessages.cs` - Add detailed messages
2. `src/Differ.Core/Services/DirectoryComparisonService.cs` - Use detailed progress

---

## Phase 3.6: Keyboard Shortcuts

### Purpose
Improve efficiency for power users.

### Shortcuts to Add:

#### Main Window:
- **Ctrl+O** - Browse left directory
- **Ctrl+Shift+O** - Browse right directory
- **F5** - Start comparison
- **Esc** - Cancel comparison
- **Enter** - View diff (when item selected)
- **F1** - Show help/about

#### Diff Window:
- **F3** - Next difference
- **Shift+F3** - Previous difference
- **Ctrl+W** - Close window
- **Esc** - Close window

### Implementation

#### Files to Update:
1. `src/Differ.UI/Views/MainWindow.xaml` - Add InputBindings
2. `src/Differ.UI/Views/FileDiffWindow.xaml` - Add InputBindings

---

## Phase 3.7: Status Bar Enhancements

### Purpose
Provide more contextual information to users.

### Enhancements:
- Show total items in comparison
- Show filtered items count
- Show current operation time
- Show selection details

### Example:
```
Status: Comparison complete - Found 1,234 items in 2.3s | Showing: 45 different | Selected: config.json (Different)
```

### Implementation

#### Files to Update:
1. `src/Differ.UI/ViewModels/MainViewModel.cs` - Enhanced status logic
2. `src/Differ.UI/Views/MainWindow.xaml` - Enhanced status bar layout

---

## Implementation Priority

### High Priority (Must Have):
1. ✅ Dialog Service - Foundation for better UX
2. ✅ About Dialog - Professional requirement
3. ✅ Help Menu - Discoverability
4. ✅ Basic Tooltips - User guidance

### Medium Priority (Should Have):
5. ⚠️ Enhanced Progress Messages
6. ⚠️ Keyboard Shortcuts (most common)

### Low Priority (Nice to Have):
7. 📋 Advanced Status Bar
8. 📋 Additional keyboard shortcuts

---

## Implementation Order

### Step 1: Dialog Service (30 min)
- Create interface and implementation
- Register in DI
- Update ViewModels to use it

### Step 2: About Dialog (45 min)
- Create ViewModel
- Create XAML view
- Add command to MainViewModel

### Step 3: Help Menu (20 min)
- Add menu bar to MainWindow
- Wire up commands
- Test menu items

### Step 4: Tooltips (30 min)
- Add tooltips to MainWindow controls
- Add tooltips to FileDiffWindow controls
- Test tooltip visibility

### Step 5: Testing (15 min)
- Test all dialogs
- Test menu navigation
- Test tooltips
- Verify accessibility

**Total Estimated Time:** 2.5 - 3 hours

---

## Testing Checklist

### Dialog Service:
- [ ] Error dialogs show correct icon and title
- [ ] Warning dialogs show correct icon and title
- [ ] Information dialogs show correct icon and title
- [ ] Confirmation dialogs return correct result
- [ ] Dialogs are modal and centered

### About Dialog:
- [ ] Shows correct version number
- [ ] Copyright info is displayed
- [ ] Links are clickable
- [ ] Dialog closes properly
- [ ] Can be opened from Help menu

### Help Menu:
- [ ] Menu appears in menu bar
- [ ] All menu items are accessible
- [ ] About opens About dialog
- [ ] External links open in browser

### Tooltips:
- [ ] Tooltips appear on hover
- [ ] Tooltips have helpful text
- [ ] Tooltips don't interfere with clicks
- [ ] Tooltips are properly positioned

---

## Success Criteria

Sprint 3 is complete when:
- ✅ Dialog service is implemented and used throughout
- ✅ About dialog shows application info
- ✅ Help menu provides access to resources
- ✅ Major UI elements have helpful tooltips
- ✅ All features are tested and working
- ✅ No regressions in existing functionality

---

## Next Steps After Sprint 3

With Sprints 1-3 complete:
- Application has professional branding
- User experience is polished
- Help and documentation are accessible
- Ready for beta testing or v0.2.0 release

Optional future sprints:
- **Sprint 4:** Advanced features (export, settings, themes)
- **Sprint 5:** Performance optimizations
- **Sprint 6:** Additional comparison algorithms

---

**Ready to begin Sprint 3 implementation!**
