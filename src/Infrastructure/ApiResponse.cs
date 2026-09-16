#nullable enable
using System.Text.Json.Serialization;

namespace BinanceP2pMonitor.Infrastructure;

/// <summary>
/// Standard API response wrapper for consistent output format
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation succeeded.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the response data.
    /// </summary>
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    /// <summary>
    /// Gets or sets the response message.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the errors associated with the response.
    /// </summary>
    [JsonPropertyName("errors")]
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Gets or sets the UTC timestamp when the response was created.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the unique identifier for the request.
    /// </summary>
    [JsonPropertyName("request_id")]
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Creates a successful response
    /// </summary>
    public static ApiResponse<T> SuccessResult(T data, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(data);

        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message ?? "Operation completed successfully"
        };
    }

    /// <summary>
    /// Creates a failure response with a single error
    /// </summary>
    public static ApiResponse<T> ErrorResult(string error, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new ApiResponse<T>
        {
            Success = false,
            Message = message ?? "Operation failed",
            Errors = new List<string> { error }
        };
    }

    /// <summary>
    /// Creates a failure response with multiple errors
    /// </summary>
    public static ApiResponse<T> ErrorResult(List<string> errors, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(errors);

        return new ApiResponse<T>
        {
            Success = false,
            Message = message ?? "Operation failed",
            Errors = errors
        };
    }
}

/// <summary>
/// Standard API response for non-generic results
/// </summary>
public class ApiResponse : ApiResponse<object?>
{
    /// <summary>
    /// Creates a successful response.
    /// </summary>
    /// <param name="data">The response data.</param>
    /// <param name="message">An optional response message.</param>
    /// <returns>A successful API response.</returns>
    public static new ApiResponse SuccessResult(object? data = null, string? message = null)
    {
        return new ApiResponse
        {
            Success = true,
            Data = data,
            Message = message ?? "Operation completed successfully"
        };
    }

    /// <summary>
    /// Creates a failure response with a single error.
    /// </summary>
    /// <param name="error">The error associated with the response.</param>
    /// <param name="message">An optional response message.</param>
    /// <returns>A failed API response.</returns>
    public static new ApiResponse ErrorResult(string error, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new ApiResponse
        {
            Success = false,
            Message = message ?? "Operation failed",
            Errors = new List<string> { error }
        };
    }

    /// <summary>
    /// Creates a failure response with multiple errors.
    /// </summary>
    /// <param name="errors">The errors associated with the response.</param>
    /// <param name="message">An optional response message.</param>
    /// <returns>A failed API response.</returns>
    public static new ApiResponse ErrorResult(List<string> errors, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(errors);

        return new ApiResponse
        {
            Success = false,
            Message = message ?? "Operation failed",
            Errors = errors
        };
    }
}
