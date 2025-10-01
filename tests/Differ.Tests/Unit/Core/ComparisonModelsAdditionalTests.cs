using Differ.Core.Models;
using FluentAssertions;

namespace Differ.Tests.Unit.Core;

public class ComparisonModelsAdditionalTests
{
    [Fact]
    public void ComparisonItem_IsDirectory_ShouldReturnFalseWhenBothItemsAreNull()
    {
        var item = new ComparisonItem
        {
            RelativePath = "test",
            LeftItem = null,
            RightItem = null,
            Status = ComparisonStatus.Error
        };

        item.IsDirectory.Should().BeFalse();
    }

    [Fact]
    public void ComparisonItem_IsDirectory_ShouldUseLeftItemWhenAvailable()
    {
        var item = new ComparisonItem
        {
            RelativePath = "folder",
            LeftItem = new FileSystemItem 
            { 
                FullPath = "left/folder", 
                Name = "folder", 
                IsDirectory = true,
                RelativePath = "folder"
            },
            RightItem = null,
            Status = ComparisonStatus.LeftOnly
        };

        item.IsDirectory.Should().BeTrue();
    }

    [Fact]
    public void ComparisonItem_IsDirectory_ShouldUseRightItemWhenLeftIsNull()
    {
        var item = new ComparisonItem
        {
            RelativePath = "folder",
            LeftItem = null,
            RightItem = new FileSystemItem 
            { 
                FullPath = "right/folder", 
                Name = "folder", 
                IsDirectory = true,
                RelativePath = "folder"
            },
            Status = ComparisonStatus.RightOnly
        };

        item.IsDirectory.Should().BeTrue();
    }

    [Fact]
    public void DirectoryComparisonResult_Summary_ShouldHandleEmptyItems()
    {
        var result = new DirectoryComparisonResult
        {
            LeftPath = @"C:\left",
            RightPath = @"C:\right",
            Items = new List<ComparisonItem>()
        };

        var summary = result.Summary;

        summary.TotalItems.Should().Be(0);
        summary.IdenticalItems.Should().Be(0);
        summary.DifferentItems.Should().Be(0);
        summary.LeftOnlyItems.Should().Be(0);
        summary.RightOnlyItems.Should().Be(0);
        summary.ErrorItems.Should().Be(0);
    }

    [Fact]
    public void DirectoryComparisonResult_Summary_ShouldCountEachStatusCorrectly()
    {
        var items = new List<ComparisonItem>
        {
            new() { RelativePath = "1", Status = ComparisonStatus.Identical },
            new() { RelativePath = "2", Status = ComparisonStatus.Identical },
            new() { RelativePath = "3", Status = ComparisonStatus.Different },
            new() { RelativePath = "4", Status = ComparisonStatus.Different },
            new() { RelativePath = "5", Status = ComparisonStatus.Different },
            new() { RelativePath = "6", Status = ComparisonStatus.LeftOnly },
            new() { RelativePath = "7", Status = ComparisonStatus.LeftOnly },
            new() { RelativePath = "8", Status = ComparisonStatus.LeftOnly },
            new() { RelativePath = "9", Status = ComparisonStatus.LeftOnly },
            new() { RelativePath = "10", Status = ComparisonStatus.RightOnly },
            new() { RelativePath = "11", Status = ComparisonStatus.RightOnly },
            new() { RelativePath = "12", Status = ComparisonStatus.RightOnly },
            new() { RelativePath = "13", Status = ComparisonStatus.RightOnly },
            new() { RelativePath = "14", Status = ComparisonStatus.RightOnly },
            new() { RelativePath = "15", Status = ComparisonStatus.Error },
        };

        var result = new DirectoryComparisonResult
        {
            LeftPath = @"C:\left",
            RightPath = @"C:\right",
            Items = items
        };

        var summary = result.Summary;

        summary.TotalItems.Should().Be(15);
        summary.IdenticalItems.Should().Be(2);
        summary.DifferentItems.Should().Be(3);
        summary.LeftOnlyItems.Should().Be(4);
        summary.RightOnlyItems.Should().Be(5);
        summary.ErrorItems.Should().Be(1);
    }

