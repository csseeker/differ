using Differ.Core.Models;

namespace Differ.Core.Interfaces;

/// <summary>
/// Defines the contract for scanning directories and retrieving file system items
/// </summary>
public interface IDirectoryScanner
{
    /// <summary>
    /// Scans a directory asynchronously and returns all file system items
    /// </summary>
    /// <param name="directoryPath">The path to the directory to scan</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <param name="progress">Progress reporter for the scanning operation</param>
    /// <returns>An operation result containing the list of file system items</returns>
    Task<OperationResult<IReadOnlyList<FileSystemItem>>> ScanDirectoryAsync(
        string directoryPath,
        CancellationToken cancellationToken = default,
        IProgress<string>? progress = null);
    
    /// <summary>
    /// Validates that a directory path exists and is accessible
    /// </summary>
    /// <param name="directoryPath">The path to validate</param>
    /// <returns>An operation result indicating success or failure</returns>
    OperationResult ValidateDirectoryPath(string directoryPath);
}