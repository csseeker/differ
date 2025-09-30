namespace Differ.Core.Models;

/// <summary>
/// Represents the result of an operation with success/error state
/// </summary>
/// <typeparam name="T">The type of data returned on success</typeparam>
public class OperationResult<T>
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful
    /// </summary>
    public bool IsSuccess { get; init; }
    
    /// <summary>
    /// Gets the data returned by the operation (null if failed)
    /// </summary>
    public T? Data { get; init; }
    
    /// <summary>
    /// Gets the error message if the operation failed
    /// </summary>
    public string? ErrorMessage { get; init; }
    
    /// <summary>
    /// Gets the exception that caused the failure (if any)
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// Creates a successful result with data
    /// </summary>
    /// <param name="data">The data to return</param>
    /// <returns>A successful operation result</returns>
    public static OperationResult<T> Success(T data)
    {
        return new OperationResult<T> 
        { 
            IsSuccess = true, 
            Data = data 
        };
    }

    /// <summary>
    /// Creates a failed result with an error message
    /// </summary>
    /// <param name="errorMessage">The error message</param>
    /// <param name="exception">The exception that caused the failure (optional)</param>
    /// <returns>A failed operation result</returns>
    public static OperationResult<T> Failure(string errorMessage, Exception? exception = null)
    {
        return new OperationResult<T> 
        { 
            IsSuccess = false, 
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Represents the result of an operation without return data
/// </summary>
public class OperationResult
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful
    /// </summary>
    public bool IsSuccess { get; init; }
    
    /// <summary>
    /// Gets the error message if the operation failed
    /// </summary>
    public string? ErrorMessage { get; init; }
    
    /// <summary>
    /// Gets the exception that caused the failure (if any)
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    /// <returns>A successful operation result</returns>
    public static OperationResult Success()
    {
        return new OperationResult { IsSuccess = true };
    }

    /// <summary>
    /// Creates a failed result with an error message
    /// </summary>
    /// <param name="errorMessage">The error message</param>
    /// <param name="exception">The exception that caused the failure (optional)</param>
    /// <returns>A failed operation result</returns>
    public static OperationResult Failure(string errorMessage, Exception? exception = null)
    {
        return new OperationResult 
        { 
            IsSuccess = false, 
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}