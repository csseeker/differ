# Architecture Overview

Differ follows a layered, MVVM-oriented architecture that keeps business logic, presentation, and composition cleanly separated.

## Solution layout

| Project | Responsibilities | Notes |
| --- | --- | --- |
| `Differ.Core` | Directory scanning, file comparison strategies, shared models | Pure .NET library – no WPF dependencies, async-first APIs, result pattern for error handling. |
| `Differ.UI` | Views, ViewModels, converters, and UI-specific models | Implements MVVM, keeps code-behind minimal. |
| `Differ.App` | WPF application bootstrap, dependency injection, logging | Wires services, configures logging, owns app-level resources. |
| `Differ.Tests` | Unit and integration tests | High coverage for core logic, mocks external dependencies, fast feedback. |
| `Differ.Package` | MSIX packaging assets | Provides manifests and images for installer builds. |

Additional shared utilities live in `Differ.Common` (logging helpers, cross-cutting infrastructure).

## Core principles

- **Simplicity first** – expose essential functionality with minimal UI friction.
- **Separation of concerns** – UI logic in ViewModels, business rules in services, infrastructure in dedicated layers.
- **Async everywhere** – directory scans, file reads, and comparisons are asynchronous and cancellable.
- **Result-based error handling** – business logic returns `OperationResult<T>` instead of throwing for expected failures.
- **Extensibility** – comparison algorithms implement interfaces (`IFileComparer`, `IDirectoryScanner`, etc.) so you can swap or extend strategies.
- **Resource safety** – large files are streamed, IO is disposed deterministically, cancellation tokens are honoured.

## Dependency injection

`Differ.App` uses the built-in .NET DI container:

- Register services via `IServiceCollection` in `DifferApp.ConfigureServices`.
- Inject dependencies through constructors; avoid service locators.
- Use scoped or transient lifetimes for stateless services, singletons for configuration and logging.

## Async patterns & responsiveness

- Every long-running operation accepts a `CancellationToken` and reports progress.
- UI updates are marshalled back to the dispatcher to keep WPF responsive.
- Background work uses `Task.Run` sparingly; prefer naturally async APIs.

## Logging

- Based on Serilog with configuration-bound options in `Differ.Common`.
- Detailed reference: [Logging guide](../engineering/logging.md).

## Tests & quality

- Target >90% coverage in `Differ.Core`.
- Tests live under `tests/Differ.Tests` with `Unit/Core`, `Unit/UI`, and `Integration` breakdown.
- Coverage expectations and recent metrics: see the [Release playbook](../distribution/release-playbook.md#quality-gates).

## Further reading

- Full implementation standards live in [`DESIGN_GUIDELINES.md`](../DESIGN_GUIDELINES.md).
- UI and branding guidelines: [Branding & icons](../branding/icons.md).
- Packaging and release process: [Release playbook](../distribution/release-playbook.md).
