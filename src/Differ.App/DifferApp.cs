using Differ.Common;
using Differ.Core.Interfaces;
using Differ.Core.Services;
using Differ.UI.Services;
using Differ.UI.ViewModels;
using Differ.UI.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Windows;

/// <summary>
/// Custom Application class
/// </summary>
public class DifferApp : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            // Build the host with dependency injection
            _host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
                        .AddEnvironmentVariables(prefix: "DIFFER_");
                })
                .UseDifferLogging()
                .ConfigureServices(ConfigureServices)
                .Build();

            await _host.StartAsync();

            // Create and show the main window
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            // Fallback logging before DI is set up
            LogStartupError(ex);

            MessageBox.Show(
                $"Failed to start application: {ex.Message}",
                "Application Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown(1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        Log.CloseAndFlush();

        base.OnExit(e);
    }

    private static void LogStartupError(Exception ex)
    {
        try
        {
            var logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Differ",
                "Logs",
                "startup-error.log");

            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
            File.AppendAllText(logPath, $"[{DateTime.Now:O}] {ex}\n");
        }
        catch
        {
            // If we can't log to file, write to console as last resort
            Console.Error.WriteLine($"[CRITICAL] Startup failed and couldn't write log: {ex}");
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Register core services
        services.AddSingleton<IDirectoryScanner, DirectoryScanner>();
        services.AddSingleton<IFileComparer, HashFileComparer>();
        services.AddSingleton<IDirectoryComparisonService, DirectoryComparisonService>();
        services.AddSingleton<ITextDiffService, TextDiffService>();

        // Register UI services
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();
        services.AddTransient<FileDiffViewModel>();
        services.AddTransient<FileDiffWindow>();
        services.AddSingleton<IFileDiffNavigationService, FileDiffNavigationService>();
    }
}
