using Differ.Core.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

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
