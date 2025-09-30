# Copilot Instructions for Differ

## Project Overview
Differ is a Windows directory and file comparison tool built with **WPF and .NET 8**, following **MVVM architecture** with clean separation of concerns. The project emphasizes simplicity, extensibility, and professional-grade architecture.

## Architecture Patterns

### Project Structure (Target)
```
Differ.Core/     # Business logic - UI-agnostic
Differ.UI/       # WPF views and ViewModels
Differ.Tests/    # Comprehensive test coverage
Differ.App/      # Entry point and DI composition
```

### MVVM Implementation
- **Views**: Pure XAML with minimal code-behind
- **ViewModels**: Handle UI logic, data binding, commands (use `ICommand` implementations)
- **Models**: Business entities and data structures
- **Services**: Business logic injected via built-in .NET DI container

### Key Interfaces to Implement
```csharp
IFileComparer       # Strategy pattern for different comparison algorithms
IDirectoryScanner   # Async directory traversal
IComparisonResult   # Result wrapper with success/error states
```

## Coding Conventions

### Async Patterns
- All I/O operations MUST be async: `ScanDirectoryAsync()`, `CompareFilesAsync()`
- Use `ConfigureAwait(false)` in library code
- Include `CancellationToken` for long-running operations
- Report progress for operations > 1 second

### Error Handling
Use result pattern instead of throwing exceptions for business logic:
```csharp
public class OperationResult<T>
{
    public bool IsSuccess { get; set; }
    public T Data { get; set; }
    public string ErrorMessage { get; set; }
}
```

### Naming Conventions
- Async methods: suffix with `Async` (`CompareFilesAsync`)
- Private fields: camelCase with underscore (`_fileComparer`)
- Interfaces: `I` prefix (`IFileComparer`)

## Performance Requirements

### File Operations
- **Stream large files** (>1MB) - never load entirely into memory
- **Process files concurrently** where independent
- **Fail gracefully** for extremely large files
- **Proper resource disposal** - use `using` statements

### UI Responsiveness
- **Never block UI thread** - all I/O on background threads
- **Use virtualization** for large file lists
- **Lazy load data** on demand
- **Provide cancellation** for long operations

## Testing Strategy

### Coverage Expectations
- **>90% coverage** on `Differ.Core` business logic
- **Mock external dependencies** (file system, etc.)
- **Test error conditions** and edge cases
- **Fast unit tests** (<100ms each)

### Test Organization
```
Differ.Tests/
├── Unit/Core/           # Business logic tests
├── Unit/UI/             # ViewModel tests
├── Integration/         # End-to-end scenarios
└── TestData/           # Sample files and fixtures
```

## Security & Validation

### File System Safety
- **Validate all paths** to prevent directory traversal
- **Handle access denied** gracefully
- **Sanitize user inputs** before file operations
- **Limit memory usage** with streaming

## Key Design Principles

1. **Simplicity First**: Minimize cognitive load, clean UI, essential features only
2. **Modularity**: Single responsibility, interface-driven, loose coupling
3. **Extensibility**: Plugin architecture for file types and comparison algorithms
4. **Performance**: Async-first, responsive UI, memory efficient
5. **Reliability**: Graceful error handling, proper resource management

## Current State
Project is in **design phase** with comprehensive guidelines established. When implementing:
- Start with `Differ.Core` business logic (UI-agnostic)
- Implement async file scanning and comparison services
- Build WPF UI with proper MVVM binding
- Use built-in .NET DI for service registration

Reference `DESIGN_GUIDELINES.md` for detailed architectural decisions and implementation standards.