using Differ.Common.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Differ.Common;

/// <summary>
/// Extension methods for wiring Differ's logging stack into an <see cref="IHostBuilder"/>.
/// </summary>
public static class LoggingBuilderExtensions
{
    /// <summary>
    /// Configures Serilog with Differ defaults, binding <see cref="DifferLoggingOptions"/> from configuration.
    /// </summary>
    /// <param name="builder">The host builder.</param>
    /// <param name="configure">Optional callback to tweak the options programmatically.</param>
    /// <returns>The original builder.</returns>
    public static IHostBuilder UseDifferLogging(
        this IHostBuilder builder,
        Action<DifferLoggingOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureServices((context, services) =>
        {
            // Bind options from configuration and allow optional overrides.
            var optionsBuilder = services
                .AddOptions<DifferLoggingOptions>()
                .Bind(context.Configuration.GetSection(DifferLoggingOptions.SectionName))
                .PostConfigure(EnsureLogDirectory);

            if (configure is not null)
            {
                optionsBuilder.Configure(configure);
            }

            // Register level switch for runtime control
            services.AddSingleton(CreateLoggingLevelSwitch);
            services.AddSingleton<IDifferLogLevelManager, DifferLogLevelManager>();
        });

        builder.UseSerilog((context, services, loggerConfiguration) =>
        {
            var options = services.GetRequiredService<IOptionsMonitor<DifferLoggingOptions>>().CurrentValue;
            var levelSwitch = services.GetRequiredService<LoggingLevelSwitch>();

            loggerConfiguration
                .MinimumLevel.ControlledBy(levelSwitch)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "Differ")
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);

            // Apply level overrides
            foreach (var (source, level) in options.LevelOverrides)
            {
                loggerConfiguration.MinimumLevel.Override(source, level);
            }

            // Configure sinks based on options
            if (options.EnableConsoleLogging)
            {
                loggerConfiguration.WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");
            }

            if (options.EnableDebugLogging)
            {
                loggerConfiguration.WriteTo.Debug(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");
            }

            if (options.EnableFileLogging)
            {
                loggerConfiguration.WriteTo.File(
                    options.GetLogFilePath(),
                    rollingInterval: options.RollingInterval,
                    retainedFileCountLimit: options.RetainedFileCountLimit,
                    fileSizeLimitBytes: options.FileSizeLimitBytes,
                    rollOnFileSizeLimit: true,
                    shared: options.UseSharedLogFile,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");
            }
        });

        return builder;
    }

    private static LoggingLevelSwitch CreateLoggingLevelSwitch(IServiceProvider provider)
    {
        var monitor = provider.GetRequiredService<IOptionsMonitor<DifferLoggingOptions>>();
        var levelSwitch = new LoggingLevelSwitch(monitor.CurrentValue.MinimumLevel);

        // Update level when configuration changes
        monitor.OnChange(options => levelSwitch.MinimumLevel = options.MinimumLevel);

        return levelSwitch;
    }

    private static void EnsureLogDirectory(DifferLoggingOptions options)
    {
        if (options.EnableFileLogging)
        {
            Directory.CreateDirectory(options.GetResolvedLogDirectory());
        }
    }
}
