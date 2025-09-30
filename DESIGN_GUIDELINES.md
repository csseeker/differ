# Directory Difference Tool - Design Guidelines

## Project Vision

Build a **clean, intuitive, and extensible** Windows application for comparing directories and files. The tool should be simple enough for anyone to use while maintaining professional-grade architecture that can evolve with future requirements.

## Core Principles

### 1. Simplicity First
- **User Experience**: Minimize clicks and cognitive load
- **Interface Design**: Clean, uncluttered, purpose-driven UI
- **Feature Scope**: Start with essential features, add complexity only when justified
- **Documentation**: Self-explanatory interfaces with minimal learning curve

### 2. Modularity & Separation of Concerns
- **Clear Boundaries**: Each component has a single, well-defined responsibility
- **Interface-Driven**: Program to interfaces, not implementations
- **Loose Coupling**: Components interact through well-defined contracts
- **High Cohesion**: Related functionality grouped together logically

### 3. Extensibility Without Over-Engineering
- **Plugin Architecture**: Support for future file type comparers
- **Configuration System**: Extensible settings without UI bloat
- **Strategy Patterns**: Swappable algorithms for different comparison needs
- **Event-Driven**: Extensible through events and observers

### 4. Performance & Responsiveness
- **Async First**: All I/O operations must be asynchronous
- **Responsive UI**: Never block the UI thread
- **Memory Efficient**: Stream large files, dispose resources properly
- **Cancellable Operations**: Users can cancel long-running operations

### 5. Reliability & Robustness
- **Error Handling**: Graceful degradation with meaningful error messages
- **Input Validation**: Validate all user inputs and file operations
- **Resource Management**: Proper disposal of file handles and streams
- **Testing**: Comprehensive unit and integration test coverage

## Architecture Guidelines

### MVVM Pattern Implementation
```
View (XAML) ↔ ViewModel (C#) ↔ Model/Service (C#)
```

- **Views**: Pure XAML with minimal code-behind
- **ViewModels**: Handle UI logic, data binding, and commands
- **Models**: Business entities and data structures
- **Services**: Business logic and external interactions

### Dependency Injection Strategy
- Use built-in .NET DI container for simplicity
- Register services with appropriate lifetimes
- Prefer constructor injection over service locator
- Keep dependencies explicit and testable

### Project Structure Rules
```
Differ.Core/          # Business logic - no UI dependencies
Differ.UI/            # WPF-specific code - no business logic
Differ.Tests/         # Comprehensive test coverage
Differ.App/           # Application entry point and composition
```

## Code Quality Standards

### Naming Conventions
- **Classes**: PascalCase (`DirectoryScanner`)
- **Methods**: PascalCase (`ScanDirectoryAsync`)
- **Properties**: PascalCase (`ComparisonResult`)
- **Fields**: camelCase with underscore (`_fileComparer`)
- **Interfaces**: PascalCase with 'I' prefix (`IFileComparer`)
- **Async Methods**: Suffix with 'Async' (`CompareFilesAsync`)

### Code Organization
- **File Per Class**: One primary class per file
- **Namespace Alignment**: Namespace matches folder structure
- **Using Statements**: Sort and remove unused
- **Region Usage**: Minimal - prefer good class design
- **Documentation**: XML comments for public APIs

### Error Handling Strategy
```csharp
// Use exceptions for exceptional cases
// Return results with status for business logic
public class OperationResult<T>
{
    public bool IsSuccess { get; set; }
    public T Data { get; set; }
    public string ErrorMessage { get; set; }
}
```

### Async/Await Best Practices
- Always use `ConfigureAwait(false)` in library code
- Prefer `Task` over `void` for async methods
- Use `CancellationToken` for long-running operations
- Report progress for operations > 1 second

## User Interface Guidelines

### Design Principles
- **Consistency**: Use consistent spacing, colors, and typography
- **Accessibility**: Support keyboard navigation and screen readers
- **Responsiveness**: UI adapts to different window sizes
- **Feedback**: Clear visual feedback for all user actions

### Layout Standards
- **Spacing**: Use consistent margins (8px, 16px, 24px grid)
- **Typography**: Limited font sizes with clear hierarchy
- **Colors**: Semantic color usage (success, warning, error)
- **Icons**: Consistent icon style and sizing

