using Differ.Core.Interfaces;
using Differ.Core.Models;
using Differ.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Differ.Tests.Unit.Core;

public class DirectoryComparisonServiceAdditionalTests
{
    [Fact]
    public async Task CompareDirectoriesAsync_ShouldReturnFailureWhenRightValidationFails()
    {
        var directoryScannerMock = new Mock<IDirectoryScanner>(MockBehavior.Strict);
        directoryScannerMock
            .Setup(scanner => scanner.ValidateDirectoryPath("left"))
            .Returns(OperationResult.Success());
        directoryScannerMock
            .Setup(scanner => scanner.ValidateDirectoryPath("right"))
            .Returns(OperationResult.Failure("invalid right path"));

        var fileComparer = Mock.Of<IFileComparer>();
        var service = new DirectoryComparisonService(
            directoryScannerMock.Object,
            fileComparer,
            NullLogger<DirectoryComparisonService>.Instance);

        var result = await service.CompareDirectoriesAsync("left", "right");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Right directory validation failed: invalid right path");
        directoryScannerMock.VerifyAll();
    }

    [Fact]
    public async Task CompareDirectoriesAsync_ShouldReportProgress()
    {
        const string leftRoot = "left-root";
        const string rightRoot = "right-root";

        var leftItems = new List<FileSystemItem>
        {
            CreateFile(leftRoot, "file1.txt")
        };

        var rightItems = new List<FileSystemItem>
        {
            CreateFile(rightRoot, "file1.txt")
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
            .Callback<string, CancellationToken, IProgress<string>>((_, _, p) => p?.Report("Scanning..."))
            .ReturnsAsync(OperationResult<IReadOnlyList<FileSystemItem>>.Success(leftItems));
        directoryScannerMock
            .Setup(scanner => scanner.ScanDirectoryAsync(rightRoot, It.IsAny<CancellationToken>(), It.IsAny<IProgress<string>>()))
            .Callback<string, CancellationToken, IProgress<string>>((_, _, p) => p?.Report("Scanning..."))
            .ReturnsAsync(OperationResult<IReadOnlyList<FileSystemItem>>.Success(rightItems));

        var fileComparerMock = new Mock<IFileComparer>(MockBehavior.Strict);
        fileComparerMock
            .Setup(comparer => comparer.CanCompare(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        fileComparerMock
            .Setup(comparer => comparer.CompareFilesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult<bool>.Success(true));

        var service = new DirectoryComparisonService(
            directoryScannerMock.Object,
            fileComparerMock.Object,
            NullLogger<DirectoryComparisonService>.Instance);

        var progressReports = new List<string>();
        IProgress<string> progress = new SynchronousProgress<string>(msg => progressReports.Add(msg));

        var result = await service.CompareDirectoriesAsync(leftRoot, rightRoot, progress: progress);

        result.IsSuccess.Should().BeTrue();
        progressReports.Should().NotBeEmpty();
        progressReports.Should().Contain(msg => msg.Contains("Validating directories"));
        progressReports.Should().Contain(msg => msg.Contains("Scanning left directory"));
        progressReports.Should().Contain(msg => msg.Contains("Scanning right directory"));
        progressReports.Should().Contain(msg => msg.Contains("Comparing items"));
    }

    private class SynchronousProgress<T> : IProgress<T>
    {
        private readonly Action<T> _action;

        public SynchronousProgress(Action<T> action)
        {
            _action = action;
        }

        public void Report(T value)
        {
            _action(value);
        }
    }

    [Fact]
    public async Task CompareDirectoriesAsync_ShouldHandleEmptyDirectories()
    {
        const string leftRoot = "left-root";
        const string rightRoot = "right-root";

        var leftItems = new List<FileSystemItem>();
        var rightItems = new List<FileSystemItem>();

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

        var fileComparer = Mock.Of<IFileComparer>();
        var service = new DirectoryComparisonService(
            directoryScannerMock.Object,
            fileComparer,
            NullLogger<DirectoryComparisonService>.Instance);

        var result = await service.CompareDirectoriesAsync(leftRoot, rightRoot);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task CompareDirectoriesAsync_ShouldHandleManyItems()
    {
        const string leftRoot = "left-root";
        const string rightRoot = "right-root";

        // Create 100 matching files to test the Task.Yield behavior
        var leftItems = Enumerable.Range(1, 100)
            .Select(i => CreateFile(leftRoot, $"file{i:000}.txt"))
            .ToList<FileSystemItem>();

        var rightItems = Enumerable.Range(1, 100)
            .Select(i => CreateFile(rightRoot, $"file{i:000}.txt"))
            .ToList<FileSystemItem>();

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
            .Setup(comparer => comparer.CanCompare(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        fileComparerMock
            .Setup(comparer => comparer.CompareFilesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult<bool>.Success(true));

        var service = new DirectoryComparisonService(
            directoryScannerMock.Object,
            fileComparerMock.Object,
            NullLogger<DirectoryComparisonService>.Instance);

        var result = await service.CompareDirectoriesAsync(leftRoot, rightRoot);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(100);
        result.Data.Items.Should().OnlyContain(item => item.Status == ComparisonStatus.Identical);
    }

    [Fact]
    public async Task CompareDirectoriesAsync_ShouldHandleUnexpectedExceptionInComparison()
    {
        const string leftRoot = "left-root";
        const string rightRoot = "right-root";

        var leftItems = new List<FileSystemItem> { CreateFile(leftRoot, "file.txt") };
        var rightItems = new List<FileSystemItem> { CreateFile(rightRoot, "file.txt") };

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
            .Setup(comparer => comparer.CanCompare(It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new InvalidOperationException("Unexpected error"));

        var service = new DirectoryComparisonService(
            directoryScannerMock.Object,
            fileComparerMock.Object,
            NullLogger<DirectoryComparisonService>.Instance);

        var result = await service.CompareDirectoriesAsync(leftRoot, rightRoot);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        var item = result.Data!.Items.Single();
        item.Status.Should().Be(ComparisonStatus.Error);
        item.ErrorMessage.Should().Contain("Unexpected error");
    }

    [Fact]
    public async Task CompareDirectoriesAsync_ShouldHandleIdenticalDirectories()
    {
        const string leftRoot = "left-root";
        const string rightRoot = "right-root";

        var leftItems = new List<FileSystemItem>
        {
            CreateDirectory(leftRoot, "folder1"),
            CreateDirectory(leftRoot, "folder2")
        };

        var rightItems = new List<FileSystemItem>
        {
            CreateDirectory(rightRoot, "folder1"),
            CreateDirectory(rightRoot, "folder2")
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

        var fileComparer = Mock.Of<IFileComparer>();
        var service = new DirectoryComparisonService(
            directoryScannerMock.Object,
            fileComparer,
            NullLogger<DirectoryComparisonService>.Instance);

        var result = await service.CompareDirectoriesAsync(leftRoot, rightRoot);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(2);
        result.Data.Items.Should().OnlyContain(item => 
            item.Status == ComparisonStatus.Identical && item.IsDirectory);
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