    [Fact]
    public void FileSystemItem_ShouldStoreAllProperties()
    {
        var now = DateTime.UtcNow;
        var item = new FileSystemItem
        {
            FullPath = @"C:\test\file.txt",
            Name = "file.txt",
            IsDirectory = false,
            Size = 12345,
            LastModified = now,
            RelativePath = "file.txt"
        };

        item.FullPath.Should().Be(@"C:\test\file.txt");
        item.Name.Should().Be("file.txt");
        item.IsDirectory.Should().BeFalse();
        item.Size.Should().Be(12345);
        item.LastModified.Should().Be(now);
        item.RelativePath.Should().Be("file.txt");
    }

    [Fact]
    public void FileSystemItem_ShouldAllowNullSizeForDirectories()
    {
        var item = new FileSystemItem
        {
            FullPath = @"C:\test\folder",
            Name = "folder",
            IsDirectory = true,
            Size = null,
            LastModified = DateTime.UtcNow,
            RelativePath = "folder"
        };

        item.IsDirectory.Should().BeTrue();
        item.Size.Should().BeNull();
    }

    [Fact]
    public void ComparisonItem_ShouldStoreErrorMessage()
    {
        var item = new ComparisonItem
        {
            RelativePath = "error-file.txt",
            LeftItem = new FileSystemItem 
            { 
                FullPath = "left/error-file.txt", 
                Name = "error-file.txt", 
                IsDirectory = false,
                RelativePath = "error-file.txt"
            },
            RightItem = new FileSystemItem 
            { 
                FullPath = "right/error-file.txt", 
                Name = "error-file.txt", 
                IsDirectory = false,
                RelativePath = "error-file.txt"
            },
            Status = ComparisonStatus.Error,
            ErrorMessage = "Access denied"
        };

        item.Status.Should().Be(ComparisonStatus.Error);
        item.ErrorMessage.Should().Be("Access denied");
    }

    [Fact]
    public void TextDiffRequest_ShouldHaveDefaultValues()
    {
        var request = new TextDiffRequest
        {
            LeftFilePath = "left.txt",
            RightFilePath = "right.txt"
        };

        request.LeftFilePath.Should().Be("left.txt");
        request.RightFilePath.Should().Be("right.txt");
        request.IgnoreWhitespace.Should().BeFalse();
        request.IgnoreCase.Should().BeFalse();
    }

    [Fact]
    public void DiffLine_ShouldStoreAllProperties()
    {
        var line = new DiffLine
        {
            ChangeKind = LineChangeKind.Modified,
            LeftLineNumber = 10,
            LeftText = "old text",
            RightLineNumber = 15,
            RightText = "new text"
        };

        line.ChangeKind.Should().Be(LineChangeKind.Modified);
        line.LeftLineNumber.Should().Be(10);
        line.LeftText.Should().Be("old text");
        line.RightLineNumber.Should().Be(15);
        line.RightText.Should().Be("new text");
    }

    [Fact]
    public void DiffSummary_ShouldCalculateTotalCorrectly()
    {
        var summary = new DiffSummary
        {
            TotalLines = 100,
            UnchangedLines = 60,
            AddedLines = 20,
            RemovedLines = 15,
            ModifiedLines = 5
        };

        summary.TotalLines.Should().Be(100);
        summary.UnchangedLines.Should().Be(60);
        summary.AddedLines.Should().Be(20);
        summary.RemovedLines.Should().Be(15);
        summary.ModifiedLines.Should().Be(5);
    }

    [Fact]
    public void TextDiffResult_ShouldStoreAllComponents()
    {
        var lines = new List<DiffLine>
        {
            new() { ChangeKind = LineChangeKind.Unchanged, LeftLineNumber = 1, RightLineNumber = 1 }
        };

        var summary = new DiffSummary
        {
            TotalLines = 1,
            UnchangedLines = 1
        };

        var result = new TextDiffResult
        {
            LeftFilePath = "left.txt",
            RightFilePath = "right.txt",
            Lines = lines,
            Summary = summary
        };

        result.LeftFilePath.Should().Be("left.txt");
        result.RightFilePath.Should().Be("right.txt");
        result.Lines.Should().HaveCount(1);
        result.Summary.TotalLines.Should().Be(1);
    }
}
