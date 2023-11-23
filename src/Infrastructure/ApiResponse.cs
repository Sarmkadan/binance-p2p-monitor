// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json.Serialization;

namespace BinanceP2pMonitor.Infrastructure;

/// <summary>
/// Standard API response wrapper for consistent output format
/// </summary>
public class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("errors")]
    public List<string> Errors { get; set; } = new();

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("request_id")]
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Creates a successful response
    /// </summary>
    public static ApiResponse<T> SuccessResult(T data, string? message = null)
    {
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
    public static new ApiResponse SuccessResult(object? data = null, string? message = null)
    {
        return new ApiResponse
        {
            Success = true,
            Data = data,
            Message = message ?? "Operation completed successfully"
        };
    }

    public static new ApiResponse ErrorResult(string error, string? message = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message ?? "Operation failed",
            Errors = new List<string> { error }
        };
    }

    public static new ApiResponse ErrorResult(List<string> errors, string? message = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message ?? "Operation failed",
            Errors = errors
        };
    }
}
