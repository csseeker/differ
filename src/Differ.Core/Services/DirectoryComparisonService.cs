using Differ.Core.Interfaces;
using Differ.Core.Models;
using Microsoft.Extensions.Logging;

namespace Differ.Core.Services;

/// <summary>
/// Main service for comparing directories
/// </summary>
public class DirectoryComparisonService : IDirectoryComparisonService
{
    private readonly IDirectoryScanner _directoryScanner;
    private readonly IFileComparer _fileComparer;
    private readonly ILogger<DirectoryComparisonService> _logger;

    /// <summary>
    /// Initializes a new instance of the DirectoryComparisonService class
    /// </summary>
    /// <param name="directoryScanner">The directory scanner service</param>
    /// <param name="fileComparer">The file comparer service</param>
    /// <param name="logger">The logger instance</param>
    public DirectoryComparisonService(
        IDirectoryScanner directoryScanner,
        IFileComparer fileComparer,
        ILogger<DirectoryComparisonService> logger)
    {
        _directoryScanner = directoryScanner ?? throw new ArgumentNullException(nameof(directoryScanner));
        _fileComparer = fileComparer ?? throw new ArgumentNullException(nameof(fileComparer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<OperationResult<DirectoryComparisonResult>> CompareDirectoriesAsync(
        string leftDirectoryPath,
        string rightDirectoryPath,
        CancellationToken cancellationToken = default,
        IProgress<string>? progress = null)
    {
        try
        {
            _logger.LogInformation("Starting directory comparison: {LeftPath} vs {RightPath}", 
                leftDirectoryPath, rightDirectoryPath);

            progress?.Report("Validating directories...");

            // Validate both directories
            var leftValidation = _directoryScanner.ValidateDirectoryPath(leftDirectoryPath);
            if (!leftValidation.IsSuccess)
            {
                return OperationResult<DirectoryComparisonResult>.Failure(
                    $"Left directory validation failed: {leftValidation.ErrorMessage}");
            }

            var rightValidation = _directoryScanner.ValidateDirectoryPath(rightDirectoryPath);
            if (!rightValidation.IsSuccess)
            {
                return OperationResult<DirectoryComparisonResult>.Failure(
                    $"Right directory validation failed: {rightValidation.ErrorMessage}");
            }

            // Scan both directories
            progress?.Report("Scanning left directory...");
            var leftScanResult = await _directoryScanner.ScanDirectoryAsync(
                leftDirectoryPath, cancellationToken, progress).ConfigureAwait(false);

            if (!leftScanResult.IsSuccess)
            {
                return OperationResult<DirectoryComparisonResult>.Failure(
                    $"Failed to scan left directory: {leftScanResult.ErrorMessage}",
                    leftScanResult.Exception);
            }

            progress?.Report("Scanning right directory...");
            var rightScanResult = await _directoryScanner.ScanDirectoryAsync(
                rightDirectoryPath, cancellationToken, progress).ConfigureAwait(false);

            if (!rightScanResult.IsSuccess)
            {
                return OperationResult<DirectoryComparisonResult>.Failure(
                    $"Failed to scan right directory: {rightScanResult.ErrorMessage}",
                    rightScanResult.Exception);
            }

            // Perform comparison
            progress?.Report("Comparing items...");
            var comparisonItems = await CompareItemsAsync(
                leftScanResult.Data!,
                rightScanResult.Data!,
                cancellationToken,
                progress).ConfigureAwait(false);

            var result = new DirectoryComparisonResult
            {
                LeftPath = leftDirectoryPath,
                RightPath = rightDirectoryPath,
                Items = comparisonItems
            };

            _logger.LogInformation("Directory comparison completed. Found {ItemCount} items", 
                comparisonItems.Count);

            return OperationResult<DirectoryComparisonResult>.Success(result);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Directory comparison was cancelled");
            return OperationResult<DirectoryComparisonResult>.Failure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during directory comparison");
            return OperationResult<DirectoryComparisonResult>.Failure(
                $"Directory comparison failed: {ex.Message}", ex);
        }
    }

    private async Task<List<ComparisonItem>> CompareItemsAsync(
        IReadOnlyList<FileSystemItem> leftItems,
        IReadOnlyList<FileSystemItem> rightItems,
        CancellationToken cancellationToken,
        IProgress<string>? progress)
    {
        var leftItemsByPath = leftItems.ToDictionary(item => item.RelativePath, StringComparer.OrdinalIgnoreCase);
        var rightItemsByPath = rightItems.ToDictionary(item => item.RelativePath, StringComparer.OrdinalIgnoreCase);
        var allPaths = leftItemsByPath.Keys.Union(rightItemsByPath.Keys, StringComparer.OrdinalIgnoreCase).ToList();

        var comparisonItems = new List<ComparisonItem>();
        var totalItems = allPaths.Count;
        var processedItems = 0;

        foreach (var relativePath in allPaths.OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();

            processedItems++;
            progress?.Report($"Comparing item {processedItems}/{totalItems}: {relativePath}");

            var leftItem = leftItemsByPath.TryGetValue(relativePath, out var left) ? left : null;
            var rightItem = rightItemsByPath.TryGetValue(relativePath, out var right) ? right : null;

            var comparisonItem = await CreateComparisonItemAsync(
                relativePath, leftItem, rightItem, cancellationToken).ConfigureAwait(false);

            comparisonItems.Add(comparisonItem);

            // Yield control periodically for responsiveness
            if (processedItems % 10 == 0)
            {
                await Task.Yield();
            }
        }

        return comparisonItems;
    }

    private async Task<ComparisonItem> CreateComparisonItemAsync(
        string relativePath,
        FileSystemItem? leftItem,
        FileSystemItem? rightItem,
        CancellationToken cancellationToken)
    {
        try
        {
            // Determine status based on existence
            if (leftItem == null && rightItem != null)
            {
                return new ComparisonItem
                {
                    RelativePath = relativePath,
                    LeftItem = null,
                    RightItem = rightItem,
                    Status = ComparisonStatus.RightOnly
                };
            }

            if (leftItem != null && rightItem == null)
            {
                return new ComparisonItem
                {
                    RelativePath = relativePath,
                    LeftItem = leftItem,
                    RightItem = null,
                    Status = ComparisonStatus.LeftOnly
                };
            }

            // Both items exist
            if (leftItem != null && rightItem != null)
            {
                // Check if both are directories or both are files
                if (leftItem.IsDirectory != rightItem.IsDirectory)
                {
                    return new ComparisonItem
                    {
                        RelativePath = relativePath,
                        LeftItem = leftItem,
                        RightItem = rightItem,
                        Status = ComparisonStatus.Different
                    };
                }

                // If both are directories, they're considered identical by structure
                if (leftItem.IsDirectory)
                {
                    return new ComparisonItem
                    {
                        RelativePath = relativePath,
                        LeftItem = leftItem,
                        RightItem = rightItem,
                        Status = ComparisonStatus.Identical
                    };
                }

                // Both are files - compare them
                if (_fileComparer.CanCompare(leftItem.FullPath, rightItem.FullPath))
                {
                    var comparisonResult = await _fileComparer.CompareFilesAsync(
                        leftItem.FullPath, rightItem.FullPath, cancellationToken).ConfigureAwait(false);

                    if (!comparisonResult.IsSuccess)
                    {
                        return new ComparisonItem
                        {
                            RelativePath = relativePath,
                            LeftItem = leftItem,
                            RightItem = rightItem,
                            Status = ComparisonStatus.Error,
                            ErrorMessage = comparisonResult.ErrorMessage
                        };
                    }

                    return new ComparisonItem
                    {
                        RelativePath = relativePath,
                        LeftItem = leftItem,
                        RightItem = rightItem,
                        Status = comparisonResult.Data! ? ComparisonStatus.Identical : ComparisonStatus.Different
                    };
                }
                else
                {
                    return new ComparisonItem
                    {
                        RelativePath = relativePath,
                        LeftItem = leftItem,
                        RightItem = rightItem,
                        Status = ComparisonStatus.Error,
                        ErrorMessage = "File comparison not supported for this file type"
                    };
                }
            }

            // This should never happen, but handle it gracefully
            return new ComparisonItem
            {
                RelativePath = relativePath,
                LeftItem = leftItem,
                RightItem = rightItem,
                Status = ComparisonStatus.Error,
                ErrorMessage = "Unexpected comparison state"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing item: {RelativePath}", relativePath);
            return new ComparisonItem
            {
                RelativePath = relativePath,
                LeftItem = leftItem,
                RightItem = rightItem,
                Status = ComparisonStatus.Error,
                ErrorMessage = ex.Message
            };
        }
    }
}
