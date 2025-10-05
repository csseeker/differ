# Code Coverage Documentation

## Running Tests with Coverage

### Quick Command
```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory:TestResults --settings:coverlet.runsettings
```

### Generate HTML Report
```bash
reportgenerator -reports:"TestResults/**/*.cobertura.xml" -targetdir:"TestResults/coverage-report" -reporttypes:"Html;TextSummary"
```

Then open `TestResults/coverage-report/index.html` in your browser.

## Current Coverage Status

**Last Updated:** October 4, 2025  
**Note:** Coverage should be re-measured and updated after each release or significant code changes.

- **Overall Coverage:** 40.2%
- **Line Coverage:** 707/1758 lines
- **Branch Coverage:** 32.4%
- **Total Tests:** 80

### By Project

| Project | Coverage | Status |
|---------|----------|--------|
| **Differ.Core** | 89.5% | ✅ Excellent |
| **Differ.Common** | 25% | ⚠️ Needs Work |
| **Differ.UI** | 0% | ❌ Not Tested |

### Differ.Core Details
- All models: **100% coverage** ✅
- `TextDiffService`: 93.5%
- `DirectoryComparisonService`: 92.7%
- `HashFileComparer`: 79.7%
- `DirectoryScanner`: 73%

## Configuration

### coverlet.runsettings

The `coverlet.runsettings` file at the repository root configures:
- Exclusion of auto-generated files (*.g.cs)
- Exclusion of Designer files
- Skip auto-properties
- Cobertura format output

### Known Issues

#### 1. Source Generator File Warnings

**Symptom:** ReportGenerator shows warnings like:
```
File 'Differ.UI.ViewModels.MainViewModel.CancelComparison.g.cs' does not exist (any more).
```

**Cause:** CommunityToolkit.Mvvm source generators create temporary .g.cs files during compilation that get cleaned up after the build. Coverlet references them in the coverage report, but they're gone by the time ReportGenerator runs.

**Impact:** **None** - This is purely cosmetic. The coverage data is accurate, and the warnings don't affect the report quality. The generated command wrapper code shouldn't be tested anyway.

**Why It Happens:**
1. Build compiles `MainViewModel` 
2. Source generators create `*.g.cs` files in `obj/` folder
3. Coverlet records coverage for these files
4. Build completes and cleans up intermediate files
5. ReportGenerator can't find the now-deleted files

**Attempted Fixes:**
- ExcludeByFile patterns in runsettings (partial success)
- The files are still referenced because they're compiled into the assembly

**Recommendation:** **Ignore these warnings** - they're harmless and expected behavior with source generators.

#### 2. Flaky Progress Test

**File:** `TextDiffServiceAdditionalTests.cs`
**Test:** `ComputeDiffAsync_ShouldReportProgress`

**Issue:** The test occasionally fails because `Progress<T>` callbacks are asynchronous and the 100ms delay isn't always sufficient for the synchronization context to process the final callback.

**Solution:** See fixes below.

## Improving Coverage

### Priority 1: Fix Flaky Test
The `ComputeDiffAsync_ShouldReportProgress` test needs to be more robust:

```csharp
// Instead of fixed delay
await Task.Delay(100);

// Use polling with timeout
var timeout = TimeSpan.FromSeconds(2);
var pollInterval = TimeSpan.FromMilliseconds(10);
var startTime = DateTime.UtcNow;

while (!progressReports.Contains(1.0) && (DateTime.UtcNow - startTime) < timeout)
{
    await Task.Delay(pollInterval);
}

progressReports.Should().Contain(1.0, "progress should complete at 100%");
```

### Priority 2: Add ViewModel Tests
The UI layer needs test coverage for:
- `MainViewModel` - directory selection, comparison logic, cancellation
- `FileDiffViewModel` - diff display logic, navigation
- `AboutViewModel` - simple property tests

Use Moq to mock:
- `IDirectoryComparisonService`
- `ITextDiffService`
- `IDialogService`

### Priority 3: Test Logging Infrastructure
Add tests for:
- `LoggingBuilderExtensions` configuration
- `DifferLogLevelManager` behavior

## CI/CD Integration

### GitHub Actions (Future)

```yaml
- name: Run Tests with Coverage
  run: dotnet test --collect:"XPlat Code Coverage" --settings:coverlet.runsettings
  
- name: Generate Coverage Report
  run: reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:"Html;Cobertura"
  
- name: Upload Coverage  
  uses: codecov/codecov-action@v3
  with:
    files: ./coverage-report/Cobertura.xml
```

### Coverage Badges

Once CI is set up, add badges to README:
- Overall coverage %
- Core library coverage %
- Test count

## Best Practices

1. **Maintain >90% coverage on Differ.Core** - This is your critical business logic
2. **Test ViewModels independently** - Mock all services
3. **Don't test auto-generated code** - Source generator output doesn't need coverage
4. **Ignore Designer files** - WPF designer-generated code should be excluded
5. **Test error paths** - Ensure exception handling and failure scenarios are covered
6. **Use async properly** - For Progress<T> tests, use polling instead of fixed delays

## Tools

- **Coverlet** - Cross-platform code coverage library for .NET
- **ReportGenerator** - Creates human-readable reports from coverage data
- **XUnit** - Testing framework
- **FluentAssertions** - Assertion library
- **Moq** - Mocking framework

## Resources

- [Coverlet Documentation](https://github.com/coverlet-coverage/coverlet)
- [ReportGenerator Documentation](https://github.com/danielpalme/ReportGenerator)
- [Unit Testing Best Practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
