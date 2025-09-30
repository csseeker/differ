using Differ.Core.Interfaces;
using Differ.Core.Models;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Differ.Core.Services;

/// <summary>
/// Hash-based file comparer that compares files by computing their SHA-256 hashes
/// </summary>
public class HashFileComparer : IFileComparer
{
    private readonly ILogger<HashFileComparer> _logger;
    private const int BufferSize = 64 * 1024; // 64KB buffer for streaming

    /// <summary>
    /// Initializes a new instance of the HashFileComparer class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public HashFileComparer(ILogger<HashFileComparer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string Name => "Hash Comparer";

    /// <inheritdoc />
    public string Description => "Compares files by computing SHA-256 hashes. Suitable for all file types.";

    /// <inheritdoc />
    public bool CanCompare(string leftFilePath, string rightFilePath)
    {
        // Hash comparison works for all file types
        return File.Exists(leftFilePath) && File.Exists(rightFilePath);
    }

    /// <inheritdoc />
    public async Task<OperationResult<bool>> CompareFilesAsync(
        string leftFilePath,
        string rightFilePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Comparing files: {LeftFile} vs {RightFile}", leftFilePath, rightFilePath);

            // Quick check: if file sizes are different, files are different
            var leftInfo = new FileInfo(leftFilePath);
            var rightInfo = new FileInfo(rightFilePath);

            if (!leftInfo.Exists)
            {
                return OperationResult<bool>.Failure($"Left file does not exist: {leftFilePath}");
            }

            if (!rightInfo.Exists)
            {
                return OperationResult<bool>.Failure($"Right file does not exist: {rightFilePath}");
            }

            if (leftInfo.Length != rightInfo.Length)
            {
                _logger.LogDebug("Files have different sizes: {LeftSize} vs {RightSize}", 
                    leftInfo.Length, rightInfo.Length);
                return OperationResult<bool>.Success(false);
            }

            // If both files are empty, they're identical
            if (leftInfo.Length == 0)
            {
                _logger.LogDebug("Both files are empty, considering them identical");
                return OperationResult<bool>.Success(true);
            }

            // Compare hashes
            var leftHash = await ComputeFileHashAsync(leftFilePath, cancellationToken).ConfigureAwait(false);
            var rightHash = await ComputeFileHashAsync(rightFilePath, cancellationToken).ConfigureAwait(false);

            var areIdentical = leftHash.SequenceEqual(rightHash);
            
            _logger.LogDebug("Hash comparison result: {Result}", areIdentical ? "Identical" : "Different");
            
            return OperationResult<bool>.Success(areIdentical);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("File comparison was cancelled");
            return OperationResult<bool>.Failure("Operation was cancelled");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Access denied while comparing files");
            return OperationResult<bool>.Failure($"Access denied: {ex.Message}", ex);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "I/O error while comparing files");
            return OperationResult<bool>.Failure($"I/O error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while comparing files");
            return OperationResult<bool>.Failure($"Unexpected error: {ex.Message}", ex);
        }
    }

    private async Task<byte[]> ComputeFileHashAsync(string filePath, CancellationToken cancellationToken)
    {
        using var sha256 = SHA256.Create();
        await using var fileStream = new FileStream(
            filePath, 
            FileMode.Open, 
            FileAccess.Read, 
            FileShare.Read, 
            BufferSize, 
            FileOptions.SequentialScan);

        var buffer = new byte[BufferSize];
        int bytesRead;

        while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
        {
            sha256.TransformBlock(buffer, 0, bytesRead, null, 0);
            cancellationToken.ThrowIfCancellationRequested();
        }

        sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        return sha256.Hash ?? Array.Empty<byte>();
    }
}