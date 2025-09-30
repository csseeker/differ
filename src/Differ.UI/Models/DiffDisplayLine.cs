using Differ.Core.Models;

namespace Differ.UI.Models;

/// <summary>
/// Represents a single row of diff output prepared for UI binding.
/// </summary>
public sealed class DiffDisplayLine
{
    public DiffDisplayLine(
        LineChangeKind changeKind,
        int? leftLineNumber,
        string? leftText,
        int? rightLineNumber,
        string? rightText)
    {
        ChangeKind = changeKind;
        LeftLineNumber = leftLineNumber;
        LeftText = leftText;
        RightLineNumber = rightLineNumber;
        RightText = rightText;
    }

    public LineChangeKind ChangeKind { get; }

    public int? LeftLineNumber { get; }

    public string? LeftText { get; }

    public int? RightLineNumber { get; }

    public string? RightText { get; }

    public bool HasDifference => ChangeKind != LineChangeKind.Unchanged;

    public string LeftLineNumberDisplay => LeftLineNumber?.ToString() ?? string.Empty;

    public string RightLineNumberDisplay => RightLineNumber?.ToString() ?? string.Empty;

    public string LeftTextDisplay => LeftText ?? string.Empty;

    public string RightTextDisplay => RightText ?? string.Empty;
}
