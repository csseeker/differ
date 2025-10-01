# Sprint 1: Manual Testing Checklist

**Date:** October 1, 2025  
**Version:** 0.1.0 (Sprint 1 - Core Branding)  
**Tester:** _________________

## Prerequisites
- [ ] Application built successfully (✅ Confirmed - no errors)
- [ ] Application launched without errors

---

## 🪟 Window Title Testing

### Main Window
- [ ] **Window opens successfully**
- [ ] **Title bar shows:** "Differ - Directory Comparison Tool"
- [ ] **Title is fully visible** (not truncated)
- [ ] **Title appears in taskbar** correctly
- [ ] **Title appears in Alt+Tab switcher** correctly

**Expected Title:** `Differ - Directory Comparison Tool`

**Screenshot/Notes:**
```
_______________________________________________________
```

### File Diff Window
- [ ] **Compare two different files** to open diff window
- [ ] **Title bar shows:** "Differ - Comparing: [filename]"
- [ ] **Only filename shown** (not full path)
- [ ] **Title is readable** and professional
- [ ] **Title appears in taskbar** when diff window is active

**Test Steps:**
1. Browse and select two directories
2. Click "Compare"
3. Find a file with "Different" status
4. Double-click or click "View Diff"
5. Verify the diff window title

**Expected Title Format:** `Differ - Comparing: filename.txt`

**Screenshot/Notes:**
```
_______________________________________________________
```

---

## 💬 Status Message Testing

### Main Window - Status Bar

#### Initial State
- [ ] **On launch, status shows:** "Ready to compare directories"
- [ ] **Message is clear and professional**

**Expected:** `Ready to compare directories`

#### Comparison Flow
Test by comparing two directories and verify these messages appear:

1. **Starting:**
   - [ ] Shows: "Starting comparison..."
   - **Expected:** `Starting comparison...`

2. **During comparison:**
   - [ ] Progress messages appear
   - [ ] Messages update during scan

3. **Completion:**
   - [ ] Shows: "Comparison complete - Found [N] items in [X]s"
   - [ ] Number formatting is correct (uses commas for thousands)
   - [ ] Time shows 1 decimal place
   - **Expected Format:** `Comparison complete - Found 1,234 items in 2.3s`

4. **Cancel operation:**
   - [ ] Click "Cancel" during comparison
   - [ ] Shows: "Cancelling operation..."
   - [ ] Then shows: "Comparison cancelled by user"
   - **Expected:** `Cancelling operation...` → `Comparison cancelled by user`

#### Diff Operations
Test diff-related status messages:

1. **Directory selected (not file):**
   - [ ] Select a directory item
   - [ ] Click "View Diff"
   - [ ] Shows: "Diff view is available for files only"
   - **Expected:** `Diff view is available for files only`

2. **Opening diff:**
   - [ ] Select a different file
   - [ ] Click "View Diff"
   - [ ] Shows: "Opening diff for '[filename]'..."
   - **Expected Format:** `Opening diff for 'test.txt'...`

3. **Diff opened:**
   - [ ] After diff window opens
   - [ ] Shows: "Diff view opened for '[filename]'"
   - **Expected Format:** `Diff view opened for 'test.txt'`

**Status Message Screenshot/Notes:**
```
_______________________________________________________
```

---

## 🚨 Error Dialog Testing

### Application Startup Error
**Note:** This is hard to test without forcing an error. Skip if no error occurs.

- [ ] If startup error occurs
- [ ] Dialog title shows: "Differ - Application Error"
- [ ] Message is clear and professional

**Expected Title:** `Differ - Application Error`

### Comparison Error
**Test by:** Using an invalid directory path or inaccessible location

1. **Setup:**
   - Try to compare a non-existent or inaccessible directory
   
2. **Verify:**
   - [ ] Error dialog appears
   - [ ] Title shows: "Differ - Comparison Error"
   - [ ] Message explains the error clearly
   
**Expected Title:** `Differ - Comparison Error`

### Diff Error
**Test by:** Attempting to diff very large or locked files

1. **Setup:**
   - Try to view diff of problematic files
   
2. **Verify:**
   - [ ] Error dialog appears
   - [ ] Title shows: "Differ - Diff Error"
   - [ ] Message is helpful
   
**Expected Title:** `Differ - Diff Error`

**Error Dialog Screenshot/Notes:**
```
_______________________________________________________
```

---

## 🔍 File Properties Testing

