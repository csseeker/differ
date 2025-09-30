using Differ.Core.Models;

namespace Differ.Core.Interfaces;

/// <summary>
/// Defines the contract for the main directory comparison service
/// </summary>
public interface IDirectoryComparisonService
{
    /// <summary>
    /// Compares two directories asynchronously
    /// </summary>
    /// <param name="leftDirectoryPath">Path to the first directory</param>
    /// <param name="rightDirectoryPath">Path to the second directory</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <param name="progress">Progress reporter for the comparison operation</param>
    /// <returns>An operation result containing the comparison results</returns>
    Task<OperationResult<DirectoryComparisonResult>> CompareDirectoriesAsync(
        string leftDirectoryPath,
        string rightDirectoryPath,
        CancellationToken cancellationToken = default,
        IProgress<string>? progress = null);
}