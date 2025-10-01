namespace Differ.UI.Resources;

/// <summary>
/// Centralized application messages for consistent branding and user communication
/// </summary>
public static class AppMessages
{
    public const string AppName = "Differ";
    public const string AppFullName = "Differ - Directory Comparison Tool";

    #region Status Messages

    public const string Ready = "Ready to compare directories";
    public const string StartingComparison = "Starting comparison...";
    public const string ComparingDirectories = "Comparing directories - please wait...";
    public const string ComparisonCancelled = "Comparison cancelled by user";
    public const string CancellingComparison = "Cancelling operation...";

    public static string ComparisonCompleted(int itemCount, double seconds) =>
        $"Comparison complete - Found {itemCount:N0} items in {seconds:F1}s";

    public static string OpeningDiff(string filename) =>
        $"Opening diff for '{filename}'...";

    public static string DiffOpened(string filename) =>
        $"Diff view opened for '{filename}'";

    public const string DiffCancelled = "Diff opening cancelled";
    public const string DiffFailed = "Failed to open diff view";
    public const string DiffUnavailableForDirectories = "Diff view is available for files only";
    public const string DiffFilesNotFound = "Unable to locate files for diff view";

    public const string ComputingDiff = "Computing differences...";
    public const string DiffComplete = "Diff computation complete";
    public const string DiffComputationCancelled = "Diff computation cancelled";

    public static string ComparisonFailed(string? errorMessage) =>
        $"Comparison failed: {errorMessage ?? "Unknown error"}";

    public static string UnexpectedError(string message) =>
        $"An unexpected error occurred: {message}";

    #endregion

    #region Dialog Titles

    public const string ApplicationErrorTitle = "Differ - Application Error";
    public const string ComparisonErrorTitle = "Differ - Comparison Error";
    public const string DiffErrorTitle = "Differ - Diff Error";
    public const string ValidationErrorTitle = "Differ - Validation Error";
    public const string WarningTitle = "Differ - Warning";
    public const string InformationTitle = "Differ - Information";

    #endregion

    #region Error Messages

    public static string ApplicationStartupFailed(string message) =>
        $"Failed to start application: {message}";

    public static string DiffComputationFailed(string? errorMessage) =>
        errorMessage ?? "Unable to compute diff";

    public static string UnexpectedDiffError(string message) =>
        $"An unexpected error occurred while computing diff: {message}";

    #endregion

    #region Window Titles

    public const string MainWindowTitle = "Differ - Directory Comparison Tool";

    public static string FileDiffWindowTitle(string filename) =>
        $"Differ - Comparing: {filename}";

    #endregion
}
