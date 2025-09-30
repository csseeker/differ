using Differ.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Differ.Tests.Unit.Core;

public class DirectoryScannerTests
{
    [Fact]
    public async Task ScanDirectoryAsync_ShouldReturnAllItemsWithMetadata()
    {
        var rootPath = CreateTemporaryDirectory();
        try
        {
            var subDirectory = Path.Combine(rootPath, "SubFolder");
            Directory.CreateDirectory(subDirectory);
            var nestedDirectory = Path.Combine(subDirectory, "Nested");
            Directory.CreateDirectory(nestedDirectory);

            var rootFile = Path.Combine(rootPath, "root.txt");
            await File.WriteAllTextAsync(rootFile, "root file");
            var subFile = Path.Combine(subDirectory, "sub.txt");
            await File.WriteAllTextAsync(subFile, "sub file");
            var nestedFile = Path.Combine(nestedDirectory, "nested.bin");
            await File.WriteAllBytesAsync(nestedFile, new byte[] { 1, 2, 3, 4 });

            var scanner = new DirectoryScanner(NullLogger<DirectoryScanner>.Instance);

            var result = await scanner.ScanDirectoryAsync(rootPath);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            var items = result.Data!;
            items.Should().HaveCount(5);

            items.Should().ContainSingle(i => i.RelativePath == Path.GetRelativePath(rootPath, subDirectory)
                                              && i.IsDirectory);
            items.Should().ContainSingle(i => i.RelativePath == Path.GetRelativePath(rootPath, nestedDirectory)
                                              && i.IsDirectory);

            var rootFileInfo = new FileInfo(rootFile);
            items.Should().ContainSingle(i => i.RelativePath == Path.GetRelativePath(rootPath, rootFile)
                                              && !i.IsDirectory
                                              && i.Size == rootFileInfo.Length);

            var subFileInfo = new FileInfo(subFile);
            items.Should().ContainSingle(i => i.RelativePath == Path.GetRelativePath(rootPath, subFile)
                                              && !i.IsDirectory
                                              && i.Size == subFileInfo.Length);

            var nestedFileInfo = new FileInfo(nestedFile);
            items.Should().ContainSingle(i => i.RelativePath == Path.GetRelativePath(rootPath, nestedFile)
                                              && !i.IsDirectory
                                              && i.Size == nestedFileInfo.Length);
        }
        finally
        {
            DeleteDirectory(rootPath);
        }
    }

    [Fact]
    public void ValidateDirectoryPath_ShouldFailWhenDirectoryDoesNotExist()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var scanner = new DirectoryScanner(NullLogger<DirectoryScanner>.Instance);

        var result = scanner.ValidateDirectoryPath(nonExistentPath);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNull();
        result.ErrorMessage.Should().Contain("does not exist");
    }

    [Fact]
    public async Task ScanDirectoryAsync_ShouldReturnCancelledResultWhenTokenCancelled()
    {
        var rootPath = CreateTemporaryDirectory();
        try
        {
            var scanner = new DirectoryScanner(NullLogger<DirectoryScanner>.Instance);
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var result = await scanner.ScanDirectoryAsync(rootPath, cts.Token);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Operation was cancelled");
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
