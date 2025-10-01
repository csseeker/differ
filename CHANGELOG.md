# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- **Enhanced UX Features (Sprint 3):**
  - Dialog Service (`IDialogService`) for consistent, testable dialog management
  - About Dialog showing version, copyright, and GitHub link
  - Help menu with "About Differ" option
  - Professional dialog titles with "Differ - " branding
  - Async dialog support for thread-safe operations
  - Proper dependency injection for all UI services
- **Application Icons (Sprint 2 - Partial):**
  - Application icon (`.ico` file) embedded in executable
  - Window icon displayed in title bar, taskbar, and Alt+Tab
  - MSIX package icons for Windows Store listing
  - Icon resources properly configured in project files

### Changed
- **Professional Branding Upgrade (Sprint 1 - Core Branding):**
  - Updated main window title to "Differ - Directory Comparison Tool"
  - Improved FileDiff window titles to show just filename instead of full path
  - Centralized all user-facing messages in `AppMessages.cs` for consistency
  - Updated all status messages to be more professional and user-friendly
  - Standardized all dialog titles with "Differ - " prefix for brand consistency
  - Enhanced assembly metadata with proper title, description, and copyright
  - Improved MSIX package manifest with better descriptions
- **Dialog Management (Sprint 3):**
  - Replaced all direct `MessageBox.Show` calls with `IDialogService`
  - ViewModels now use dependency-injected dialog service
  - Thread-safe dialog display on UI thread

### Improved
- Code maintainability with centralized dialog service
- Testability through mockable dialog interface  
- User experience with professional About dialog
- Discoverability with Help menu

### Fixed
- Long file paths in diff window titles now show only filename for better readability

### Documentation
- `docs/SPRINT1_BRANDING_COMPLETE.md` - Sprint 1 completion report
- `docs/SPRINT1_VISUAL_SUMMARY.md` - Visual before/after comparison
- `docs/SPRINT1_TESTING_CHECKLIST.md` - Testing procedures
- `docs/SPRINT1_QUICK_TEST.md` - 5-minute test guide
- `docs/SPRINT2_PLAN.md` - Icon implementation plan (on hold)
- `docs/SPRINT3_PLAN.md` - UX features plan
- `docs/SPRINT3_COMPLETE.md` - Sprint 3 completion report

---

## [Previous Changes]
- Documented manual release process, release notes template, and MSIX packaging guidance.
- Added `scripts/create-msix.ps1` to automate MSIX generation with optional signing.
- Introduced the `Differ.Package` Windows Application Packaging Project with placeholder assets and manifest.
- Added `scripts/refresh-packaging-assets.ps1` to regenerate packaging logos on demand.

