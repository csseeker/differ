# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.0.2-alpha] - 2025-10-01

### Added
- **Structured Logging Infrastructure (Sprint 4):**
  - New `Differ.Common` project for cross-cutting concerns
  - Serilog integration with file, console, and debug output sinks
  - Configuration-based logging via `appsettings.json`
  - Environment-specific logging configs (Development/Production)
  - `IDifferLogLevelManager` for runtime log level control
  - Strongly-typed `DifferLoggingOptions` with validation
  - File logging to `%LOCALAPPDATA%\Differ\Logs` with rolling policies
  - Automatic log directory creation
  - Startup error logging with fallback mechanisms
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
- **Logging Infrastructure Refinement (Sprint 4):**
  - Upgraded Microsoft.Extensions.* packages from 8.0.0 to 9.0.0
  - Simplified logging configuration (removed mixed config/code approaches)
  - Extracted `CreateLoggingLevelSwitch()` and `LogStartupError()` methods
  - Improved startup error handling with user-specific log paths
  - Streamlined `appsettings.Development.json` (removed redundant overrides)
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
- **Logging Architecture (Sprint 4):**
  - 27% reduction in logging setup code (removed complexity)
  - Pure code-based configuration (eliminated mixed approaches)
  - Better testability with extracted methods instead of inline lambdas
  - Cleaner error handling with dedicated logging methods
  - DRY principle applied (removed configuration redundancy)
  - Simplified `LoggingBuilderExtensions` by removing unused methods
- Code maintainability with centralized dialog service
- Testability through mockable dialog interface  
- User experience with professional About dialog
- Discoverability with Help menu

### Fixed
- Long file paths in diff window titles now show only filename for better readability
- Startup error logging now uses proper user-specific directories

### Removed
- Unused template file `Class1.cs` from `Differ.Common` project
- Redundant `.ReadFrom.Configuration()` call in logging setup
- Unused `AddDifferLogLevelConfiguration()` extension method
- Redundant `LevelOverrides` in Development configuration

### Documentation
- `docs/LOGGING_REFACTORING.md` - Complete logging refactoring summary
- `docs/SPRINT4_PLAN.md` - Sprint 4 planning document
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

