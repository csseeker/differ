using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Differ.Core.Interfaces;
using Differ.Core.Models;
using Differ.UI.Models;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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

    private bool CanCompare => !string.IsNullOrWhiteSpace(LeftDirectoryPath) &&
                               !string.IsNullOrWhiteSpace(RightDirectoryPath) &&
                               !IsComparing;
    
    [ObservableProperty]
    private DirectoryComparisonResult? _comparisonResult;

    [ObservableProperty]
    private ComparisonSummary? _comparisonSummary;

    public ObservableCollection<UITreeViewItem> LeftDirectoryTree { get; } = new();
    public ObservableCollection<UITreeViewItem> RightDirectoryTree { get; } = new();

    public ObservableCollection<ComparisonItem> FilteredComparisonItems { get; } = new();

    private string? _currentFilter;

    public MainViewModel(
        IDirectoryComparisonService comparisonService,
        ILogger<MainViewModel> logger)
    {
        _comparisonService = comparisonService ?? throw new ArgumentNullException(nameof(comparisonService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    partial void OnLeftDirectoryPathChanged(string value)
    {
        CompareDirectoriesCommand.NotifyCanExecuteChanged();
    }

    partial void OnRightDirectoryPathChanged(string value)
    {
        CompareDirectoriesCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsComparingChanged(bool value)
    {
        CompareDirectoriesCommand.NotifyCanExecuteChanged();
        CancelComparisonCommand.NotifyCanExecuteChanged();
    }

    partial void OnComparisonResultChanged(DirectoryComparisonResult? value)
    {
        _currentFilter = null; // Reset filter
        ApplyFilter();

        if (value?.Summary != null)
        {
            ComparisonSummary = value.Summary;
        }
        else
        {
            ComparisonSummary = null;
        }

        LeftDirectoryTree.Clear();
        RightDirectoryTree.Clear();

        if (value?.Items != null)
        {
            var leftPaths = value.Items.Where(i => i.LeftItem != null).Select(i => i.RelativePath);
            BuildTree(leftPaths, LeftDirectoryTree);

            var rightPaths = value.Items.Where(i => i.RightItem != null).Select(i => i.RelativePath);
            BuildTree(rightPaths, RightDirectoryTree);
        }
    }

    [RelayCommand]
    private void FilterItems(string? category)
    {
        _currentFilter = category;
        ApplyFilter();
    }

    private void BuildTree(IEnumerable<string> paths, ObservableCollection<UITreeViewItem> rootNodes)
    {
        var allNodes = new Dictionary<string, UITreeViewItem>();

        foreach (var path in paths.OrderBy(p => p))
        {
            var parts = path.Split(Path.DirectorySeparatorChar);
            string currentPath = string.Empty;

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                var parentPath = currentPath;
                currentPath = i == 0 ? part : Path.Combine(currentPath, part);

                if (!allNodes.TryGetValue(currentPath, out var node))
                {
                    node = new UITreeViewItem { Name = part, FullPath = currentPath };
                    allNodes[currentPath] = node;

                    if (allNodes.TryGetValue(parentPath, out var parentNode))
                    {
                        if (!parentNode.Children.Any(c => c.FullPath == node.FullPath))
                            parentNode.Children.Add(node);
                    }
                    else
                    {
                        if (!rootNodes.Any(r => r.FullPath == node.FullPath))
                           rootNodes.Add(node);
                    }
                }
            }
        }
    }

    private void ApplyFilter()
    {
        FilteredComparisonItems.Clear();

        if (ComparisonResult?.Items == null)
        {
            return;
        }

        var itemsToShow = ComparisonResult.Items.AsEnumerable();

        if (!string.IsNullOrEmpty(_currentFilter))
        {
            if (Enum.TryParse<ComparisonStatus>(_currentFilter, out var status))
            {
                itemsToShow = itemsToShow.Where(i => i.Status == status);
            }
        }

        foreach (var item in itemsToShow)
        {
            FilteredComparisonItems.Add(item);
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

        var stopwatch = new Stopwatch();

        try
        {
            IsComparing = true;
            _cancellationTokenSource = new CancellationTokenSource();
            
            var progress = new Progress<string>(message => StatusMessage = message);
            
            _logger.LogInformation("Starting directory comparison");
            StatusMessage = "Starting comparison...";

            stopwatch.Start();

            var result = await _comparisonService.CompareDirectoriesAsync(
                LeftDirectoryPath,
                RightDirectoryPath,
                _cancellationTokenSource.Token,
                progress);

            stopwatch.Stop();

            if (result.IsSuccess)
            {
                ComparisonResult = result.Data;
                StatusMessage = $"Comparison completed in {stopwatch.Elapsed.TotalSeconds:F2} seconds. Found {result.Data!.Items.Count} items.";
                _logger.LogInformation("Directory comparison completed successfully in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
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