### Windows File Properties
**Test Steps:**
1. Navigate to: `src\Differ.App\bin\Debug\net8.0-windows\`
2. Right-click `Differ.App.exe`
3. Select "Properties"
4. Go to "Details" tab

**Verify:**
- [ ] **File description:** "Differ - Directory Comparison Tool"
- [ ] **Product name:** "Differ"
- [ ] **Product version:** "0.1.0.0"
- [ ] **Copyright:** "Copyright © 2025 csseeker"

**Properties Screenshot/Notes:**
```
_______________________________________________________
```

### Task Manager
**Test Steps:**
1. Open Task Manager (Ctrl+Shift+Esc)
2. Find the running Differ application
3. Look at the Name column

**Verify:**
- [ ] **Shows:** "Differ - Directory Comparison Tool" or "Differ.App"
- [ ] Appears professional

**Task Manager Screenshot/Notes:**
```
_______________________________________________________
```

---

## 🎨 Consistency Testing

### Branding Consistency
- [ ] All window titles start with "Differ - "
- [ ] All dialog titles start with "Differ - "
- [ ] Status messages use professional language
- [ ] No typos in any visible text
- [ ] Capitalization is consistent

### Message Quality
- [ ] Messages are clear and understandable
- [ ] No overly technical jargon
- [ ] Grammar is correct
- [ ] Tone is professional and friendly

---

## 📱 User Experience Testing

### Readability
- [ ] Window titles are easy to read
- [ ] Status messages are clear
- [ ] No unnecessarily long text
- [ ] Important information is visible

### Professionalism
- [ ] Application feels more polished than before
- [ ] Branding is consistent throughout
- [ ] Would feel confident sharing with others
- [ ] Looks like a professional tool

**UX Notes:**
```
_______________________________________________________
```

---

## 🐛 Regression Testing

### Core Functionality
- [ ] **Directory comparison** still works correctly
- [ ] **File diff** still works correctly
- [ ] **Filters** work properly
- [ ] **Tree view** updates correctly
- [ ] **Cancel button** works
- [ ] **Browse buttons** work
- [ ] **All existing features** still function

### Performance
- [ ] No noticeable performance degradation
- [ ] Application starts quickly
- [ ] Comparisons run at normal speed
- [ ] UI remains responsive

**Regression Testing Notes:**
```
_______________________________________________________
```

---

## 📸 Screenshots to Capture

Please capture screenshots of:

1. **Main window on launch** - showing new title and ready message
2. **Main window during comparison** - showing progress message
3. **Main window after completion** - showing completion message
4. **Diff window** - showing new title format
5. **Error dialog** (if you encounter one)
6. **File properties** - showing assembly metadata
7. **Taskbar** - showing window title
8. **Alt+Tab** - showing window title in switcher

---

## ✅ Test Results Summary

### Pass/Fail Status
- Window Titles: [ ] Pass [ ] Fail [ ] Not Tested
- Status Messages: [ ] Pass [ ] Fail [ ] Not Tested
- Error Dialogs: [ ] Pass [ ] Fail [ ] Not Tested
- File Properties: [ ] Pass [ ] Fail [ ] Not Tested
- Consistency: [ ] Pass [ ] Fail [ ] Not Tested
- Regression: [ ] Pass [ ] Fail [ ] Not Tested

### Issues Found
```
Issue #1:
Description: ___________________________________________
Severity: [ ] Critical [ ] High [ ] Medium [ ] Low
_______________________________________________________

Issue #2:
Description: ___________________________________________
Severity: [ ] Critical [ ] High [ ] Medium [ ] Low
_______________________________________________________
```

### Overall Assessment
```
Sprint 1 testing: [ ] PASSED [ ] FAILED [ ] NEEDS FIXES

Notes:
_______________________________________________________
_______________________________________________________
_______________________________________________________
```

---

## 🎯 Ready for Sprint 2?

Based on testing results:
- [ ] All tests passed - ready to proceed to Sprint 2
- [ ] Minor issues found - can proceed with notes
- [ ] Major issues found - need fixes before Sprint 2

**Sign-off:** _________________  **Date:** _________

---

## Quick Testing Script

If you want to quickly verify the main changes, follow this **5-minute test:**

1. ✅ **Launch app** - Check main window title
2. ✅ **Compare two directories** - Watch status messages
3. ✅ **Open a diff** - Check diff window title
4. ✅ **Cancel comparison** - Verify cancel message
5. ✅ **Check taskbar/Alt+Tab** - Verify titles appear

**Quick Test Result:** [ ] Pass [ ] Fail

---

**Testing Template Version:** 1.0  
**Created:** October 1, 2025  
**For Sprint:** 1 - Core Branding
