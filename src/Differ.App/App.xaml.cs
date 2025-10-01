using Differ.Core.Interfaces;
using Differ.Core.Services;
using Differ.UI.Resources;
using Differ.UI.ViewModels;
using Differ.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace Differ.App;

/// <summary>
/// Application entry point
/// </summary>
public class App : Application
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
            System.Windows.MessageBox.Show(
                AppMessages.ApplicationStartupFailed(ex.Message),
                AppMessages.ApplicationErrorTitle,
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

        // Register UI services
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();
    }
}
