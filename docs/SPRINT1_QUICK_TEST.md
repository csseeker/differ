# Sprint 1 Testing Guide - Quick Visual Comparison

## 🎯 What to Look For

### 1️⃣ Main Window Title (TOP PRIORITY)

**Look at the window title bar:**

```
┌────────────────────────────────────────────────────┐
│ 👀 Differ - Directory Comparison Tool     ─ □ ×  │  ← Should say this!
├────────────────────────────────────────────────────┤
│                                                    │
│  Left Directory:  [           ] [Browse]          │
│  Right Directory: [           ] [Browse]          │
│                                                    │
│  Status: Ready to compare directories             │  ← Check this message
│                                                    │
└────────────────────────────────────────────────────┘
```

✅ **PASS if:** Title says "Differ - Directory Comparison Tool"  
❌ **FAIL if:** Title says "Directory Differ" or anything else

---

### 2️⃣ Status Messages (IMPORTANT)

**Watch the status bar at the bottom during these actions:**

#### A) When app first opens:
```
Status: Ready to compare directories  ← Should be more descriptive than just "Ready"
```

#### B) After clicking "Compare" button:
```
Status: Starting comparison...
Status: Scanning directories...
Status: Comparison complete - Found 150 items in 2.3s  ← Note the format!
```

✅ **PASS if:** Messages are clear and professional  
❌ **FAIL if:** Messages are unclear or have wrong format

---

### 3️⃣ Diff Window Title (HIGH PRIORITY)

**Steps to test:**
1. Compare two directories
2. Find a file marked as "Different"
3. Double-click it or click "View Diff"
4. Look at the new window's title

**Expected:**
```
┌────────────────────────────────────────────────────┐
│ 👀 Differ - Comparing: myfile.txt        ─ □ ×   │  ← Just filename!
├────────────────────────────────────────────────────┤
│                                                    │
│  [Side-by-side file comparison view]              │
│                                                    │
└────────────────────────────────────────────────────┘
```

✅ **PASS if:** Shows "Differ - Comparing: filename.txt" (just the filename)  
❌ **FAIL if:** Shows full path like "C:\Users\...\file.txt"

---

### 4️⃣ Error Dialogs (IF YOU SEE ONE)

**If you encounter any error dialog, check the title:**

```
┌─────────────────────────────────┐
│ 👀 Differ - Application Error × │  ← Should start with "Differ -"
├─────────────────────────────────┤
│                                 │
│  [Error message here]           │
│                                 │
│         [ OK ]                  │
└─────────────────────────────────┘
```

✅ **PASS if:** Dialog title starts with "Differ - "  
❌ **FAIL if:** Just says "Error" or "Application Error"

---

## 🚀 Quick 5-Minute Test

**Do this to verify Sprint 1 is working:**

### Step 1: Launch & Check Title (30 seconds)
- [ ] Start the app
- [ ] Look at window title → Should say "Differ - Directory Comparison Tool"
- [ ] Look at status bar → Should say "Ready to compare directories"

### Step 2: Test Comparison (2 minutes)
- [ ] Click "Browse" for left directory
- [ ] Select any folder (e.g., `C:\Windows\System32`)
- [ ] Click "Browse" for right directory  
- [ ] Select another folder (e.g., `C:\Windows\Temp`)
- [ ] Click "Compare" button
- [ ] Watch status messages change
- [ ] Verify final message format: "Comparison complete - Found X items in Y.Ys"

### Step 3: Test Diff Window (2 minutes)
- [ ] Find a file marked as "Different" in the results
- [ ] Double-click it to open diff view
- [ ] Check the diff window title
- [ ] Should show: "Differ - Comparing: filename.txt"
- [ ] Should NOT show full path

### Step 4: Test Cancel (30 seconds)
- [ ] Start another comparison with large directories
- [ ] Click "Cancel" button while it's running
- [ ] Verify status shows: "Cancelling operation..."
- [ ] Then shows: "Comparison cancelled by user"

### Step 5: Check Taskbar (30 seconds)
- [ ] Look at Windows taskbar
- [ ] Verify the app name shows correctly
- [ ] Press Alt+Tab
- [ ] Verify the window title appears properly

---

## ✅ Pass Criteria

**Sprint 1 PASSES if:**
- ✅ Main window title is correct
- ✅ Diff window title shows just filename
- ✅ Status messages are professional
- ✅ All dialogs have "Differ - " prefix
- ✅ No crashes or errors
- ✅ All features still work

**Sprint 1 FAILS if:**
- ❌ Titles are wrong or unchanged
- ❌ Status messages are unclear
- ❌ App crashes
- ❌ Features don't work anymore

---

## 📸 Quick Screenshot Checklist

Capture these 3 screenshots if possible:

1. **Main window** showing the title bar and status
2. **Diff window** showing the title with just filename
3. **Taskbar** showing the app name

Save them for the Sprint 1 completion report!

---

## 🐛 Common Issues to Watch For

### Issue: Diff window title still shows full path
**Symptom:** Title shows `C:\Users\...\file.txt` instead of `Differ - Comparing: file.txt`  
**Severity:** HIGH - This is a key Sprint 1 feature  
**Report:** This needs to be fixed

### Issue: Status messages unchanged
**Symptom:** Still shows "Ready" instead of "Ready to compare directories"  
**Severity:** MEDIUM - Messages should be updated  
**Report:** This needs investigation

### Issue: Dialog titles unchanged
**Symptom:** Error dialogs don't have "Differ - " prefix  
**Severity:** LOW - Less common but should be fixed  
**Report:** Note for review

---

## 💡 Testing Tips

1. **Use different directories** - Compare C:\Windows with C:\Program Files
2. **Test with large directories** - This lets you test cancel button
3. **Find different files** - Look for text files to test diff view
4. **Try edge cases** - Empty directories, single files, etc.
5. **Check multiple windows** - Open several diff windows and check titles

---

## 📊 Expected Results vs Actual

| Test | Expected Result | Actual Result | Pass/Fail |
|------|----------------|---------------|-----------|
| Main window title | "Differ - Directory Comparison Tool" | __________ | [ ] |
| Ready status | "Ready to compare directories" | __________ | [ ] |
| Complete status | "Comparison complete - Found X items in Y.Ys" | __________ | [ ] |
| Diff window title | "Differ - Comparing: filename.txt" | __________ | [ ] |
| Error dialog title | "Differ - [Error Type]" | __________ | [ ] |

---

## 🎯 Test Result

After completing the 5-minute test:

**Overall Result:** [ ] ✅ PASS  [ ] ❌ FAIL  [ ] ⚠️ NEEDS REVIEW

**Notes:**
```
_________________________________________________________________
_________________________________________________________________
_________________________________________________________________
```

**Ready for Sprint 2?** [ ] YES  [ ] NO  [ ] WITH FIXES

---

**Happy Testing! 🧪**

If everything passes, we're ready to move on to Sprint 2: Icons & Visuals! 🎨
