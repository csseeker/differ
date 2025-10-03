# Logging Guide

This document summarises the logging refactor completed in October 2025 and explains how to work with the current Serilog setup.

## Architecture snapshot

- Logging is configured centrally in `Differ.Common/Logging/LoggingBuilderExtensions.cs`.
- The application uses Serilog with configuration-bound options (`DifferLogging` section in `appsettings.json`).
- `Differ.App/DifferApp.cs` wires Serilog during startup and persists fatal errors to `%LOCALAPPDATA%\Differ\Logs\startup-error.log` if initialisation fails.

## Key principles

- **Code-first configuration** – Serilog sinks and enrichers are configured in code for clarity; the old `ReadFrom.Configuration()` path was removed.
- **Options pattern** – `DifferLoggingOptions` binds structured configuration for minimum levels and overrides.
- **Guard clauses** – helper methods ensure log directories exist and avoid nested conditionals.
- **Result handling** – startup failures write to a local log file and fall back to stderr when necessary.

## Extending logging

### Adjusting minimum levels

Modify `appsettings.json` (or environment-specific variants):

```json
{
  "DifferLogging": {
    "MinimumLevel": "Information",
    "LevelOverrides": {
      "Microsoft": "Warning",
      "Differ.Core": "Debug"
    }
  }
}
```

The application reads these settings at startup. Runtime adjustments can use `IDifferLogLevelManager`.

### Adding a sink

Extend `UseDifferSerilog` (in `LoggingBuilderExtensions`) – for example, to add a rolling file sink:

```csharp
loggerConfiguration.WriteTo.File(
    path: Path.Combine(options.LogDirectory, "differ-.log"),
    rollingInterval: RollingInterval.Day,
    restrictedToMinimumLevel: LogEventLevel.Information);
```

Keep sinks focused and avoid unbounded retention. Update tests if you add new options or configuration keys.

### Accessing loggers

Inject `ILogger<T>` into classes via constructor injection:

```csharp
public class DirectoryScanner
{
    private readonly ILogger<DirectoryScanner> _logger;

    public DirectoryScanner(ILogger<DirectoryScanner> logger) => _logger = logger;

    public async Task ScanAsync(...) {
        _logger.LogInformation("Scanning {Path}", path);
        // ...
    }
}
```

## Testing & validation

- Unit tests live under `tests/Differ.Tests/Unit/Core/` and cover logging behaviour where relevant.
- When altering logging configuration, run `dotnet test` and ensure the application still launches without writing startup errors.

## Historical context

See `docs/history/notes/logging/REFACTORING.md` for the full refactor report (previously `LOGGING_REFACTORING.md`). The refactor removed unused template files, simplified configuration, and improved startup error handling.

## Related resources

- [Architecture overview](../overview/architecture.md)
- [Release playbook](../distribution/release-playbook.md) – references coverage expectations
- [Certificates & signing](../distribution/certificates.md) – when distributing signed builds
