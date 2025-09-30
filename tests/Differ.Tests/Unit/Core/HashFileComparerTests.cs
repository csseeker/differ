using Differ.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Differ.Tests.Unit.Core;

public class HashFileComparerTests
{
    [Fact]
    public async Task CompareFilesAsync_ShouldReturnIdenticalForMatchingContent()
    {
        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var leftFile = Path.Combine(tempDirectory, "left.txt");
            var rightFile = Path.Combine(tempDirectory, "right.txt");
            await File.WriteAllTextAsync(leftFile, "sample content");
            await File.WriteAllTextAsync(rightFile, "sample content");

            var comparer = new HashFileComparer(NullLogger<HashFileComparer>.Instance);

            var result = await comparer.CompareFilesAsync(leftFile, rightFile);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();
        }
        finally
        {
            DeleteDirectory(tempDirectory);
        }
    }

    [Fact]
    public async Task CompareFilesAsync_ShouldReturnDifferentWhenSizesMismatch()
    {
        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var leftFile = Path.Combine(tempDirectory, "left.bin");
            var rightFile = Path.Combine(tempDirectory, "right.bin");
            await File.WriteAllTextAsync(leftFile, "short");
            await File.WriteAllTextAsync(rightFile, "much longer content");

            var comparer = new HashFileComparer(NullLogger<HashFileComparer>.Instance);

            var result = await comparer.CompareFilesAsync(leftFile, rightFile);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeFalse();
        }
        finally
        {
            DeleteDirectory(tempDirectory);
        }
    }

    [Fact]
    public async Task CompareFilesAsync_ShouldFailWhenFileMissing()
    {
        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var leftFile = Path.Combine(tempDirectory, "missing.txt");
            var rightFile = Path.Combine(tempDirectory, "existing.txt");
            await File.WriteAllTextAsync(rightFile, "content");

            var comparer = new HashFileComparer(NullLogger<HashFileComparer>.Instance);

            var result = await comparer.CompareFilesAsync(leftFile, rightFile);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Left file does not exist");
        }
        finally
        {
            DeleteDirectory(tempDirectory);
        }
    }

    [Fact]
    public async Task CompareFilesAsync_ShouldReturnCancelledWhenTokenCancelled()
    {
        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var leftFile = Path.Combine(tempDirectory, "left.txt");
            var rightFile = Path.Combine(tempDirectory, "right.txt");
            await File.WriteAllTextAsync(leftFile, "content");
            await File.WriteAllTextAsync(rightFile, "content");

            var comparer = new HashFileComparer(NullLogger<HashFileComparer>.Instance);
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var result = await comparer.CompareFilesAsync(leftFile, rightFile, cts.Token);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Operation was cancelled");
        }
        finally
        {
            DeleteDirectory(tempDirectory);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "DifferHashComparerTests", Guid.NewGuid().ToString("N"));
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
