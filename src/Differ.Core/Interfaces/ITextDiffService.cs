using Differ.Core.Models;

namespace Differ.Core.Interfaces;

/// <summary>
/// Defines the contract for producing human-readable differences between two text files.
/// </summary>
public interface ITextDiffService
{
    /// <summary>
    /// Determines whether the service can compute a diff for the supplied paths.
    /// </summary>
    /// <param name="leftFilePath">The path to the left (original) file.</param>
    /// <param name="rightFilePath">The path to the right (modified) file.</param>
    /// <returns><c>true</c> if the service can diff the files; otherwise <c>false</c>.</returns>
    bool CanDiff(string leftFilePath, string rightFilePath);

    /// <summary>
    /// Produces a textual diff for the supplied request.
    /// </summary>
    /// <param name="request">The diff request.</param>
    /// <param name="cancellationToken">Token used to cancel the diff operation.</param>
    /// <param name="progress">Optional progress reporter that will be invoked with values in the range [0, 1].</param>
    /// <returns>An <see cref="OperationResult{T}"/> containing the diff outcome.</returns>
    Task<OperationResult<TextDiffResult>> ComputeDiffAsync(
        TextDiffRequest request,
        CancellationToken cancellationToken = default,
        IProgress<double>? progress = null);
}
