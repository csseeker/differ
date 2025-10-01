using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Differ.Core.Interfaces;
using Differ.Core.Models;
using Differ.UI.Models;
using Differ.UI.Resources;
using Differ.UI.Services;
using Differ.UI.Views;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Differ.UI.ViewModels;

/// <summary>
/// Main view model for the directory comparison application
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IDirectoryComparisonService _comparisonService;
    private readonly ILogger<MainViewModel> _logger;
    private readonly IFileDiffNavigationService _fileDiffNavigationService;
    private readonly IDialogService _dialogService;
    private CancellationTokenSource? _cancellationTokenSource;

    [ObservableProperty]
    private string _leftDirectoryPath = string.Empty;

    [ObservableProperty]
    private string _rightDirectoryPath = string.Empty;

    [ObservableProperty]
    private string _statusMessage = AppMessages.Ready;

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

    private readonly Dictionary<string, UITreeViewItem> _leftTreeNodes = new();
    private readonly Dictionary<string, UITreeViewItem> _rightTreeNodes = new();
    private bool _isSynchronizingSelection;

    public ObservableCollection<ComparisonItem> FilteredComparisonItems { get; } = new();

    private string? _currentFilter;

    public MainViewModel(
        IDirectoryComparisonService comparisonService,
        ILogger<MainViewModel> logger,
        IFileDiffNavigationService fileDiffNavigationService,
        IDialogService dialogService)
    {
        _comparisonService = comparisonService ?? throw new ArgumentNullException(nameof(comparisonService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileDiffNavigationService = fileDiffNavigationService ?? throw new ArgumentNullException(nameof(fileDiffNavigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
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
        _leftTreeNodes.Clear();
        _rightTreeNodes.Clear();

        if (value?.Items != null)
        {
            var leftPaths = value.Items.Where(i => i.LeftItem != null).Select(i => i.RelativePath);
            BuildTree(leftPaths, LeftDirectoryTree, _leftTreeNodes);

            var rightPaths = value.Items.Where(i => i.RightItem != null).Select(i => i.RelativePath);
            BuildTree(rightPaths, RightDirectoryTree, _rightTreeNodes);
        }
    }

    [RelayCommand]
    private void FilterItems(string? category)
    {
        _currentFilter = category;
        ApplyFilter();
    }

    private void BuildTree(IEnumerable<string> paths, ObservableCollection<UITreeViewItem> rootNodes, Dictionary<string, UITreeViewItem> allNodes)
    {
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
                    node.PropertyChanged += OnTreeViewItemPropertyChanged;
                    allNodes[currentPath] = node;

                    if (allNodes.TryGetValue(parentPath, out var parentNode))
                    {
                        node.Parent = parentNode;
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

    private void OnTreeViewItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_isSynchronizingSelection) return;
        if (sender is not UITreeViewItem changedItem) return;

        _isSynchronizingSelection = true;
        try
        {
            switch (e.PropertyName)
            {
                case nameof(UITreeViewItem.IsSelected):
                    if (changedItem.IsSelected)
                    {
                        SynchronizeSelection(changedItem);
                    }
                    break;

                case nameof(UITreeViewItem.IsExpanded):
                    SynchronizeExpansion(changedItem);
                    break;
            }
        }
        finally
        {
            _isSynchronizingSelection = false;
        }
    }

    private void SynchronizeSelection(UITreeViewItem selectedItem)
    {
        var counterpart = GetCounterpart(selectedItem);

        // Deselect all other items in both trees
        DeselectAll(_leftTreeNodes.Values);
        DeselectAll(_rightTreeNodes.Values);

        // Reselect the original item
        selectedItem.IsSelected = true;

        if (counterpart != null)
        {
            counterpart.IsSelected = true;
            ExpandToItem(counterpart);
        }
    }

    private void SynchronizeExpansion(UITreeViewItem expandedItem)
    {
        var counterpart = GetCounterpart(expandedItem);

        if (counterpart != null)
        {
            counterpart.IsExpanded = expandedItem.IsExpanded;
        }
    }

    private UITreeViewItem? GetCounterpart(UITreeViewItem sourceItem)
    {
        if (_leftTreeNodes.TryGetValue(sourceItem.FullPath, out var leftNode) && ReferenceEquals(leftNode, sourceItem))
        {
            return _rightTreeNodes.TryGetValue(sourceItem.FullPath, out var rightMatch) ? rightMatch : null;
        }

        if (_rightTreeNodes.TryGetValue(sourceItem.FullPath, out var rightNode) && ReferenceEquals(rightNode, sourceItem))
        {
            return _leftTreeNodes.TryGetValue(sourceItem.FullPath, out var leftMatch) ? leftMatch : null;
        }

        return null;
    }

    private void DeselectAll(IEnumerable<UITreeViewItem> nodes)
    {
        foreach (var node in nodes)
        {
            node.IsSelected = false;
        }
    }

    private void ExpandToItem(UITreeViewItem? item)
    {
        if (item == null) return;

        var current = item.Parent;
        while (current != null)
        {
            current.IsExpanded = true;
            current = current.Parent;
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

    [RelayCommand]
    private async Task OpenDiffAsync(ComparisonItem? comparisonItem)
    {
        if (comparisonItem is null)
        {
            return;
        }

        if (comparisonItem.Status != ComparisonStatus.Different)
        {
            return;
        }

        if (comparisonItem.LeftItem?.IsDirectory == true || comparisonItem.RightItem?.IsDirectory == true)
        {
            StatusMessage = AppMessages.DiffUnavailableForDirectories;
            return;
        }

        var leftPath = comparisonItem.LeftItem?.FullPath;
        var rightPath = comparisonItem.RightItem?.FullPath;

        if (string.IsNullOrWhiteSpace(leftPath) || string.IsNullOrWhiteSpace(rightPath))
        {
            StatusMessage = AppMessages.DiffFilesNotFound;
            return;
        }

        try
        {
            StatusMessage = AppMessages.OpeningDiff(comparisonItem.RelativePath);
            await _fileDiffNavigationService.ShowDiffAsync(leftPath, rightPath, comparisonItem.RelativePath);
            StatusMessage = AppMessages.DiffOpened(comparisonItem.RelativePath);
        }
        catch (OperationCanceledException)
        {
            StatusMessage = AppMessages.DiffCancelled;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open diff window for {RelativePath}", comparisonItem.RelativePath);
            StatusMessage = AppMessages.DiffFailed;
            _dialogService.ShowError(
                $"Unable to open diff view: {ex.Message}",
                AppMessages.DiffErrorTitle);
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
            StatusMessage = AppMessages.StartingComparison;

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
                StatusMessage = AppMessages.ComparisonCompleted(result.Data!.Items.Count, stopwatch.Elapsed.TotalSeconds);
                _logger.LogInformation("Directory comparison completed successfully in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
            }
            else
            {
                StatusMessage = AppMessages.ComparisonFailed(result.ErrorMessage);
                _logger.LogError("Directory comparison failed: {Error}", result.ErrorMessage);
                
                _dialogService.ShowError(
                    $"Comparison failed: {result.ErrorMessage}",
                    AppMessages.ComparisonErrorTitle);
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = AppMessages.ComparisonCancelled;
            _logger.LogInformation("Directory comparison was cancelled");
        }
        catch (Exception ex)
        {
            StatusMessage = AppMessages.UnexpectedError(ex.Message);
            _logger.LogError(ex, "Unexpected error during directory comparison");
            
            _dialogService.ShowError(
                $"An unexpected error occurred: {ex.Message}",
                AppMessages.ApplicationErrorTitle);
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
        StatusMessage = AppMessages.CancellingComparison;
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
                AppMessages.WarningTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return null;
        }
    }

    [RelayCommand]
    private void ShowAbout()
    {
        var aboutWindow = new AboutWindow
        {
            Owner = Application.Current.MainWindow
        };
        aboutWindow.ShowDialog();
    }
}
