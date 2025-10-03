# Directory Differ

A clean, intuitive Windows application for comparing directories and files, built with WPF and .NET 8.

📚 Looking for docs? Start with the [documentation hub](docs/index.md) for install guides, architecture notes, and release playbooks.

## Download

### Latest Release: v0.1.2-alpha

Get the latest version from the [releases page](https://github.com/csseeker/differ/releases).

#### Portable Version (Recommended for Quick Start)
- Download: `Differ-v*-win-x64.zip`
- Extract anywhere and run `Differ.App.exe`
- No installation required
- Windows SmartScreen may prompt; choose **"More info"** → **"Run anyway"**

#### MSIX Installer (Start Menu Integration)

**🚀 Quick Install Method** (Easiest - Recommended):
1. Download these 4 files:
   - `Differ_*_x64.msix`
   - `differ-signing-cert.cer`
   - `install-differ.ps1`
   - `Install-Differ.bat` ⭐ **Start here!**

2. Put all files in the same folder

3. Double-click `Install-Differ.bat`

4. Click "Yes" for Administrator permission

5. Follow the prompts - it installs everything automatically!

**Manual Method** (for advanced users):
- First time: Install certificate (requires Admin)
  ```powershell
  # Right-click PowerShell → Run as Administrator
  powershell -ExecutionPolicy Bypass -File install-certificate.ps1
  ```
- Then: Double-click the `.msix` file to install
- Future updates: Just install new MSIX (certificate stays)

Need more detail or troubleshooting help? Check the [Installing Differ guide](docs/user-guide/installing-differ.md).

## Features

- **Directory Comparison**: Compare two directories side-by-side and identify all differences
- **File Analysis**: Hash-based file comparison (MD5) for accurate difference detection
- **Detailed Diff Viewer**: View line-by-line differences for text files
- **Professional UI**: Clean WPF interface with progress reporting and cancellation support
- **Status Indicators**: Clear visual feedback (Identical, Different, Left Only, Right Only, Error)
- **Structured Logging**: Comprehensive logging with Serilog to local application data folder
- **Extensible Architecture**: Plugin-ready design for future comparison algorithms and file type handlers

## Architecture

The application follows a clean, modular architecture with clear separation of concerns:

- **Differ.Core**: Business logic and domain models (UI-agnostic)
- **Differ.Common**: Cross-cutting concerns (logging infrastructure, shared utilities)
- **Differ.UI**: WPF views and ViewModels following MVVM pattern
- **Differ.App**: Application entry point and dependency injection setup
- **Differ.Tests**: Comprehensive unit and integration tests
- **Differ.Package**: Windows Application Packaging Project that builds MSIX installers

## System Requirements

- **Packaged release:** Windows 10 or later (no separate .NET install required).
- **Building from source:** .NET 8 SDK, Windows 10 or later.

## Building from Source

### Prerequisites

- .NET 8 SDK
- Visual Studio 2022 or Visual Studio Code

### Build Steps

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd differ
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build the solution:
   ```bash
   dotnet build
   ```

4. Run tests:
   ```bash
   dotnet test
   ```

5. Run the application:
   ```bash
   dotnet run --project src/Differ.App
   ```

## Usage

1. **Select Directories**: Use the "Browse..." buttons to select the left and right directories to compare
2. **Start Comparison**: Click "Compare Directories" to begin the analysis
3. **View Results**: The results grid shows all items with their comparison status:
   - 🟢 **Identical**: Files/folders are the same
   - 🔴 **Different**: Files have different content (click to view detailed diff)
   - 🔵 **Left Only**: Item exists only in the left directory
   - 🟡 **Right Only**: Item exists only in the right directory
   - 🟣 **Error**: Comparison failed due to an error
4. **View Differences**: Double-click any "Different" file to see line-by-line changes
5. **Cancel Anytime**: Use the Cancel button to stop long-running comparisons

## Key Design Principles

- **Simplicity First**: Minimize cognitive load with clean, purpose-driven UI
- **Async Operations**: All I/O operations are asynchronous with cancellation support
- **Memory Efficient**: Streams large files without loading them entirely into memory
- **Extensible**: Plugin architecture ready for future comparison algorithms
- **Reliable**: Comprehensive error handling and graceful degradation

## Contributing

1. Follow the coding conventions outlined in [`DESIGN_GUIDELINES.md`](DESIGN_GUIDELINES.md)
2. Ensure all tests pass and maintain >90% code coverage
3. Use the result pattern for error handling instead of exceptions
4. All I/O operations must be async with `CancellationToken` support

## Release Management

- Follow the [release playbook](docs/distribution/release-playbook.md) for tagging, packaging, and publishing builds.
- See the same playbook for QA, coverage targets, and manual validation steps.
- Start each release draft from `docs/RELEASE_NOTES_TEMPLATE.md` to keep notes consistent.
- Package builds manually using the commands in the playbook, then upload artifacts to GitHub Releases.
- Generate MSIX installers either by building the `Differ.Package` project in Visual Studio or by running `scripts/create-msix.ps1`.

## License

This project is licensed under the [MIT License](https://opensource.org/licenses/MIT).

*Thank you for trying Directory Differ! I look forward to your feedback.*
