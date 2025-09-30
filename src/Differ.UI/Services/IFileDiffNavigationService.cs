using System.Threading;
using System.Threading.Tasks;

namespace Differ.UI.Services;

/// <summary>
/// Provides a UI abstraction for launching the file diff window.
/// </summary>
public interface IFileDiffNavigationService
{
    /// <summary>
    /// Displays the diff window for the supplied files.
    /// </summary>
    /// <param name="leftFilePath">The path to the left (original) file.</param>
    /// <param name="rightFilePath">The path to the right (modified) file.</param>
    /// <param name="displayName">Optional display name for the window title.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    Task ShowDiffAsync(string leftFilePath, string rightFilePath, string? displayName = null, CancellationToken cancellationToken = default);
}
