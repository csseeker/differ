using Differ.Core.Models;

namespace Differ.Core.Interfaces;

/// <summary>
/// Defines the contract for comparing files using different algorithms
/// </summary>
public interface IFileComparer
{
    /// <summary>
    /// Gets the name of this file comparison algorithm
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Gets a description of what this comparer does
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Compares two files asynchronously
    /// </summary>
    /// <param name="leftFilePath">Path to the first file</param>
    /// <param name="rightFilePath">Path to the second file</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>True if files are identical, false if different</returns>
    Task<OperationResult<bool>> CompareFilesAsync(
        string leftFilePath,
        string rightFilePath,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Determines if this comparer can handle the given file types
    /// </summary>
    /// <param name="leftFilePath">Path to the first file</param>
    /// <param name="rightFilePath">Path to the second file</param>
    /// <returns>True if this comparer can handle these files</returns>
    bool CanCompare(string leftFilePath, string rightFilePath);
}