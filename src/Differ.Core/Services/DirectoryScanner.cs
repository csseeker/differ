using Differ.Core.Interfaces;
using Differ.Core.Models;
using Microsoft.Extensions.Logging;

namespace Differ.Core.Services;

/// <summary>
/// Default implementation of directory scanning service
/// </summary>
public class DirectoryScanner : IDirectoryScanner
{
    private readonly ILogger<DirectoryScanner> _logger;

    /// <summary>
    /// Initializes a new instance of the DirectoryScanner class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public DirectoryScanner(ILogger<DirectoryScanner> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<OperationResult<IReadOnlyList<FileSystemItem>>> ScanDirectoryAsync(
        string directoryPath,
        CancellationToken cancellationToken = default,
        IProgress<string>? progress = null)
    {
        try
        {
            _logger.LogInformation("Starting directory scan: {DirectoryPath}", directoryPath);
            
            var validationResult = ValidateDirectoryPath(directoryPath);
            if (!validationResult.IsSuccess)
            {
                return OperationResult<IReadOnlyList<FileSystemItem>>.Failure(
                    validationResult.ErrorMessage!, 
                    validationResult.Exception);
            }

            var items = new List<FileSystemItem>();
            var rootDirectory = new DirectoryInfo(directoryPath);
            
            progress?.Report($"Scanning directory: {directoryPath}");
            
            await ScanDirectoryRecursiveAsync(
                rootDirectory, 
                rootDirectory.FullName, 
                items, 
                cancellationToken, 
                progress).ConfigureAwait(false);

            _logger.LogInformation("Directory scan completed. Found {ItemCount} items", items.Count);
            
            return OperationResult<IReadOnlyList<FileSystemItem>>.Success(items.AsReadOnly());
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Directory scan was cancelled");
            return OperationResult<IReadOnlyList<FileSystemItem>>.Failure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during directory scan");
            return OperationResult<IReadOnlyList<FileSystemItem>>.Failure(
                $"Failed to scan directory: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public OperationResult ValidateDirectoryPath(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            return OperationResult.Failure("Directory path cannot be empty");
        }

        try
        {
            // Validate path format
            var fullPath = Path.GetFullPath(directoryPath);
            
            // Check if directory exists
            if (!Directory.Exists(fullPath))
            {
                return OperationResult.Failure($"Directory does not exist: {fullPath}");
            }

            // Check if we can access the directory
            _ = Directory.GetDirectories(fullPath);
            
            return OperationResult.Success();
        }
        catch (ArgumentException ex)
        {
            return OperationResult.Failure($"Invalid directory path: {ex.Message}", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            return OperationResult.Failure($"Access denied to directory: {directoryPath}", ex);
        }
        catch (DirectoryNotFoundException ex)
        {
            return OperationResult.Failure($"Directory not found: {directoryPath}", ex);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Error validating directory path: {ex.Message}", ex);
        }
    }

    private async Task ScanDirectoryRecursiveAsync(
        DirectoryInfo directory,
        string rootPath,
        List<FileSystemItem> items,
        CancellationToken cancellationToken,
        IProgress<string>? progress)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        try
        {
            // Add current directory
            var relativePath = Path.GetRelativePath(rootPath, directory.FullName);
            if (relativePath != ".")
            {
                items.Add(new FileSystemItem
                {
                    FullPath = directory.FullName,
                    Name = directory.Name,
                    IsDirectory = true,
                    Size = null,
                    LastModified = directory.LastWriteTime,
                    RelativePath = relativePath
                });
            }

            // Process files in current directory
            foreach (var file in directory.GetFiles())
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var fileRelativePath = Path.GetRelativePath(rootPath, file.FullName);
                progress?.Report($"Processing: {fileRelativePath}");
                
                items.Add(new FileSystemItem
                {
                    FullPath = file.FullName,
                    Name = file.Name,
                    IsDirectory = false,
                    Size = file.Length,
                    LastModified = file.LastWriteTime,
                    RelativePath = fileRelativePath
                });
                
                // Yield control periodically for responsiveness
                if (items.Count % 100 == 0)
                {
                    await Task.Yield();
                }
            }

            // Process subdirectories
            foreach (var subDirectory in directory.GetDirectories())
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                await ScanDirectoryRecursiveAsync(
                    subDirectory, 
                    rootPath, 
                    items, 
                    cancellationToken, 
                    progress).ConfigureAwait(false);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Access denied to directory: {DirectoryPath}. Error: {Error}", 
                directory.FullName, ex.Message);
            // Continue processing other directories
        }
        catch (DirectoryNotFoundException ex)
        {
            _logger.LogWarning("Directory not found: {DirectoryPath}. Error: {Error}", 
                directory.FullName, ex.Message);
            // Continue processing other directories
        }
    }
}