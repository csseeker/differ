using Differ.Core.Models;
using Differ.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Differ.Tests.Unit.Core;

public class TextDiffServiceAdditionalTests : IDisposable
{
    private readonly List<string> _tempFiles = new();

    [Fact]
    public async Task ComputeDiffAsync_WhenBothFilesAreEmpty_ReturnsEmptyDiff()
    {
        var leftFile = CreateTempFile("");
        var rightFile = CreateTempFile("");

        var service = new TextDiffService(NullLogger<TextDiffService>.Instance);
        var request = new TextDiffRequest
        {
            LeftFilePath = leftFile,
            RightFilePath = rightFile
        };

        var result = await service.ComputeDiffAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Lines.Should().BeEmpty();
        result.Data.Summary.TotalLines.Should().Be(0);
    }

    [Fact]
    public async Task ComputeDiffAsync_WhenLeftFileDoesNotExist_ReturnsFailure()
    {
        var leftFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");
        var rightFile = CreateTempFile("content");

        var service = new TextDiffService(NullLogger<TextDiffService>.Instance);
        var request = new TextDiffRequest
        {
            LeftFilePath = leftFile,
            RightFilePath = rightFile
        };

        var result = await service.ComputeDiffAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Left file does not exist");
    }

    [Fact]
    public async Task ComputeDiffAsync_WhenRightFileDoesNotExist_ReturnsFailure()
    {
        var leftFile = CreateTempFile("content");
        var rightFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");

        var service = new TextDiffService(NullLogger<TextDiffService>.Instance);
        var request = new TextDiffRequest
        {
            LeftFilePath = leftFile,
            RightFilePath = rightFile
        };

        var result = await service.ComputeDiffAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Right file does not exist");
    }

    [Fact]
    public async Task ComputeDiffAsync_WhenFileIsBinary_ReturnsFailure()
    {
        var leftFile = CreateTempBinaryFile();
        var rightFile = CreateTempBinaryFile();

        var service = new TextDiffService(NullLogger<TextDiffService>.Instance);
        var request = new TextDiffRequest
        {
            LeftFilePath = leftFile,
            RightFilePath = rightFile
        };

        var result = await service.ComputeDiffAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Binary files are not supported");
    }

    [Fact]
    public async Task ComputeDiffAsync_WhenFileIsTooLarge_ReturnsFailure()
    {
        var leftFile = CreateLargeTempFile();
        var rightFile = CreateTempFile("small");

        var service = new TextDiffService(NullLogger<TextDiffService>.Instance);
        var request = new TextDiffRequest
        {
            LeftFilePath = leftFile,
            RightFilePath = rightFile
        };

        var result = await service.ComputeDiffAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("too large");
    }

    [Fact]
    public async Task ComputeDiffAsync_WhenOnlyLeftHasContent_ReturnsRemovals()
    {
        var leftFile = CreateTempFile("Line 1\nLine 2\nLine 3");
        var rightFile = CreateTempFile("");

        var service = new TextDiffService(NullLogger<TextDiffService>.Instance);
        var request = new TextDiffRequest
        {
            LeftFilePath = leftFile,
            RightFilePath = rightFile
        };

        var result = await service.ComputeDiffAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Lines.Should().HaveCount(3);
        result.Data.Lines.Should().OnlyContain(line => line.ChangeKind == LineChangeKind.Removed);
        result.Data.Summary.RemovedLines.Should().Be(3);
    }

    [Fact]
    public async Task ComputeDiffAsync_WhenOnlyRightHasContent_ReturnsAdditions()
    {
        var leftFile = CreateTempFile("");
        var rightFile = CreateTempFile("Line 1\nLine 2\nLine 3");

        var service = new TextDiffService(NullLogger<TextDiffService>.Instance);
        var request = new TextDiffRequest
        {
            LeftFilePath = leftFile,
            RightFilePath = rightFile
        };

        var result = await service.ComputeDiffAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Lines.Should().HaveCount(3);
        result.Data.Lines.Should().OnlyContain(line => line.ChangeKind == LineChangeKind.Added);
        result.Data.Summary.AddedLines.Should().Be(3);
    }

    [Fact]
    public async Task ComputeDiffAsync_WithIgnoreCase_TreatsUpperAndLowerAsIdentical()
    {
        var leftFile = CreateTempFile("Hello World");
        var rightFile = CreateTempFile("HELLO WORLD");

        var service = new TextDiffService(NullLogger<TextDiffService>.Instance);
        var request = new TextDiffRequest
        {
            LeftFilePath = leftFile,
            RightFilePath = rightFile,
            IgnoreCase = true
        };

        var result = await service.ComputeDiffAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Lines.Should().HaveCount(1);
        result.Data.Lines[0].ChangeKind.Should().Be(LineChangeKind.Unchanged);
    }

