using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Differ.Core.Interfaces;
using Differ.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Differ.UI.Models;
using Microsoft.Extensions.Logging;

namespace Differ.UI.ViewModels;

/// <summary>
/// View model responsible for presenting side-by-side text differences.
/// </summary>
public partial class FileDiffViewModel : ObservableObject, IDisposable
{
    private readonly ITextDiffService _textDiffService;
    private readonly ILogger<FileDiffViewModel> _logger;
    private readonly Dispatcher _dispatcher;

    private CancellationTokenSource? _loadCancellationSource;
    private string _leftFilePath = string.Empty;
    private string _rightFilePath = string.Empty;
    private string? _displayName;
    private bool _isInitialised;

    public ObservableCollection<DiffDisplayLine> DiffLines { get; } = new();

    [ObservableProperty]
    private string _title = "File Diff";

    [ObservableProperty]
    private string _leftFileName = string.Empty;

    [ObservableProperty]
    private string _rightFileName = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private DiffSummary? _summary;

    [ObservableProperty]
    private string? _summaryText;

    [ObservableProperty]
    private bool _ignoreWhitespace;

    [ObservableProperty]
    private bool _ignoreCase;

    [ObservableProperty]
    private double _progressValue;

    [ObservableProperty]
    private bool _showWhitespaceCharacters;

    public FileDiffViewModel(ITextDiffService textDiffService, ILogger<FileDiffViewModel> logger)
    {
        _textDiffService = textDiffService ?? throw new ArgumentNullException(nameof(textDiffService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
    }

    /// <summary>
    /// Configures the view model with the file paths that should be diffed.
    /// </summary>
    public void Initialize(string leftFilePath, string rightFilePath, string? displayName)
    {
        _leftFilePath = leftFilePath ?? throw new ArgumentNullException(nameof(leftFilePath));
        _rightFilePath = rightFilePath ?? throw new ArgumentNullException(nameof(rightFilePath));
        _displayName = displayName;

        LeftFileName = System.IO.Path.GetFileName(_leftFilePath);
        RightFileName = System.IO.Path.GetFileName(_rightFilePath);
        OnPropertyChanged(nameof(LeftFilePath));
        OnPropertyChanged(nameof(RightFilePath));

        Title = string.IsNullOrWhiteSpace(displayName)
            ? $"{LeftFileName} ↔ {RightFileName}"
            : displayName!;

        Summary = null;
        SummaryText = null;
        DiffLines.Clear();
        StatusMessage = "Ready";

        _isInitialised = true;
    }

    public string LeftFilePath => _leftFilePath;

    public string RightFilePath => _rightFilePath;

    /// <summary>
    /// Initiates loading of the diff information.
    /// </summary>
    public Task LoadDiffAsync(CancellationToken cancellationToken = default)
    {
        if (!_isInitialised)
        {
            throw new InvalidOperationException("The view model must be initialised before loading.");
        }

        return LoadDiffInternalAsync(cancellationToken);
    }

    [RelayCommand]
    private Task ReloadAsync()
    {
        if (!_isInitialised)
        {
            return Task.CompletedTask;
        }

        return LoadDiffInternalAsync();
    }

    partial void OnIgnoreWhitespaceChanged(bool value)
    {
        _ = ReloadAsync();
    }

    partial void OnIgnoreCaseChanged(bool value)
    {
        _ = ReloadAsync();
    }

    private async Task LoadDiffInternalAsync(CancellationToken cancellationToken = default)
    {
        _loadCancellationSource?.Cancel();
        _loadCancellationSource?.Dispose();
        _loadCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var linkedToken = _loadCancellationSource.Token;

        try
        {
            IsLoading = true;
            ProgressValue = 0;
            StatusMessage = "Computing diff...";

            var request = new TextDiffRequest
            {
                LeftFilePath = _leftFilePath,
                RightFilePath = _rightFilePath,
                IgnoreWhitespace = IgnoreWhitespace,
                IgnoreCase = IgnoreCase,
                ContextLineCount = 0
            };

            var progress = new Progress<double>(value => ProgressValue = value);
            var result = await _textDiffService.ComputeDiffAsync(request, linkedToken, progress);

            if (linkedToken.IsCancellationRequested)
            {
                StatusMessage = "Diff cancelled.";
                return;
            }

            if (!result.IsSuccess || result.Data is null)
            {
                await ClearLinesAsync();
                Summary = null;
                SummaryText = null;
                var message = result.ErrorMessage ?? "Unable to compute diff.";
                StatusMessage = message;

                await _dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show(
                        message,
                        "Diff Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                });
                return;
            }

            await PopulateLinesAsync(result.Data);
            Summary = result.Data.Summary;
            SummaryText = BuildSummaryText(result.Data.Summary);
            StatusMessage = result.Data.HasDifferences
                ? $"Differences detected ({result.Data.Summary.ModifiedLines} modified, {result.Data.Summary.AddedLines} added, {result.Data.Summary.RemovedLines} removed)."
                : "Files are identical.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Diff cancelled.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while loading diff window.");
            await _dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(
                    $"An unexpected error occurred: {ex.Message}",
                    "Diff Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            });
            StatusMessage = "Failed to load diff.";
        }
        finally
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                ProgressValue = 0;
                IsLoading = false;
            }
        }
    }

    private Task PopulateLinesAsync(TextDiffResult diffResult)
    {
        return _dispatcher.InvokeAsync(() =>
        {
            DiffLines.Clear();
            foreach (var line in diffResult.Lines)
            {
                DiffLines.Add(new DiffDisplayLine(
                    line.ChangeKind,
                    line.LeftLineNumber,
                    line.LeftText,
                    line.RightLineNumber,
                    line.RightText));
            }
        }).Task;
    }

    private Task ClearLinesAsync()
    {
        return _dispatcher.InvokeAsync(() => DiffLines.Clear()).Task;
    }

    private static string? BuildSummaryText(DiffSummary? summary)
    {
        if (summary is null)
        {
            return null;
        }

        return $"Total: {summary.TotalLines}, Unchanged: {summary.UnchangedLines}, Modified: {summary.ModifiedLines}, Added: {summary.AddedLines}, Removed: {summary.RemovedLines}";
    }

    public void Dispose()
    {
        _loadCancellationSource?.Cancel();
        _loadCancellationSource?.Dispose();
    }
}
