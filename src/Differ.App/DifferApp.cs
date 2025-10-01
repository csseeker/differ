using Differ.Core.Interfaces;
using Differ.Core.Services;
using Differ.UI.Services;
using Differ.UI.ViewModels;
using Differ.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
                .ConfigureServices(ConfigureServices)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .Build();

            await _host.StartAsync();

            // Create and show the main window
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[DifferApp] Failed to start application: {ex}");
            try
            {
                var logPath = Path.Combine(AppContext.BaseDirectory, "startup-error.log");
                File.AppendAllText(logPath, $"[{DateTime.Now:O}] {ex}\n");
            }
            catch
            {
                // Ignore logging failures
            }
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

        base.OnExit(e);
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
