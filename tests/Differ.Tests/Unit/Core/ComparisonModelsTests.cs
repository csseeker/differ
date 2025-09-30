using Differ.Core.Models;
using FluentAssertions;

namespace Differ.Tests.Unit.Core;

public class ComparisonModelsTests
{
    [Fact]
    public void DirectoryComparisonResult_Summary_ShouldCalculateCorrectly()
    {
        // Arrange
        var items = new List<ComparisonItem>
        {
            new() { RelativePath = "file1.txt", Status = ComparisonStatus.Identical },
            new() { RelativePath = "file2.txt", Status = ComparisonStatus.Different },
            new() { RelativePath = "file3.txt", Status = ComparisonStatus.LeftOnly },
            new() { RelativePath = "file4.txt", Status = ComparisonStatus.RightOnly },
            new() { RelativePath = "file5.txt", Status = ComparisonStatus.Error },
            new() { RelativePath = "file6.txt", Status = ComparisonStatus.Identical }
        };

        var result = new DirectoryComparisonResult
        {
            LeftPath = @"C:\left",
            RightPath = @"C:\right",
            Items = items
        };

        // Act
        var summary = result.Summary;

        // Assert
        summary.TotalItems.Should().Be(6);
        summary.IdenticalItems.Should().Be(2);
        summary.DifferentItems.Should().Be(1);
        summary.LeftOnlyItems.Should().Be(1);
        summary.RightOnlyItems.Should().Be(1);
        summary.ErrorItems.Should().Be(1);
    }

    [Fact]
    public void ComparisonItem_IsDirectory_ShouldReturnCorrectValue()
    {
        // Arrange & Act
        var fileItem = new ComparisonItem
        {
            RelativePath = "file.txt",
            LeftItem = new FileSystemItem 
            { 
                FullPath = @"C:\left\file.txt", 
                Name = "file.txt", 
                IsDirectory = false, 
                RelativePath = "file.txt" 
            },
            Status = ComparisonStatus.Identical
        };

        var dirItem = new ComparisonItem
        {
            RelativePath = "folder",
            RightItem = new FileSystemItem 
            { 
                FullPath = @"C:\right\folder", 
                Name = "folder", 
                IsDirectory = true, 
                RelativePath = "folder" 
            },
            Status = ComparisonStatus.RightOnly
        };

        // Assert
        fileItem.IsDirectory.Should().BeFalse();
        dirItem.IsDirectory.Should().BeTrue();
    }
}