using System;
using System.IO;
using System.Linq;
using Differ.Core.Models;
using Differ.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Differ.Tests.Unit.Core;

public sealed class TextDiffServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly TextDiffService _sut;

    public TextDiffServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"Differ_TextDiff_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDirectory);
        _sut = new TextDiffService(NullLogger<TextDiffService>.Instance);
    }

    [Fact]
    public async Task ComputeDiffAsync_WhenFilesAreIdentical_ReturnsNoDifferences()
    {
        var leftPath = WriteFile("left.txt", "alpha", "beta", "gamma");
        var rightPath = WriteFile("right.txt", "alpha", "beta", "gamma");

        var request = new TextDiffRequest
        {
            LeftFilePath = leftPath,
            RightFilePath = rightPath
        };

        var result = await _sut.ComputeDiffAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.HasDifferences.Should().BeFalse();
        result.Data.Summary.ModifiedLines.Should().Be(0);
        result.Data.Summary.AddedLines.Should().Be(0);
        result.Data.Summary.RemovedLines.Should().Be(0);
    }

    [Fact]
    public async Task ComputeDiffAsync_WhenLinesDiffer_ReturnsModifiedEntry()
    {
        var leftPath = WriteFile("left2.txt", "alpha", "beta", "gamma");
        var rightPath = WriteFile("right2.txt", "alpha", "delta", "gamma");

        var request = new TextDiffRequest
        {
            LeftFilePath = leftPath,
            RightFilePath = rightPath
        };

        var result = await _sut.ComputeDiffAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.HasDifferences.Should().BeTrue();
        result.Data.Summary.ModifiedLines.Should().Be(1);

        var modifiedLine = result.Data.Lines.Single(l => l.ChangeKind == LineChangeKind.Modified);
        modifiedLine.LeftText.Should().Be("beta");
        modifiedLine.RightText.Should().Be("delta");
    }

    [Fact]
    public async Task ComputeDiffAsync_IgnoreWhitespace_TreatsSpacingOnlyChangesAsUnchanged()
    {
        var leftPath = WriteFile("left3.txt", "alpha", "beta", "gamma");
        var rightPath = WriteFile("right3.txt", "alpha", "   beta   ", "gamma");

        var request = new TextDiffRequest
        {
            LeftFilePath = leftPath,
            RightFilePath = rightPath,
            IgnoreWhitespace = true
        };

        var result = await _sut.ComputeDiffAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.HasDifferences.Should().BeFalse();
    }

    [Fact]
    public async Task ComputeDiffAsync_IgnoreWhitespace_DoesNotCountWhitespaceOnlyInsertions()
    {
        var leftPath = WriteFile("left4.txt", "alpha", "beta", "gamma");
        var rightPath = WriteFile("right4.txt", "alpha", "   ", "beta", "gamma");

        var request = new TextDiffRequest
        {
            LeftFilePath = leftPath,
            RightFilePath = rightPath,
            IgnoreWhitespace = true
        };

        var result = await _sut.ComputeDiffAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.HasDifferences.Should().BeFalse();
        result.Data.Summary.AddedLines.Should().Be(0);
        result.Data.Summary.RemovedLines.Should().Be(0);
    }

    private string WriteFile(string relativeName, params string[] lines)
    {
        var path = Path.Combine(_tempDirectory, relativeName);
        File.WriteAllLines(path, lines);
        return path;
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }
        catch (IOException)
        {
            // Ignore cleanup exceptions
        }
    }
}
