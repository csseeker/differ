# Directory Differ

A clean, intuitive Windows application for comparing directories and files, built with WPF and .NET 8.

## Download

- Grab the latest `Differ-v*-win-x64.zip` from the [releases page](https://github.com/csseeker/differ/releases).
- Unzip anywhere you have write access.
- Double-click `DifferApp.exe` (Windows may prompt with SmartScreen; choose **Run anyway**).
- Optional: pin the executable to Start or Taskbar for quick access.
- If a `.msix` package is available, install it for Start menu integration (see `docs/MSIX_PACKAGING.md`).

## Features

- **Directory Comparison**: Compare two directories and identify differences
- **File Analysis**: Hash-based file comparison for accurate difference detection
- **Intuitive UI**: Clean WPF interface with real-time progress reporting
- **Extensible Architecture**: Plugin-ready design for future enhancements

## Architecture

The application follows a clean, modular architecture with clear separation of concerns:

- **Differ.Core**: Business logic and domain models (UI-agnostic)
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
   - 🔴 **Different**: Files have different content
   - 🔵 **Left Only**: Item exists only in the left directory
   - 🟡 **Right Only**: Item exists only in the right directory
   - 🟣 **Error**: Comparison failed due to an error

## Key Design Principles

- **Simplicity First**: Minimize cognitive load with clean, purpose-driven UI
- **Async Operations**: All I/O operations are asynchronous with cancellation support
- **Memory Efficient**: Streams large files without loading them entirely into memory
- **Extensible**: Plugin architecture ready for future comparison algorithms
- **Reliable**: Comprehensive error handling and graceful degradation

## Contributing

1. Follow the coding conventions outlined in `DESIGN_GUIDELINES.md`
2. Ensure all tests pass and maintain >90% code coverage
3. Use the result pattern for error handling instead of exceptions
4. All I/O operations must be async with `CancellationToken` support

## Release Management

- Follow the playbook in `docs/RELEASE.md` for tagging and shipping builds.
- Use `docs/QA.md` as the pre-release validation checklist.
- Start each release draft from `docs/RELEASE_NOTES_TEMPLATE.md` to keep notes consistent.
- Package builds manually using the commands in the release playbook, then upload artifacts to GitHub Releases.
- Generate MSIX installers either by building the `Differ.Package` project in Visual Studio or by running `scripts/create-msix.ps1`.

## License

[Add your license information here]