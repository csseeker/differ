using Differ.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Differ.Tests.Unit.Core;

public class HashFileComparerAdditionalTests
{
    [Fact]
    public async Task CompareFilesAsync_ShouldReturnTrueForEmptyFiles()
    {
        var leftFile = CreateTempFile();
        var rightFile = CreateTempFile();
        try
        {
            // Create empty files
            await File.WriteAllTextAsync(leftFile, string.Empty);
            await File.WriteAllTextAsync(rightFile, string.Empty);

            var comparer = new HashFileComparer(NullLogger<HashFileComparer>.Instance);

            var result = await comparer.CompareFilesAsync(leftFile, rightFile);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue(); // Both empty files are identical
        }
        finally
        {
            DeleteFile(leftFile);
            DeleteFile(rightFile);
        }
    }

    [Fact]
    public async Task CompareFilesAsync_ShouldReturnFalseForDifferentContent()
    {
        var leftFile = CreateTempFile();
        var rightFile = CreateTempFile();
        try
        {
            await File.WriteAllTextAsync(leftFile, "content A");
            await File.WriteAllTextAsync(rightFile, "content B");

            var comparer = new HashFileComparer(NullLogger<HashFileComparer>.Instance);

            var result = await comparer.CompareFilesAsync(leftFile, rightFile);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeFalse();
        }
        finally
        {
            DeleteFile(leftFile);
            DeleteFile(rightFile);
        }
    }

    [Fact]
    public async Task CompareFilesAsync_ShouldHandleLargeFiles()
    {
        var leftFile = CreateTempFile();
        var rightFile = CreateTempFile();
        try
        {
            // Create files larger than buffer size (64KB)
            var largeContent = new string('A', 100 * 1024); // 100KB
            await File.WriteAllTextAsync(leftFile, largeContent);
            await File.WriteAllTextAsync(rightFile, largeContent);

            var comparer = new HashFileComparer(NullLogger<HashFileComparer>.Instance);

            var result = await comparer.CompareFilesAsync(leftFile, rightFile);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();
        }
        finally
        {
            DeleteFile(leftFile);
            DeleteFile(rightFile);
        }
    }

    [Fact]
    public async Task CompareFilesAsync_ShouldDetectDifferenceInLargeFiles()
    {
        var leftFile = CreateTempFile();
        var rightFile = CreateTempFile();
        try
        {
            // Create large files with slight difference
            var leftContent = new string('A', 100 * 1024);
            var rightContent = new string('A', 100 * 1024 - 1) + 'B'; // Last char different
            await File.WriteAllTextAsync(leftFile, leftContent);
            await File.WriteAllTextAsync(rightFile, rightContent);

            var comparer = new HashFileComparer(NullLogger<HashFileComparer>.Instance);

            var result = await comparer.CompareFilesAsync(leftFile, rightFile);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeFalse();
        }
        finally
        {
            DeleteFile(leftFile);
            DeleteFile(rightFile);
        }
    }

    [Fact]
    public async Task CompareFilesAsync_ShouldHandleBinaryFiles()
    {
        var leftFile = CreateTempFile();
        var rightFile = CreateTempFile();
        try
        {
            var binaryData = new byte[] { 0x00, 0x01, 0x02, 0xFF, 0xFE, 0xFD };
            await File.WriteAllBytesAsync(leftFile, binaryData);
            await File.WriteAllBytesAsync(rightFile, binaryData);

            var comparer = new HashFileComparer(NullLogger<HashFileComparer>.Instance);

            var result = await comparer.CompareFilesAsync(leftFile, rightFile);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();
        }
        finally
        {
            DeleteFile(leftFile);
            DeleteFile(rightFile);
        }
    }

    [Fact]
    public void CanCompare_ShouldReturnTrueForExistingFiles()
    {
        var leftFile = CreateTempFile();
        var rightFile = CreateTempFile();
        try
        {
            File.WriteAllText(leftFile, "test");
            File.WriteAllText(rightFile, "test");

            var comparer = new HashFileComparer(NullLogger<HashFileComparer>.Instance);

            var result = comparer.CanCompare(leftFile, rightFile);

            result.Should().BeTrue();
        }
        finally
        {
            DeleteFile(leftFile);
            DeleteFile(rightFile);
        }
    }

    [Fact]
    public void CanCompare_ShouldReturnFalseForNonExistentLeftFile()
    {
        var leftFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");
        var rightFile = CreateTempFile();
        try
        {
            File.WriteAllText(rightFile, "test");

            var comparer = new HashFileComparer(NullLogger<HashFileComparer>.Instance);

            var result = comparer.CanCompare(leftFile, rightFile);

            result.Should().BeFalse();
        }
        finally
        {
            DeleteFile(rightFile);
        }
    }

    [Fact]
    public void CanCompare_ShouldReturnFalseForNonExistentRightFile()
    {
        var leftFile = CreateTempFile();
        var rightFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");
        try
        {
            File.WriteAllText(leftFile, "test");

            var comparer = new HashFileComparer(NullLogger<HashFileComparer>.Instance);

            var result = comparer.CanCompare(leftFile, rightFile);

            result.Should().BeFalse();
        }
        finally
        {
            DeleteFile(leftFile);
        }
    }

    [Fact]
    public void Name_ShouldReturnHashComparer()
    {
        var comparer = new HashFileComparer(NullLogger<HashFileComparer>.Instance);

        comparer.Name.Should().Be("Hash Comparer");
    }

    [Fact]
    public void Description_ShouldContainSHA256()
    {
        var comparer = new HashFileComparer(NullLogger<HashFileComparer>.Instance);

        comparer.Description.Should().Contain("SHA-256");
    }

    private static string CreateTempFile()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".tmp");
        File.WriteAllText(path, string.Empty);
        return path;
    }

    private static void DeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // Ignore cleanup failures
        }
    }
}
