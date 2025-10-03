# QA Checklist

Use this guide to validate release candidates before publishing.

## Test Matrix

| Scenario | OS | Hardware | Notes |
| --- | --- | --- | --- |
| Smoke test | Windows 11 23H2 | x64 desktop | Clean machine without .NET runtime installed |
| Smoke test | Windows 10 22H2 | x64 laptop | Validate SmartScreen prompt and antivirus exclusions |
| Regression | Windows 11 23H2 | x64 developer machine | Confirm existing user settings and large directory comparisons |
| Accessibility | Windows 11 23H2 | x64 desktop | High-contrast theme, keyboard-only navigation |

## Smoke Test Script

1. Download the latest `Differ-v*-win-x64.zip` from GitHub Releases.
2. Extract the archive to `C:\Program Files\Differ` (or a temp directory without admin rights).
3. Double-click `DifferApp.exe` and confirm the splash screen and main window load within 3 seconds.
4. Compare the sample directories in `tests/Differ.Tests/TestData/` and confirm status icons render correctly.
5. Trigger a cancellation mid-scan and confirm the UI returns to an idle state.
6. Close the app and relaunch to verify settings persistence (if implemented).
7. (Optional) Install the MSIX package and confirm Start menu integration, then uninstall to ensure cleanup.

## Regression Sweep

- Run the automated test suite: `dotnet test -c Release`.
- Execute long-running comparisons on directories with >5,000 files to ensure responsiveness.
- Validate diff viewer navigation and large file streaming performance.

## Accessibility & Localization

- Enable Windows High Contrast mode and ensure all text remains legible.
- Navigate the UI with keyboard only (Tab/Shift+Tab/Enter/Space).
- Check that text elements respect system font scaling at 125% and 150%.

## Bug Triage & Reporting

- Create issues for any defects discovered, referencing the release tag.
- Block the release until all high-severity issues are resolved or explicitly waived.
