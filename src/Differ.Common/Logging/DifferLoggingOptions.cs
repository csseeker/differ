using Serilog;
using Serilog.Events;

namespace Differ.Common.Logging;

/// <summary>
/// Strongly-typed options for configuring Differ's logging pipeline.
/// </summary>
public class DifferLoggingOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "DifferLogging";

    /// <summary>
    /// Gets or sets the directory where log files should be written.
    /// </summary>
    public string LogDirectory { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Differ",
        "Logs");

    /// <summary>
    /// Gets or sets the file name template (supports Serilog rolling tokens like "differ-.log").
    /// </summary>
    public string LogFileName { get; set; } = "differ-.log";

    /// <summary>
    /// Gets or sets a value indicating whether file logging is enabled.
    /// </summary>
    public bool EnableFileLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether console logging is enabled.
    /// </summary>
    public bool EnableConsoleLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether debug output logging (System.Diagnostics.Debug) is enabled.
    /// </summary>
    public bool EnableDebugLogging { get; set; } = false;

    /// <summary>
    /// Gets or sets the rolling interval for file logs.
    /// </summary>
    public RollingInterval RollingInterval { get; set; } = RollingInterval.Day;

    /// <summary>
    /// Gets or sets the maximum number of retained log files. Set null to keep all.
    /// </summary>
    public int? RetainedFileCountLimit { get; set; } = 7;

    /// <summary>
    /// Gets or sets the log file size limit in bytes before rolling.
    /// </summary>
    public long? FileSizeLimitBytes { get; set; } = 10 * 1024 * 1024; // 10 MB

    /// <summary>
    /// Gets or sets a value indicating whether log files should be shared between processes.
    /// </summary>
    public bool UseSharedLogFile { get; set; } = true;

    /// <summary>
    /// Gets or sets the minimum log event level for the application.
    /// </summary>
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;

    /// <summary>
    /// Gets or sets log level overrides (e.g. { "Microsoft": "Warning" }).
    /// </summary>
    public Dictionary<string, LogEventLevel> LevelOverrides { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the fully qualified log file path based on the current options.
    /// </summary>
    public string GetLogFilePath()
    {
        if (string.IsNullOrWhiteSpace(LogFileName))
        {
            throw new InvalidOperationException("LogFileName cannot be null or empty.");
        }

        var directory = GetResolvedLogDirectory();
        return Path.Combine(directory, LogFileName);
    }

    /// <summary>
    /// Resolves <see cref="LogDirectory"/> into an absolute path, expanding environment variables.
    /// </summary>
    /// <returns>The absolute log directory path.</returns>
    public string GetResolvedLogDirectory()
    {
        if (string.IsNullOrWhiteSpace(LogDirectory))
        {
            throw new InvalidOperationException("LogDirectory cannot be null or empty.");
        }

        var expanded = Environment.ExpandEnvironmentVariables(LogDirectory);
        if (string.IsNullOrWhiteSpace(expanded))
        {
            throw new InvalidOperationException("LogDirectory expands to an empty value.");
        }

        if (!Path.IsPathRooted(expanded))
        {
            expanded = Path.Combine(AppContext.BaseDirectory, expanded);
        }

        return Path.GetFullPath(expanded);
    }
}
