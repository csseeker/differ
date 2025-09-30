using Differ.Core.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Text;

namespace Differ.UI.Converters;

/// <summary>
/// Converts ComparisonStatus to appropriate colors for UI display
/// </summary>
public class ComparisonStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ComparisonStatus status)
        {
            return status switch
            {
                ComparisonStatus.Identical => System.Windows.Media.Brushes.LightGreen,
                ComparisonStatus.Different => System.Windows.Media.Brushes.LightCoral,
                ComparisonStatus.LeftOnly => System.Windows.Media.Brushes.LightBlue,
                ComparisonStatus.RightOnly => System.Windows.Media.Brushes.LightYellow,
                ComparisonStatus.Error => System.Windows.Media.Brushes.Pink,
                _ => System.Windows.Media.Brushes.Transparent
            };
        }
        return System.Windows.Media.Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts ComparisonStatus to human-readable text
/// </summary>
public class ComparisonStatusToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ComparisonStatus status)
        {
            return status switch
            {
                ComparisonStatus.Identical => "Identical",
                ComparisonStatus.Different => "Different",
                ComparisonStatus.LeftOnly => "Left Only",
                ComparisonStatus.RightOnly => "Right Only",
                ComparisonStatus.Error => "Error",
                _ => "Unknown"
            };
        }
        return "Unknown";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts file size in bytes to human-readable format
/// </summary>
public class FileSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is long size)
        {
            return FormatFileSize(size);
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static string FormatFileSize(long size)
    {
        string[] units = { "B", "KB", "MB", "GB", "TB" };
        double sizeInBytes = size;
        int unitIndex = 0;

        while (sizeInBytes >= 1024 && unitIndex < units.Length - 1)
        {
            sizeInBytes /= 1024;
            unitIndex++;
        }

        return $"{sizeInBytes:F1} {units[unitIndex]}";
    }
}

/// <summary>
/// Static converter instances for use in XAML
/// </summary>
public static class Converters
{
    public static readonly BooleanToVisibilityConverter BooleanToVisibilityConverter = new();
    public static readonly NullToVisibilityConverter NullToVisibilityConverter = new();
    public static readonly ItemTypeConverter ItemTypeConverter = new();
}

/// <summary>
/// Converts null values to Visibility
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts raw text into a representation that highlights whitespace and control characters.
/// </summary>
public class WhitespaceDisplayConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2)
        {
            return string.Empty;
        }

        var text = values[0] as string ?? string.Empty;
        var showWhitespace = values[1] is bool flag && flag;

        if (!showWhitespace)
        {
            return text;
        }

        if (text.Length == 0)
        {
            return "⏎";
        }

        var builder = new StringBuilder(text.Length + 4);
        foreach (var ch in text)
        {
            builder.Append(ConvertCharacter(ch));
        }

        builder.Append('⏎');
        return builder.ToString();
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static string ConvertCharacter(char ch)
    {
        return ch switch
        {
            ' ' => "·",
            '\t' => "→",
            '\r' => "␍",
            '\n' => "␊",
            (char)127 => "␡",
            < ' ' => new string((char)('\u2400' + ch), 1),
            _ => ch.ToString()
        };
    }
}

/// <summary>
/// Determines item type (File/Directory) from FileSystemItem
/// </summary>
public class ItemTypeConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var leftItem = values[0] as FileSystemItem;
        var rightItem = values[1] as FileSystemItem;
        
        var item = leftItem ?? rightItem;
        return item?.IsDirectory == true ? "Dir" : "File";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public record FilterButtonViewModel(string Label, int Count, Brush Color, string? Category);

public class SummaryToFilterButtonsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ComparisonSummary summary)
        {
            return Enumerable.Empty<FilterButtonViewModel>();
        }

        var buttons = new List<FilterButtonViewModel>
        {
            new("All", summary.TotalItems, Brushes.Black, null),
            new("Identical", summary.IdenticalItems, Brushes.Green, ComparisonStatus.Identical.ToString()),
            new("Different", summary.DifferentItems, Brushes.Red, ComparisonStatus.Different.ToString()),
            new("Left Only", summary.LeftOnlyItems, Brushes.Blue, ComparisonStatus.LeftOnly.ToString()),
            new("Right Only", summary.RightOnlyItems, Brushes.Orange, ComparisonStatus.RightOnly.ToString()),
            new("Errors", summary.ErrorItems, Brushes.Purple, ComparisonStatus.Error.ToString())
        };

        return buttons.Where(b => b.Count > 0 || b.Category == null);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts <see cref="LineChangeKind"/> values to background brushes for the diff view.
/// </summary>
public class LineChangeKindToBrushConverter : IValueConverter
{
    private static readonly Brush AddedBrush = CreateBrush(Color.FromRgb(220, 252, 231));
    private static readonly Brush RemovedBrush = CreateBrush(Color.FromRgb(254, 226, 226));
    private static readonly Brush ModifiedBrush = CreateBrush(Color.FromRgb(254, 243, 199));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is LineChangeKind changeKind)
        {
            return changeKind switch
            {
                LineChangeKind.Added => AddedBrush,
                LineChangeKind.Removed => RemovedBrush,
                LineChangeKind.Modified => ModifiedBrush,
                _ => Brushes.Transparent
            };
        }

        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static SolidColorBrush CreateBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}

/// <summary>
/// Determines whether a comparison item can be opened in the file diff view.
/// </summary>
public class ComparisonItemCanDiffConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ComparisonItem item)
        {
            var leftIsFile = item.LeftItem?.IsDirectory == false;
            var rightIsFile = item.RightItem?.IsDirectory == false;
            return item.Status == ComparisonStatus.Different && leftIsFile && rightIsFile;
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
