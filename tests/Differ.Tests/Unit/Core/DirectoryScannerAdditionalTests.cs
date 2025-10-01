using Differ.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Differ.Tests.Unit.Core;

public class DirectoryScannerAdditionalTests
{
    [Fact]
    public async Task ScanDirectoryAsync_ShouldReturnEmptyListForEmptyDirectory()
    {
        var rootPath = CreateTemporaryDirectory();
        try
        {
            var scanner = new DirectoryScanner(NullLogger<DirectoryScanner>.Instance);

            var result = await scanner.ScanDirectoryAsync(rootPath);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Should().BeEmpty();
        }
        finally
        {
            DeleteDirectory(rootPath);
        }
    }

    [Fact]
    public void ValidateDirectoryPath_ShouldFailWhenPathIsEmpty()
    {
        var scanner = new DirectoryScanner(NullLogger<DirectoryScanner>.Instance);

        var result = scanner.ValidateDirectoryPath("");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("cannot be empty");
    }

    [Fact]
    public void ValidateDirectoryPath_ShouldFailWhenPathIsWhitespace()
    {
        var scanner = new DirectoryScanner(NullLogger<DirectoryScanner>.Instance);

        var result = scanner.ValidateDirectoryPath("   ");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("cannot be empty");
    }

    [Fact]
    public void ValidateDirectoryPath_ShouldSucceedForValidDirectory()
    {
        var rootPath = CreateTemporaryDirectory();
        try
        {
            var scanner = new DirectoryScanner(NullLogger<DirectoryScanner>.Instance);

            var result = scanner.ValidateDirectoryPath(rootPath);

            result.IsSuccess.Should().BeTrue();
        }
        finally
        {
            DeleteDirectory(rootPath);
        }
    }

    [Fact]
    public async Task ScanDirectoryAsync_ShouldHandleDeepNestedStructure()
    {
        var rootPath = CreateTemporaryDirectory();
        try
        {
            // Create deeply nested structure: root/a/b/c/d/e/file.txt
            var deepPath = rootPath;
            var directories = new[] { "a", "b", "c", "d", "e" };
            foreach (var dir in directories)
            {
                deepPath = Path.Combine(deepPath, dir);
                Directory.CreateDirectory(deepPath);
            }
            
            var deepFile = Path.Combine(deepPath, "deep-file.txt");
            await File.WriteAllTextAsync(deepFile, "deep content");

            var scanner = new DirectoryScanner(NullLogger<DirectoryScanner>.Instance);

            var result = await scanner.ScanDirectoryAsync(rootPath);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            // Should have 5 directories + 1 file = 6 items
            result.Data!.Should().HaveCount(6);
            result.Data.Should().ContainSingle(i => i.Name == "deep-file.txt" && !i.IsDirectory);
        }
        finally
        {
            DeleteDirectory(rootPath);
        }
    }

    [Fact]
    public async Task ScanDirectoryAsync_ShouldYieldControlPeriodically()
    {
        var rootPath = CreateTemporaryDirectory();
        try
        {
            // Create many files to trigger the yield behavior (>100 files)
            for (int i = 0; i < 150; i++)
            {
                var filePath = Path.Combine(rootPath, $"file{i:000}.txt");
                await File.WriteAllTextAsync(filePath, $"content {i}");
            }

            var scanner = new DirectoryScanner(NullLogger<DirectoryScanner>.Instance);

            var result = await scanner.ScanDirectoryAsync(rootPath);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Should().HaveCount(150);
        }
        finally
        {
            DeleteDirectory(rootPath);
        }
    }

    [Fact]
    public async Task ScanDirectoryAsync_ShouldReportProgress()
    {
        var rootPath = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootPath, "file1.txt"), "test");
            await File.WriteAllTextAsync(Path.Combine(rootPath, "file2.txt"), "test");

            var scanner = new DirectoryScanner(NullLogger<DirectoryScanner>.Instance);
            var progressReports = new List<string>();
            var progress = new Progress<string>(msg => progressReports.Add(msg));

            var result = await scanner.ScanDirectoryAsync(rootPath, progress: progress);

            result.IsSuccess.Should().BeTrue();
            progressReports.Should().NotBeEmpty();
            progressReports.Should().Contain(msg => msg.Contains("Scanning directory"));
        }
        finally
        {
            DeleteDirectory(rootPath);
        }
    }

    [Fact]
    public async Task ScanDirectoryAsync_ShouldIncludeFileMetadata()
    {
        var rootPath = CreateTemporaryDirectory();
        try
        {
            var testFile = Path.Combine(rootPath, "test.txt");
            var testContent = "Hello, World!";
            await File.WriteAllTextAsync(testFile, testContent);
            
            var fileInfo = new FileInfo(testFile);
            var expectedSize = fileInfo.Length;
            var expectedModified = fileInfo.LastWriteTime;

            var scanner = new DirectoryScanner(NullLogger<DirectoryScanner>.Instance);

            var result = await scanner.ScanDirectoryAsync(rootPath);

            result.IsSuccess.Should().BeTrue();
            var fileItem = result.Data!.Single(i => i.Name == "test.txt");
            fileItem.Size.Should().Be(expectedSize);
            fileItem.LastModified.Should().BeCloseTo(expectedModified, TimeSpan.FromSeconds(1));
            fileItem.IsDirectory.Should().BeFalse();
            fileItem.RelativePath.Should().Be("test.txt");
        }
        finally
        {
            DeleteDirectory(rootPath);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "DifferTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void DeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup failures in tests
        }
    }
}
