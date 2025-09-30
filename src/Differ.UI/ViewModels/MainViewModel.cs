using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Differ.Core.Interfaces;
using Differ.Core.Models;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows;

namespace Differ.UI.ViewModels;

/// <summary>
/// Main view model for the directory comparison application
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IDirectoryComparisonService _comparisonService;
    private readonly ILogger<MainViewModel> _logger;
    private CancellationTokenSource? _cancellationTokenSource;

    [ObservableProperty]
    private string _leftDirectoryPath = string.Empty;

    [ObservableProperty]
    private string _rightDirectoryPath = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _isComparing = false;

    [ObservableProperty]
    private bool _canCompare = false;

    [ObservableProperty]
    private DirectoryComparisonResult? _comparisonResult;

    [ObservableProperty]
    private ComparisonSummary? _comparisonSummary;

    public ObservableCollection<ComparisonItem> ComparisonItems { get; } = new();

    public MainViewModel(
        IDirectoryComparisonService comparisonService,
        ILogger<MainViewModel> logger)
    {
        _comparisonService = comparisonService ?? throw new ArgumentNullException(nameof(comparisonService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    partial void OnLeftDirectoryPathChanged(string value)
    {
        UpdateCanCompare();
    }

    partial void OnRightDirectoryPathChanged(string value)
    {
        UpdateCanCompare();
    }

    partial void OnComparisonResultChanged(DirectoryComparisonResult? value)
    {
        ComparisonItems.Clear();
        if (value?.Items != null)
        {
            foreach (var item in value.Items)
            {
                ComparisonItems.Add(item);
            }
            ComparisonSummary = value.Summary;
        }
        else
        {
            ComparisonSummary = null;
        }
    }

    [RelayCommand]
    private void BrowseLeftDirectory()
    {
        var selectedPath = BrowseForDirectory("Select Left Directory");
        if (!string.IsNullOrEmpty(selectedPath))
        {
            LeftDirectoryPath = selectedPath;
        }
    }

    [RelayCommand]
    private void BrowseRightDirectory()
    {
        var selectedPath = BrowseForDirectory("Select Right Directory");
        if (!string.IsNullOrEmpty(selectedPath))
        {
            RightDirectoryPath = selectedPath;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCompare))]
    private async Task CompareDirectoriesAsync()
    {
        if (IsComparing)
            return;

        try
        {
            IsComparing = true;
            _cancellationTokenSource = new CancellationTokenSource();
            
            var progress = new Progress<string>(message => StatusMessage = message);
            
            _logger.LogInformation("Starting directory comparison");
            StatusMessage = "Starting comparison...";

            var result = await _comparisonService.CompareDirectoriesAsync(
                LeftDirectoryPath,
                RightDirectoryPath,
                _cancellationTokenSource.Token,
                progress);

            if (result.IsSuccess)
            {
                ComparisonResult = result.Data;
                StatusMessage = $"Comparison completed. Found {result.Data!.Items.Count} items.";
                _logger.LogInformation("Directory comparison completed successfully");
            }
            else
            {
                StatusMessage = $"Comparison failed: {result.ErrorMessage}";
                _logger.LogError("Directory comparison failed: {Error}", result.ErrorMessage);
                
                System.Windows.MessageBox.Show(
                    $"Comparison failed: {result.ErrorMessage}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Comparison cancelled";
            _logger.LogInformation("Directory comparison was cancelled");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Unexpected error: {ex.Message}";
            _logger.LogError(ex, "Unexpected error during directory comparison");
            
            System.Windows.MessageBox.Show(
                $"An unexpected error occurred: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            IsComparing = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    [RelayCommand(CanExecute = nameof(IsComparing))]
    private void CancelComparison()
    {
        _cancellationTokenSource?.Cancel();
        StatusMessage = "Cancelling comparison...";
    }

    private void UpdateCanCompare()
    {
        CanCompare = !string.IsNullOrWhiteSpace(LeftDirectoryPath) && 
                     !string.IsNullOrWhiteSpace(RightDirectoryPath) && 
                     !IsComparing;
    }

    private static string? BrowseForDirectory(string description)
    {
        try
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = description,
                Multiselect = false
            };

            var result = dialog.ShowDialog();
            return result == true ? dialog.FolderName : null;
        }
        catch (Exception)
        {
            // Fallback to a simple message if folder browser fails
            System.Windows.MessageBox.Show(
                "Unable to open folder browser. Please type the path manually.",
                "Browse Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return null;
        }
    }
}
