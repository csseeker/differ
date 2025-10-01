# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed
- **Professional Branding Upgrade (Sprint 1 - Core Branding):**
  - Updated main window title to "Differ - Directory Comparison Tool"
  - Improved FileDiff window titles to show just filename instead of full path
  - Centralized all user-facing messages in `AppMessages.cs` for consistency
  - Updated all status messages to be more professional and user-friendly
  - Standardized all dialog titles with "Differ - " prefix for brand consistency
  - Enhanced assembly metadata with proper title, description, and copyright
  - Improved MSIX package manifest with better descriptions

### Added
- New `AppMessages.cs` resource file for centralized message management
- Assembly metadata in `Differ.App.csproj` (version, copyright, description)
- Documentation: `docs/SPRINT1_BRANDING_COMPLETE.md`

### Fixed
- Long file paths in diff window titles now show only filename for better readability

---

## [Previous Changes]

- Documented manual release process, release notes template, and MSIX packaging guidance.
- Added `scripts/create-msix.ps1` to automate MSIX generation with optional signing.
- Introduced the `Differ.Package` Windows Application Packaging Project with placeholder assets and manifest.
- Added `scripts/refresh-packaging-assets.ps1` to regenerate packaging logos on demand.

