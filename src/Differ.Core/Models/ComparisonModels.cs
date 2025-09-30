namespace Differ.Core.Models;

/// <summary>
/// Represents information about a file system item (file or directory)
/// </summary>
public class FileSystemItem
{
    /// <summary>
    /// Gets the full path to the item
    /// </summary>
    public required string FullPath { get; init; }
    
    /// <summary>
    /// Gets the name of the item (filename or directory name)
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Gets a value indicating whether this item is a directory
    /// </summary>
    public required bool IsDirectory { get; init; }
    
    /// <summary>
    /// Gets the size of the file in bytes (null for directories)
    /// </summary>
    public long? Size { get; init; }
    
    /// <summary>
    /// Gets the last modified date of the item
    /// </summary>
    public DateTime LastModified { get; init; }
    
    /// <summary>
    /// Gets the relative path from the root comparison directory
    /// </summary>
    public required string RelativePath { get; init; }
}

/// <summary>
/// Represents the status of a file system item in comparison
/// </summary>
public enum ComparisonStatus
{
    /// <summary>
    /// Item exists only in the left directory
    /// </summary>
    LeftOnly,
    
    /// <summary>
    /// Item exists only in the right directory
    /// </summary>
    RightOnly,
    
    /// <summary>
    /// Item exists in both directories and is identical
    /// </summary>
    Identical,
    
    /// <summary>
    /// Item exists in both directories but differs
    /// </summary>
    Different,
    
    /// <summary>
    /// Item comparison failed due to error
    /// </summary>
    Error
}

/// <summary>
/// Represents a comparison result for a single file system item
/// </summary>
public class ComparisonItem
{
    /// <summary>
    /// Gets the relative path of the item being compared
    /// </summary>
    public required string RelativePath { get; init; }
    
    /// <summary>
    /// Gets the item from the left directory (null if doesn't exist)
    /// </summary>
    public FileSystemItem? LeftItem { get; init; }
    
    /// <summary>
    /// Gets the item from the right directory (null if doesn't exist)
    /// </summary>
    public FileSystemItem? RightItem { get; init; }
    
    /// <summary>
    /// Gets the comparison status
    /// </summary>
    public ComparisonStatus Status { get; init; }
    
    /// <summary>
    /// Gets any error message from comparison (for Error status)
    /// </summary>
    public string? ErrorMessage { get; init; }
    
    /// <summary>
    /// Gets a value indicating whether this item is a directory
    /// </summary>
    public bool IsDirectory => LeftItem?.IsDirectory ?? RightItem?.IsDirectory ?? false;
}

/// <summary>
/// Represents the complete result of a directory comparison
/// </summary>
public class DirectoryComparisonResult
{
    /// <summary>
    /// Gets the path of the left directory
    /// </summary>
    public required string LeftPath { get; init; }
    
    /// <summary>
    /// Gets the path of the right directory
    /// </summary>
    public required string RightPath { get; init; }
    
    /// <summary>
    /// Gets the comparison items
    /// </summary>
    public required IReadOnlyList<ComparisonItem> Items { get; init; }
    
    /// <summary>
    /// Gets the timestamp when the comparison was performed
    /// </summary>
    public DateTime ComparisonTime { get; init; } = DateTime.Now;
    
    /// <summary>
    /// Gets summary statistics for the comparison
    /// </summary>
    public ComparisonSummary Summary => new()
    {
        TotalItems = Items.Count,
        IdenticalItems = Items.Count(i => i.Status == ComparisonStatus.Identical),
        DifferentItems = Items.Count(i => i.Status == ComparisonStatus.Different),
        LeftOnlyItems = Items.Count(i => i.Status == ComparisonStatus.LeftOnly),
        RightOnlyItems = Items.Count(i => i.Status == ComparisonStatus.RightOnly),
        ErrorItems = Items.Count(i => i.Status == ComparisonStatus.Error)
    };
}

/// <summary>
/// Provides summary statistics for a directory comparison
/// </summary>
public class ComparisonSummary
{
    /// <summary>
    /// Gets the total number of items compared
    /// </summary>
    public int TotalItems { get; init; }
    
    /// <summary>
    /// Gets the number of identical items
    /// </summary>
    public int IdenticalItems { get; init; }
    
    /// <summary>
    /// Gets the number of different items
    /// </summary>
    public int DifferentItems { get; init; }
    
    /// <summary>
    /// Gets the number of items that exist only in the left directory
    /// </summary>
    public int LeftOnlyItems { get; init; }
    
    /// <summary>
    /// Gets the number of items that exist only in the right directory
    /// </summary>
    public int RightOnlyItems { get; init; }
    
    /// <summary>
    /// Gets the number of items that had comparison errors
    /// </summary>
    public int ErrorItems { get; init; }
}