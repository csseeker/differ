using System;
using System.IO;
using System.Text;
using Differ.Core.Interfaces;
using Differ.Core.Models;
using Microsoft.Extensions.Logging;

namespace Differ.Core.Services;

/// <summary>
/// Provides a line-oriented text diff implementation with optional preprocessing rules.
/// </summary>
public class TextDiffService : ITextDiffService
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB guard rail
    private const long MaxLineMatrixCells = 2_000_000;      // Prevent excessive memory usage
    private const int SampleBytesForBinaryDetection = 1024;

    private readonly ILogger<TextDiffService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextDiffService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public TextDiffService(ILogger<TextDiffService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public bool CanDiff(string leftFilePath, string rightFilePath)
    {
        return File.Exists(leftFilePath) && File.Exists(rightFilePath);
    }

    /// <inheritdoc />
    public async Task<OperationResult<TextDiffResult>> ComputeDiffAsync(
        TextDiffRequest request,
        CancellationToken cancellationToken = default,
        IProgress<double>? progress = null)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        try
        {
            if (!File.Exists(request.LeftFilePath))
            {
                return OperationResult<TextDiffResult>.Failure($"Left file does not exist: {request.LeftFilePath}");
            }

            if (!File.Exists(request.RightFilePath))
            {
                return OperationResult<TextDiffResult>.Failure($"Right file does not exist: {request.RightFilePath}");
            }

            var leftInfo = new FileInfo(request.LeftFilePath);
            var rightInfo = new FileInfo(request.RightFilePath);

            if (leftInfo.Length > MaxFileSizeBytes || rightInfo.Length > MaxFileSizeBytes)
            {
                return OperationResult<TextDiffResult>.Failure(
                    "One or both files are too large for the built-in viewer. " +
                    "Please open them in an external diff tool.");
            }

            if (await IsProbablyBinaryAsync(leftInfo, cancellationToken).ConfigureAwait(false) ||
                await IsProbablyBinaryAsync(rightInfo, cancellationToken).ConfigureAwait(false))
            {
                return OperationResult<TextDiffResult>.Failure("Binary files are not supported by the text diff viewer.");
            }

            progress?.Report(0.05);

            var leftLines = await ReadAllLinesAsync(
                leftInfo.FullName,
                request.IgnoreWhitespace,
                request.IgnoreCase,
                cancellationToken).ConfigureAwait(false);

            progress?.Report(0.35);

            var rightLines = await ReadAllLinesAsync(
                rightInfo.FullName,
                request.IgnoreWhitespace,
                request.IgnoreCase,
                cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var matrixCells = (long)leftLines.Count * (long)rightLines.Count;
            if (matrixCells > MaxLineMatrixCells)
            {
                _logger.LogWarning("Diff matrix would be too large (cells: {Cells}). Falling back to failure result.", matrixCells);
                return OperationResult<TextDiffResult>.Failure(
                    "The files are too large or contain too many lines to diff inside the application. " +
                    "Try narrowing your comparison or using an external diff tool.");
            }

            progress?.Report(0.45);

            var diffResult = await Task.Run(
                () => ComputeDiffInternal(leftLines, rightLines, request, cancellationToken),
                cancellationToken).ConfigureAwait(false);

            progress?.Report(1.0);

            return OperationResult<TextDiffResult>.Success(diffResult);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Text diff operation was cancelled");
            return OperationResult<TextDiffResult>.Failure("Diff operation was cancelled by the user.");
        }
        catch (DecoderFallbackException ex)
        {
            _logger.LogError(ex, "Unable to decode file contents during diff.");
            return OperationResult<TextDiffResult>.Failure("Unable to decode one of the files using the expected encoding.", ex);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "I/O error while computing text diff.");
            return OperationResult<TextDiffResult>.Failure($"I/O error occurred: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while computing text diff.");
            return OperationResult<TextDiffResult>.Failure($"Unexpected diff error: {ex.Message}", ex);
        }
    }

    private async Task<IReadOnlyList<LineContent>> ReadAllLinesAsync(
        string filePath,
        bool ignoreWhitespace,
        bool ignoreCase,
        CancellationToken cancellationToken)
    {
        var lines = new List<LineContent>();

        await using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 16 * 1024,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);

        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false)) is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var normalized = Normalize(line, ignoreWhitespace, ignoreCase);
            lines.Add(new LineContent(line, normalized));
        }

        return lines;
    }

    private static string Normalize(string value, bool ignoreWhitespace, bool ignoreCase)
    {
        string normalized = value;

        if (ignoreWhitespace)
        {
            var builder = new StringBuilder(value.Length);
            foreach (var ch in value)
            {
                if (!char.IsWhiteSpace(ch))
                {
                    builder.Append(ch);
                }
            }
            normalized = builder.ToString();
        }

        if (ignoreCase)
        {
            normalized = normalized.ToUpperInvariant();
        }

        return normalized;
    }

    private static async Task<bool> IsProbablyBinaryAsync(FileInfo fileInfo, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            fileInfo.FullName,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: SampleBytesForBinaryDetection,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);

        var buffer = new byte[SampleBytesForBinaryDetection];
        var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false);

        for (int i = 0; i < read; i++)
        {
            if (buffer[i] == 0)
            {
                return true;
            }
        }

        return false;
    }

    private TextDiffResult ComputeDiffInternal(
        IReadOnlyList<LineContent> leftLines,
        IReadOnlyList<LineContent> rightLines,
        TextDiffRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var lcsMatrix = BuildLongestCommonSubsequenceMatrix(leftLines, rightLines, cancellationToken);

        var operations = BuildOperations(leftLines, rightLines, lcsMatrix, cancellationToken);

    var diffLines = new List<DiffLine>(operations.Count);

    int unchanged = 0;
    int added = 0;
    int removed = 0;
    int modified = 0;

    bool ignoreWhitespace = request.IgnoreWhitespace;

        var consumed = new bool[operations.Count];

        for (int index = 0; index < operations.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (consumed[index])
            {
                continue;
            }

            var operation = operations[index];
            switch (operation.Kind)
            {
                case DiffOperationKind.Equal:
                {
                    var leftLine = leftLines[operation.LeftIndex];
                    var rightLine = rightLines[operation.RightIndex];
                    diffLines.Add(new DiffLine
                    {
                        ChangeKind = LineChangeKind.Unchanged,
                        LeftLineNumber = operation.LeftIndex + 1,
                        LeftText = leftLine.Original,
                        RightLineNumber = operation.RightIndex + 1,
                        RightText = rightLine.Original
                    });
                    unchanged++;
                    break;
                }

                case DiffOperationKind.Delete:
                {
                    if (TryPairWithInsert(operations, consumed, index, out var insertOperationIndex))
                    {
                        consumed[insertOperationIndex] = true;
                        var leftLine = leftLines[operation.LeftIndex];
                        var rightLine = rightLines[operations[insertOperationIndex].RightIndex];
                        var normalizedEqual = string.Equals(leftLine.Normalized, rightLine.Normalized, StringComparison.Ordinal);
                        var changeKind = normalizedEqual ? LineChangeKind.Unchanged : LineChangeKind.Modified;

                        if (normalizedEqual)
                        {
                            unchanged++;
                        }
                        else
                        {
                            modified++;
                        }

                        diffLines.Add(new DiffLine
                        {
                            ChangeKind = changeKind,
                            LeftLineNumber = operation.LeftIndex + 1,
                            LeftText = leftLine.Original,
                            RightLineNumber = operations[insertOperationIndex].RightIndex + 1,
                            RightText = rightLine.Original
                        });
                    }
                    else
                    {
                        var leftLine = leftLines[operation.LeftIndex];
                        if (ignoreWhitespace && string.IsNullOrWhiteSpace(leftLine.Original))
                        {
                            continue;
                        }
                        diffLines.Add(new DiffLine
                        {
                            ChangeKind = LineChangeKind.Removed,
                            LeftLineNumber = operation.LeftIndex + 1,
                            LeftText = leftLine.Original,
                            RightLineNumber = null,
                            RightText = null
                        });
                        removed++;
                    }
                    break;
                }

                case DiffOperationKind.Insert:
                {
                    var rightLine = rightLines[operation.RightIndex];
                    if (ignoreWhitespace && string.IsNullOrWhiteSpace(rightLine.Original))
                    {
                        continue;
                    }
                    diffLines.Add(new DiffLine
                    {
                        ChangeKind = LineChangeKind.Added,
                        LeftLineNumber = null,
                        LeftText = null,
                        RightLineNumber = operation.RightIndex + 1,
                        RightText = rightLine.Original
                    });
                    added++;
                    break;
                }
            }
        }

        var summary = new DiffSummary
        {
            TotalLines = diffLines.Count,
            UnchangedLines = unchanged,
            AddedLines = added,
            RemovedLines = removed,
            ModifiedLines = modified
        };

        return new TextDiffResult
        {
            LeftFilePath = request.LeftFilePath,
            RightFilePath = request.RightFilePath,
            Lines = diffLines,
            Summary = summary
        };
    }

    private static bool TryPairWithInsert(
        IReadOnlyList<DiffOperation> operations,
        bool[] consumed,
        int deleteOperationIndex,
        out int insertOperationIndex)
    {
        var nextIndex = deleteOperationIndex + 1;
        if (nextIndex < operations.Count && !consumed[nextIndex] && operations[nextIndex].Kind == DiffOperationKind.Insert)
        {
            insertOperationIndex = nextIndex;
            return true;
        }

        insertOperationIndex = -1;
        return false;
    }

    private static List<DiffOperation> BuildOperations(
        IReadOnlyList<LineContent> leftLines,
        IReadOnlyList<LineContent> rightLines,
        int[,] lcsMatrix,
        CancellationToken cancellationToken)
    {
        var operations = new List<DiffOperation>(leftLines.Count + rightLines.Count);
        int i = 0;
        int j = 0;

        while (i < leftLines.Count && j < rightLines.Count)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.Equals(leftLines[i].Normalized, rightLines[j].Normalized, StringComparison.Ordinal))
            {
                operations.Add(new DiffOperation(DiffOperationKind.Equal, i, j));
                i++;
                j++;
            }
            else if (lcsMatrix[i + 1, j] >= lcsMatrix[i, j + 1])
            {
                operations.Add(new DiffOperation(DiffOperationKind.Delete, i, j));
                i++;
            }
            else
            {
                operations.Add(new DiffOperation(DiffOperationKind.Insert, i, j));
                j++;
            }
        }

        while (i < leftLines.Count)
        {
            cancellationToken.ThrowIfCancellationRequested();
            operations.Add(new DiffOperation(DiffOperationKind.Delete, i, j));
            i++;
        }

        while (j < rightLines.Count)
        {
            cancellationToken.ThrowIfCancellationRequested();
            operations.Add(new DiffOperation(DiffOperationKind.Insert, i, j));
            j++;
        }

        return operations;
    }

    private static int[,] BuildLongestCommonSubsequenceMatrix(
        IReadOnlyList<LineContent> leftLines,
        IReadOnlyList<LineContent> rightLines,
        CancellationToken cancellationToken)
    {
        var matrix = new int[leftLines.Count + 1, rightLines.Count + 1];

        for (int i = leftLines.Count - 1; i >= 0; i--)
        {
            cancellationToken.ThrowIfCancellationRequested();

            for (int j = rightLines.Count - 1; j >= 0; j--)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.Equals(leftLines[i].Normalized, rightLines[j].Normalized, StringComparison.Ordinal))
                {
                    matrix[i, j] = matrix[i + 1, j + 1] + 1;
                }
                else
                {
                    matrix[i, j] = Math.Max(matrix[i + 1, j], matrix[i, j + 1]);
                }
            }
        }

        return matrix;
    }

    private readonly record struct LineContent(string Original, string Normalized);

    private readonly record struct DiffOperation(DiffOperationKind Kind, int LeftIndex, int RightIndex);

    private enum DiffOperationKind
    {
        Equal,
        Delete,
        Insert
    }
}
