using Microsoft.Extensions.Options;
using Serilog.Events;
using Serilog.Core;

namespace Differ.Common.Logging;

/// <summary>
/// Allows runtime inspection and adjustment of the application's minimum log level.
/// </summary>
public interface IDifferLogLevelManager
{
    /// <summary>
    /// Gets the current minimum log level applied to the root logger.
    /// </summary>
    LogEventLevel CurrentLevel { get; }

    /// <summary>
    /// Sets the root minimum log level at runtime.
    /// </summary>
    /// <param name="level">The new minimum level.</param>
    void SetMinimumLevel(LogEventLevel level);

    /// <summary>
    /// Resets the root minimum log level to the configured value.
    /// </summary>
    void ResetToConfiguredLevel();
}

/// <summary>
/// Default implementation that wraps a Serilog <see cref="LoggingLevelSwitch"/>.
/// </summary>
internal sealed class DifferLogLevelManager : IDifferLogLevelManager
{
    private readonly LoggingLevelSwitch _levelSwitch;
    private readonly IOptionsMonitor<DifferLoggingOptions> _optionsMonitor;

    public DifferLogLevelManager(
        LoggingLevelSwitch levelSwitch,
        IOptionsMonitor<DifferLoggingOptions> optionsMonitor)
    {
        _levelSwitch = levelSwitch;
        _optionsMonitor = optionsMonitor;
    }

    /// <inheritdoc />
    public LogEventLevel CurrentLevel => _levelSwitch.MinimumLevel;

    /// <inheritdoc />
    public void SetMinimumLevel(LogEventLevel level)
    {
        _levelSwitch.MinimumLevel = level;
    }

    /// <inheritdoc />
    public void ResetToConfiguredLevel()
    {
        _levelSwitch.MinimumLevel = _optionsMonitor.CurrentValue.MinimumLevel;
    }
}