    [Fact]
    public async Task ComputeDiffAsync_WithoutIgnoreCase_TreatsCaseAsDifferent()
    {
        var leftFile = CreateTempFile("Hello World");
        var rightFile = CreateTempFile("HELLO WORLD");

        var service = new TextDiffService(NullLogger<TextDiffService>.Instance);
        var request = new TextDiffRequest
        {
            LeftFilePath = leftFile,
            RightFilePath = rightFile,
            IgnoreCase = false
        };

        var result = await service.ComputeDiffAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Lines.Should().HaveCount(1);
        result.Data.Lines[0].ChangeKind.Should().Be(LineChangeKind.Modified);
    }

    [Fact]
    public async Task ComputeDiffAsync_ShouldHandleCancellation()
    {
        var leftFile = CreateTempFile("Line 1\nLine 2\nLine 3");
        var rightFile = CreateTempFile("Line A\nLine B\nLine C");

        var service = new TextDiffService(NullLogger<TextDiffService>.Instance);
        var request = new TextDiffRequest
        {
            LeftFilePath = leftFile,
            RightFilePath = rightFile
        };

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await service.ComputeDiffAsync(request, cts.Token);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("cancelled");
    }

    [Fact]
    public async Task ComputeDiffAsync_ShouldReportProgress()
    {
        var leftFile = CreateTempFile("Line 1\nLine 2");
        var rightFile = CreateTempFile("Line 1\nLine 2");

        var service = new TextDiffService(NullLogger<TextDiffService>.Instance);
        var request = new TextDiffRequest
        {
            LeftFilePath = leftFile,
            RightFilePath = rightFile
        };

        var progressReports = new List<double>();
        var progress = new Progress<double>(p => progressReports.Add(p));

        var result = await service.ComputeDiffAsync(request, progress: progress);

        result.IsSuccess.Should().BeTrue();
        progressReports.Should().NotBeEmpty();
        progressReports.Should().Contain(1.0); // Should complete at 100%
    }

    [Fact]
    public async Task ComputeDiffAsync_WithMixedChanges_ReturnsCorrectSummary()
    {
        var leftFile = CreateTempFile("Line 1\nLine 2\nLine 3\nLine 5");
        var rightFile = CreateTempFile("Line 1\nLine 2 Modified\nLine 4\nLine 5");

        var service = new TextDiffService(NullLogger<TextDiffService>.Instance);
        var request = new TextDiffRequest
        {
            LeftFilePath = leftFile,
            RightFilePath = rightFile
        };

        var result = await service.ComputeDiffAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        var summary = result.Data!.Summary;
        summary.UnchangedLines.Should().BeGreaterThan(0);
        summary.ModifiedLines.Should().BeGreaterThan(0);
        summary.RemovedLines.Should().BeGreaterOrEqualTo(0);
        summary.AddedLines.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task ComputeDiffAsync_WithOnlyWhitespaceLines_IgnoresWhenFlagSet()
    {
        var leftFile = CreateTempFile("Line 1\n   \nLine 2");
        var rightFile = CreateTempFile("Line 1\nLine 2");

        var service = new TextDiffService(NullLogger<TextDiffService>.Instance);
        var request = new TextDiffRequest
        {
            LeftFilePath = leftFile,
            RightFilePath = rightFile,
            IgnoreWhitespace = true
        };

        var result = await service.ComputeDiffAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        // The whitespace-only line should not appear as a removal
        result.Data!.Summary.UnchangedLines.Should().Be(2);
    }

    [Fact]
    public void CanDiff_ShouldReturnTrueForExistingFiles()
    {
        var leftFile = CreateTempFile("test");
        var rightFile = CreateTempFile("test");

        var service = new TextDiffService(NullLogger<TextDiffService>.Instance);

        var result = service.CanDiff(leftFile, rightFile);

        result.Should().BeTrue();
    }

    [Fact]
    public void CanDiff_ShouldReturnFalseForNonExistentFiles()
    {
        var leftFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");
        var rightFile = CreateTempFile("test");

        var service = new TextDiffService(NullLogger<TextDiffService>.Instance);

        var result = service.CanDiff(leftFile, rightFile);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ComputeDiffAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        var service = new TextDiffService(NullLogger<TextDiffService>.Instance);

        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await service.ComputeDiffAsync(null!));
    }

    private string CreateTempFile(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");
        File.WriteAllText(path, content);
        _tempFiles.Add(path);
        return path;
    }

    private string CreateTempBinaryFile()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".bin");
        // Create a file with null bytes to be detected as binary
        var binaryData = new byte[2048];
        binaryData[100] = 0; // Null byte
        binaryData[500] = 0; // Another null byte
        File.WriteAllBytes(path, binaryData);
        _tempFiles.Add(path);
        return path;
    }

    private string CreateLargeTempFile()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");
        // Create a file larger than 10MB
        using (var writer = new StreamWriter(path))
        {
            var largeContent = new string('A', 1024); // 1KB per line
            for (int i = 0; i < 11000; i++) // 11MB+
            {
                writer.WriteLine(largeContent);
            }
        }
        _tempFiles.Add(path);
        return path;
    }

    public void Dispose()
    {
        foreach (var file in _tempFiles)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            catch
            {
                // Ignore cleanup failures
            }
        }
    }
}
