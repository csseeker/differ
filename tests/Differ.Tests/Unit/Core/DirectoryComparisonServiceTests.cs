using Differ.Core.Interfaces;
using Differ.Core.Models;
using Differ.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Differ.Tests.Unit.Core;

public class DirectoryComparisonServiceTests
{
    [Fact]
    public async Task CompareDirectoriesAsync_ShouldReturnFailureWhenLeftValidationFails()
    {
        var directoryScannerMock = new Mock<IDirectoryScanner>(MockBehavior.Strict);
        directoryScannerMock
            .Setup(scanner => scanner.ValidateDirectoryPath("left"))
            .Returns(OperationResult.Failure("invalid left"));

        var fileComparer = Mock.Of<IFileComparer>();
        var service = new DirectoryComparisonService(
            directoryScannerMock.Object,
            fileComparer,
            NullLogger<DirectoryComparisonService>.Instance);

        var result = await service.CompareDirectoriesAsync("left", "right");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Left directory validation failed: invalid left");
        directoryScannerMock.VerifyAll();
    }

    [Fact]
    public async Task CompareDirectoriesAsync_ShouldReturnFailureWhenLeftScanFails()
    {
        var directoryScannerMock = new Mock<IDirectoryScanner>(MockBehavior.Strict);
        directoryScannerMock
            .Setup(scanner => scanner.ValidateDirectoryPath("left"))
            .Returns(OperationResult.Success());
        directoryScannerMock
            .Setup(scanner => scanner.ValidateDirectoryPath("right"))
            .Returns(OperationResult.Success());
        directoryScannerMock
            .Setup(scanner => scanner.ScanDirectoryAsync("left", It.IsAny<CancellationToken>(), It.IsAny<IProgress<string>>()))
            .ReturnsAsync(OperationResult<IReadOnlyList<FileSystemItem>>.Failure("left scan failed"));

        var fileComparer = Mock.Of<IFileComparer>();
        var service = new DirectoryComparisonService(
            directoryScannerMock.Object,
            fileComparer,
            NullLogger<DirectoryComparisonService>.Instance);

        var result = await service.CompareDirectoriesAsync("left", "right");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Failed to scan left directory: left scan failed");
        directoryScannerMock.VerifyAll();
    }

    [Fact]
    public async Task CompareDirectoriesAsync_ShouldReturnFailureWhenRightScanFails()
    {
        var directoryScannerMock = new Mock<IDirectoryScanner>(MockBehavior.Strict);
        directoryScannerMock
            .Setup(scanner => scanner.ValidateDirectoryPath("left"))
            .Returns(OperationResult.Success());
        directoryScannerMock
            .Setup(scanner => scanner.ValidateDirectoryPath("right"))
            .Returns(OperationResult.Success());
        directoryScannerMock
            .Setup(scanner => scanner.ScanDirectoryAsync("left", It.IsAny<CancellationToken>(), It.IsAny<IProgress<string>>()))
            .ReturnsAsync(OperationResult<IReadOnlyList<FileSystemItem>>.Success(new List<FileSystemItem>()));
        directoryScannerMock
            .Setup(scanner => scanner.ScanDirectoryAsync("right", It.IsAny<CancellationToken>(), It.IsAny<IProgress<string>>()))
            .ReturnsAsync(OperationResult<IReadOnlyList<FileSystemItem>>.Failure("right scan failed"));

        var fileComparer = Mock.Of<IFileComparer>();
        var service = new DirectoryComparisonService(
            directoryScannerMock.Object,
            fileComparer,
            NullLogger<DirectoryComparisonService>.Instance);

        var result = await service.CompareDirectoriesAsync("left", "right");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Failed to scan right directory: right scan failed");
        directoryScannerMock.VerifyAll();
    }

