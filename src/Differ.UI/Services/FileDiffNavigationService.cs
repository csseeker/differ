using Differ.UI.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Differ.UI.Services;

/// <summary>
/// Default implementation that wires up the file diff window using dependency injection.
/// </summary>
public class FileDiffNavigationService : IFileDiffNavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FileDiffNavigationService> _logger;

    public FileDiffNavigationService(IServiceProvider serviceProvider, ILogger<FileDiffNavigationService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ShowDiffAsync(string leftFilePath, string rightFilePath, string? displayName = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(leftFilePath))
        {
            throw new ArgumentException("Left file path is required.", nameof(leftFilePath));
        }

        if (string.IsNullOrWhiteSpace(rightFilePath))
        {
            throw new ArgumentException("Right file path is required.", nameof(rightFilePath));
        }

        var window = _serviceProvider.GetRequiredService<FileDiffWindow>();
        var viewModel = window.ViewModel;
        viewModel.Initialize(leftFilePath, rightFilePath, displayName);

        var owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
        if (owner != null && !ReferenceEquals(owner, window))
        {
            window.Owner = owner;
        }

        window.Show();
        window.Activate();

        try
        {
            await viewModel.LoadDiffAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("File diff loading was cancelled for {LeftFile} vs {RightFile}.", leftFilePath, rightFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while loading diff for {LeftFile} vs {RightFile}.", leftFilePath, rightFilePath);
            window.Close();
            throw;
        }
    }
}
