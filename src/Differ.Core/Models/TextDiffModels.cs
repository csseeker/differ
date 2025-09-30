namespace Differ.Core.Models;

/// <summary>
/// Represents a request to compute the differences between two text files.
/// </summary>
public class TextDiffRequest
{
    /// <summary>
    /// Gets or sets the full path to the file that is considered the left (or original) side.
    /// </summary>
    public required string LeftFilePath { get; init; }

    /// <summary>
    /// Gets or sets the full path to the file that is considered the right (or modified) side.
    /// </summary>
    public required string RightFilePath { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether whitespace differences should be ignored.
    /// </summary>
    public bool IgnoreWhitespace { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether character casing differences should be ignored.
    /// </summary>
    public bool IgnoreCase { get; init; }

    /// <summary>
    /// Gets or sets the number of unchanged context lines to include around differences.
    /// </summary>
    /// <remarks>
    /// The current implementation includes all lines, but this value enables future optimisation.
    /// </remarks>
    public int ContextLineCount { get; init; }
}

/// <summary>
/// Represents a single line within a diff result.
/// </summary>
public class DiffLine
{
    /// <summary>
    /// Gets the type of change associated with this line.
    /// </summary>
    public required LineChangeKind ChangeKind { get; init; }

    /// <summary>
    /// Gets the (1-based) line number in the left file, if applicable.
    /// </summary>
    public int? LeftLineNumber { get; init; }

    /// <summary>
    /// Gets the text for the left file line, if available.
    /// </summary>
    public string? LeftText { get; init; }

    /// <summary>
    /// Gets the (1-based) line number in the right file, if applicable.
    /// </summary>
    public int? RightLineNumber { get; init; }

    /// <summary>
    /// Gets the text for the right file line, if available.
    /// </summary>
    public string? RightText { get; init; }
}

/// <summary>
/// Provides summary statistics for a text diff operation.
/// </summary>
public class DiffSummary
{
    /// <summary>
    /// Gets the total number of lines in the diff output.
    /// </summary>
    public int TotalLines { get; init; }

    /// <summary>
    /// Gets the number of unchanged (context) lines.
    /// </summary>
    public int UnchangedLines { get; init; }

    /// <summary>
    /// Gets the number of added lines.
    /// </summary>
    public int AddedLines { get; init; }

    /// <summary>
    /// Gets the number of removed lines.
    /// </summary>
    public int RemovedLines { get; init; }

    /// <summary>
    /// Gets the number of modified lines.
    /// </summary>
    public int ModifiedLines { get; init; }
}

/// <summary>
/// Represents the result of a text diff operation.
/// </summary>
public class TextDiffResult
{
    /// <summary>
    /// Gets the path of the left file involved in the diff.
    /// </summary>
    public required string LeftFilePath { get; init; }

    /// <summary>
    /// Gets the path of the right file involved in the diff.
    /// </summary>
    public required string RightFilePath { get; init; }

    /// <summary>
    /// Gets the collection of diff lines representing the comparison outcome.
    /// </summary>
    public required IReadOnlyList<DiffLine> Lines { get; init; }

    /// <summary>
    /// Gets the summary statistics for this diff.
    /// </summary>
    public required DiffSummary Summary { get; init; }

    /// <summary>
    /// Gets a value indicating whether the diff detected any differences.
    /// </summary>
    public bool HasDifferences => Summary.AddedLines + Summary.RemovedLines + Summary.ModifiedLines > 0;
}

/// <summary>
/// Describes the type of change detected for a diff line.
/// </summary>
public enum LineChangeKind
{
    /// <summary>
    /// The line is unchanged between the files.
    /// </summary>
    Unchanged,

    /// <summary>
    /// The line exists only in the right file (added).
    /// </summary>
    Added,

    /// <summary>
    /// The line exists only in the left file (removed).
    /// </summary>
    Removed,

    /// <summary>
    /// The line exists in both files but has been modified.
    /// </summary>
    Modified
}