    [Fact]
    public async Task CompareDirectoriesAsync_ShouldReturnFailureWhenCancelled()
    {
        var leftItems = new List<FileSystemItem> { CreateFile("left", "file.txt") };
        var rightItems = new List<FileSystemItem> { CreateFile("right", "file.txt") };

        var directoryScannerMock = new Mock<IDirectoryScanner>(MockBehavior.Strict);
        directoryScannerMock
            .Setup(scanner => scanner.ValidateDirectoryPath("left"))
            .Returns(OperationResult.Success());
        directoryScannerMock
            .Setup(scanner => scanner.ValidateDirectoryPath("right"))
            .Returns(OperationResult.Success());
        directoryScannerMock
            .Setup(scanner => scanner.ScanDirectoryAsync("left", It.IsAny<CancellationToken>(), It.IsAny<IProgress<string>>()))
            .ReturnsAsync(OperationResult<IReadOnlyList<FileSystemItem>>.Success(leftItems));
        directoryScannerMock
            .Setup(scanner => scanner.ScanDirectoryAsync("right", It.IsAny<CancellationToken>(), It.IsAny<IProgress<string>>()))
            .ReturnsAsync(OperationResult<IReadOnlyList<FileSystemItem>>.Success(rightItems));

        var fileComparer = new Mock<IFileComparer>(MockBehavior.Loose);

        var service = new DirectoryComparisonService(
            directoryScannerMock.Object,
            fileComparer.Object,
            NullLogger<DirectoryComparisonService>.Instance);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await service.CompareDirectoriesAsync("left", "right", cts.Token);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Operation was cancelled");
        directoryScannerMock.VerifyAll();
    }

