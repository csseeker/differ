using System.Threading.Tasks;

namespace Differ.UI.Services;

/// <summary>
/// Service for displaying dialogs to the user in a consistent, testable manner.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows an error dialog with the specified message.
    /// </summary>
    /// <param name="message">The error message to display.</param>
    /// <param name="title">The dialog title. If null, uses default error title.</param>
    void ShowError(string message, string? title = null);

    /// <summary>
    /// Shows a warning dialog with the specified message.
    /// </summary>
    /// <param name="message">The warning message to display.</param>
    /// <param name="title">The dialog title. If null, uses default warning title.</param>
    void ShowWarning(string message, string? title = null);

    /// <summary>
    /// Shows an information dialog with the specified message.
    /// </summary>
    /// <param name="message">The information message to display.</param>
    /// <param name="title">The dialog title. If null, uses default information title.</param>
    void ShowInformation(string message, string? title = null);

    /// <summary>
    /// Shows a confirmation dialog with Yes/No buttons.
    /// </summary>
    /// <param name="message">The confirmation message to display.</param>
    /// <param name="title">The dialog title. If null, uses default confirmation title.</param>
    /// <returns>True if the user clicked Yes, false otherwise.</returns>
    bool ShowConfirmation(string message, string? title = null);

    /// <summary>
    /// Shows an error dialog asynchronously on the UI thread.
    /// </summary>
    /// <param name="message">The error message to display.</param>
    /// <param name="title">The dialog title. If null, uses default error title.</param>
    Task ShowErrorAsync(string message, string? title = null);

    /// <summary>
    /// Shows a warning dialog asynchronously on the UI thread.
    /// </summary>
    /// <param name="message">The warning message to display.</param>
    /// <param name="title">The dialog title. If null, uses default warning title.</param>
    Task ShowWarningAsync(string message, string? title = null);
}