### Interaction Patterns
- **Progressive Disclosure**: Show details on demand
- **Immediate Feedback**: Visual response to all clicks
- **Undo/Redo**: Where applicable for destructive actions
- **Shortcuts**: Keyboard shortcuts for power users

## Performance Guidelines

### File Operations
- **Streaming**: Use streams for files > 1MB
- **Buffering**: Appropriate buffer sizes for I/O
- **Parallelism**: Process independent files concurrently
- **Memory Limits**: Fail gracefully for extremely large files

### UI Performance
- **Virtualization**: Use virtual lists for large datasets
- **Lazy Loading**: Load data on demand
- **Background Processing**: Keep UI thread free
- **Efficient Binding**: Minimize property change notifications

## Testing Strategy

### Unit Testing
- **Coverage**: Aim for >90% code coverage on core logic
- **Isolation**: Mock external dependencies
- **Scenarios**: Test happy path, edge cases, and error conditions
- **Fast**: Unit tests should run quickly (<100ms each)

### Integration Testing
- **End-to-End**: Test complete user scenarios
- **File System**: Test with real files and directories
- **UI Testing**: Test critical user workflows
- **Performance**: Test with large datasets

### Test Organization
```
Differ.Tests/
├── Unit/
│   ├── Core/           # Core business logic tests
│   └── UI/             # ViewModel tests
├── Integration/
│   ├── FileSystem/     # File operation tests
│   └── Scenarios/      # End-to-end tests
└── TestData/
    ├── SampleFiles/    # Test files
    └── Fixtures/       # Test data builders
```

## Security Considerations

### File System Access
- **Path Validation**: Prevent directory traversal attacks
- **Permission Handling**: Graceful handling of access denied
- **Sandboxing**: Limit file system access scope
- **Input Sanitization**: Clean all user-provided paths

### Memory Safety
- **Disposal**: Proper resource cleanup
- **Buffer Limits**: Prevent memory exhaustion
- **Exception Safety**: No resource leaks on exceptions

## Deployment & Distribution

### Build Requirements
- **Self-Contained**: No external .NET runtime dependency
- **Single File**: Option for portable executable
- **Installer**: MSI package for system integration
- **Signing**: Code signing for trust and security

### Versioning Strategy
- **Semantic Versioning**: MAJOR.MINOR.PATCH format
- **Backward Compatibility**: Maintain settings compatibility
- **Update Mechanism**: In-app update notifications (future)

## Future Extensibility Points

### Planned Extension Areas
1. **File Type Support**: Binary files, images, documents
2. **Comparison Algorithms**: Different diff algorithms
3. **Export Formats**: HTML, JSON, XML reports
4. **Integration**: Command-line interface, API
5. **Collaboration**: Share comparison results

### Extension Guidelines
- **Plugin Interface**: Well-defined plugin contracts
- **Configuration**: Extensible settings system
- **UI Extensions**: Pluggable UI components
- **Documentation**: Clear extension development guide

## Decision Log

### Key Architectural Decisions
| Decision | Rationale | Alternatives Considered |
|----------|-----------|------------------------|
| WPF over WinUI 3 | Mature, stable, better tooling | WinUI 3 (too new), Avalonia (cross-platform not needed) |
| .NET 8 | LTS support, performance | .NET Framework (legacy), .NET 6 (shorter support) |
| Built-in DI | Simplicity, no external deps | Unity, Autofac (overkill for this project) |
| MVVM | Testability, separation | Code-behind (not maintainable), MVP (more complex) |

### Standards Review
- **Review Frequency**: These guidelines should be reviewed monthly
- **Update Process**: Updates require team consensus
- **Compliance**: All code reviews should verify guideline adherence
- **Metrics**: Track adherence through automated tools where possible

---

## Implementation Checklist

Before starting any development:
- [ ] Read and understand these guidelines
- [ ] Set up development environment according to standards
- [ ] Configure IDE with appropriate code style settings
- [ ] Set up automated testing framework
- [ ] Establish code review process

Before each feature:
- [ ] Design interfaces before implementation
- [ ] Write tests for new functionality
- [ ] Consider extensibility implications
- [ ] Review performance impact
- [ ] Update documentation

Before each release:
- [ ] Run full test suite
- [ ] Performance testing with large datasets
- [ ] UI/UX review
- [ ] Security review
- [ ] Documentation updates

---

*These guidelines are living documents that evolve with the project. They serve as our compass to ensure we build something we can be proud of - simple, robust, and extensible.*