    [Fact]
    public async Task CompareDirectoriesAsync_ShouldReturnAggregatedComparisonItems()
    {
    const string leftRoot = "left-root";
    const string rightRoot = "right-root";

        var leftItems = new List<FileSystemItem>
        {
            CreateDirectory(leftRoot, "common-folder"),
            CreateFile(leftRoot, "same.txt"),
            CreateFile(leftRoot, "different.txt"),
            CreateFile(leftRoot, "error.txt"),
            CreateFile(leftRoot, "unsupported.bin"),
            CreateDirectory(leftRoot, "type-mismatch"),
            CreateFile(leftRoot, "left-only.txt")
        };

        var rightItems = new List<FileSystemItem>
        {
            CreateDirectory(rightRoot, "common-folder"),
            CreateFile(rightRoot, "same.txt"),
            CreateFile(rightRoot, "different.txt"),
            CreateFile(rightRoot, "error.txt"),
            CreateFile(rightRoot, "unsupported.bin"),
            CreateFile(rightRoot, "type-mismatch"),
            CreateFile(rightRoot, "right-only.txt")
        };

        var directoryScannerMock = new Mock<IDirectoryScanner>(MockBehavior.Strict);
        directoryScannerMock
            .Setup(scanner => scanner.ValidateDirectoryPath(leftRoot))
            .Returns(OperationResult.Success());
        directoryScannerMock
            .Setup(scanner => scanner.ValidateDirectoryPath(rightRoot))
            .Returns(OperationResult.Success());
        directoryScannerMock
            .Setup(scanner => scanner.ScanDirectoryAsync(leftRoot, It.IsAny<CancellationToken>(), It.IsAny<IProgress<string>>()))
            .ReturnsAsync(OperationResult<IReadOnlyList<FileSystemItem>>.Success(leftItems));
        directoryScannerMock
            .Setup(scanner => scanner.ScanDirectoryAsync(rightRoot, It.IsAny<CancellationToken>(), It.IsAny<IProgress<string>>()))
            .ReturnsAsync(OperationResult<IReadOnlyList<FileSystemItem>>.Success(rightItems));

        var fileComparerMock = new Mock<IFileComparer>(MockBehavior.Strict);
        fileComparerMock
            .Setup(comparer => comparer.CanCompare(
                It.Is<string>(path => path.EndsWith("same.txt", StringComparison.OrdinalIgnoreCase)),
                It.Is<string>(path => path.EndsWith("same.txt", StringComparison.OrdinalIgnoreCase))))
            .Returns(true);
        fileComparerMock
            .Setup(comparer => comparer.CanCompare(
                It.Is<string>(path => path.EndsWith("different.txt", StringComparison.OrdinalIgnoreCase)),
                It.Is<string>(path => path.EndsWith("different.txt", StringComparison.OrdinalIgnoreCase))))
            .Returns(true);
        fileComparerMock
            .Setup(comparer => comparer.CanCompare(
                It.Is<string>(path => path.EndsWith("error.txt", StringComparison.OrdinalIgnoreCase)),
                It.Is<string>(path => path.EndsWith("error.txt", StringComparison.OrdinalIgnoreCase))))
            .Returns(true);
        fileComparerMock
            .Setup(comparer => comparer.CanCompare(
                It.Is<string>(path => path.EndsWith("unsupported.bin", StringComparison.OrdinalIgnoreCase)),
                It.Is<string>(path => path.EndsWith("unsupported.bin", StringComparison.OrdinalIgnoreCase))))
            .Returns(false);
        fileComparerMock
            .Setup(comparer => comparer.CompareFilesAsync(
                It.Is<string>(path => path.EndsWith("same.txt", StringComparison.OrdinalIgnoreCase)),
                It.Is<string>(path => path.EndsWith("same.txt", StringComparison.OrdinalIgnoreCase)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult<bool>.Success(true));
        fileComparerMock
            .Setup(comparer => comparer.CompareFilesAsync(
                It.Is<string>(path => path.EndsWith("different.txt", StringComparison.OrdinalIgnoreCase)),
                It.Is<string>(path => path.EndsWith("different.txt", StringComparison.OrdinalIgnoreCase)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult<bool>.Success(false));
        fileComparerMock
            .Setup(comparer => comparer.CompareFilesAsync(
                It.Is<string>(path => path.EndsWith("error.txt", StringComparison.OrdinalIgnoreCase)),
                It.Is<string>(path => path.EndsWith("error.txt", StringComparison.OrdinalIgnoreCase)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult<bool>.Failure("comparison failed"));

        var service = new DirectoryComparisonService(
            directoryScannerMock.Object,
            fileComparerMock.Object,
            NullLogger<DirectoryComparisonService>.Instance);

        var result = await service.CompareDirectoriesAsync(leftRoot, rightRoot);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        var items = result.Data!.Items;

        items.Should().HaveCount(8);

    items.Should().ContainSingle(i => i.RelativePath == Normalize("left-only.txt")
                      && i.Status == ComparisonStatus.LeftOnly
                      && i.LeftItem != null
                      && i.RightItem == null);

        items.Should().ContainSingle(i => i.RelativePath == Normalize("right-only.txt")
                                          && i.Status == ComparisonStatus.RightOnly
                      && i.LeftItem == null
                      && i.RightItem != null);

        items.Should().ContainSingle(i => i.RelativePath == Normalize("common-folder")
                                          && i.Status == ComparisonStatus.Identical
                                          && i.IsDirectory);

        items.Should().ContainSingle(i => i.RelativePath == Normalize("same.txt")
                                          && i.Status == ComparisonStatus.Identical
                                          && !i.IsDirectory);

        items.Should().ContainSingle(i => i.RelativePath == Normalize("different.txt")
                                          && i.Status == ComparisonStatus.Different);

        items.Should().ContainSingle(i => i.RelativePath == Normalize("type-mismatch")
                                          && i.Status == ComparisonStatus.Different);

        items.Should().ContainSingle(i => i.RelativePath == Normalize("error.txt")
                                          && i.Status == ComparisonStatus.Error
                                          && i.ErrorMessage == "comparison failed");

    var unsupportedItem = items.Single(i => i.RelativePath == Normalize("unsupported.bin"));
    unsupportedItem.Status.Should().Be(ComparisonStatus.Error);
    unsupportedItem.ErrorMessage.Should().Be("File comparison not supported for this file type");

        directoryScannerMock.VerifyAll();
        fileComparerMock.VerifyAll();
    }

    private static FileSystemItem CreateFile(string root, string relativePath)
    {
        return new FileSystemItem
        {
            FullPath = Path.Combine(root, Normalize(relativePath)),
            Name = Path.GetFileName(relativePath),
            IsDirectory = false,
            Size = 123,
            LastModified = DateTime.UtcNow,
            RelativePath = Normalize(relativePath)
        };
    }

    private static FileSystemItem CreateDirectory(string root, string relativePath)
    {
        return new FileSystemItem
        {
            FullPath = Path.Combine(root, Normalize(relativePath)),
            Name = Path.GetFileName(relativePath),
            IsDirectory = true,
            Size = null,
            LastModified = DateTime.UtcNow,
            RelativePath = Normalize(relativePath)
        };
    }

    private static string Normalize(string relativePath)
    {
        return relativePath
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);
    }
}
