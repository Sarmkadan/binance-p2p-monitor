#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Exceptions;

/// <summary>
/// Base exception for all application-specific errors
/// </summary>
public class BinanceP2pException : Exception
{
    public string? ErrorCode { get; set; }
    public Dictionary<string, object>? Context { get; set; }

    public BinanceP2pException(string message, string? errorCode = null,
        Dictionary<string, object>? context = null) : base(message)
    {
        ErrorCode = errorCode;
        Context = context;
    }

    public BinanceP2pException(string message, Exception innerException,
        string? errorCode = null) : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public override string ToString()
    {
        var contextInfo = Context is not null && Context.Count > 0
            ? $"\nContext: {string.Join(", ", Context.Select(kv => $"{kv.Key}={kv.Value}"))}"
            : string.Empty;

        return $"{base.ToString()}{contextInfo}{(ErrorCode is not null ? $"\nErrorCode: {ErrorCode}" : string.Empty)}";
    }
}

/// <summary>
/// Thrown when price data is invalid
/// </summary>
public class InvalidPriceException : BinanceP2pException
{
    public InvalidPriceException(string message, string? errorCode = "INVALID_PRICE")
        : base(message, errorCode) { }
}

/// <summary>
/// Thrown when alert configuration is invalid
/// </summary>
public class InvalidAlertException : BinanceP2pException
{
    public InvalidAlertException(string message, string? errorCode = "INVALID_ALERT")
        : base(message, errorCode) { }
}

/// <summary>
/// Thrown when database operation fails
/// </summary>
public class DataAccessException : BinanceP2pException
{
    public DataAccessException(string message, Exception innerException,
        string? errorCode = "DB_ERROR") : base(message, innerException, errorCode) { }
}

/// <summary>
/// Thrown when external API call fails
/// </summary>
public class ApiException : BinanceP2pException
{
    public int? HttpStatusCode { get; set; }

    public ApiException(string message, int? statusCode = null,
        string? errorCode = "API_ERROR") : base(message, errorCode)
    {
        HttpStatusCode = statusCode;
    }
}

/// <summary>
/// Thrown when configuration is missing or invalid
/// </summary>
public class ConfigurationException : BinanceP2pException
{
    public ConfigurationException(string message, string? errorCode = "CONFIG_ERROR")
        : base(message, errorCode) { }
}

/// <summary>
/// Thrown when resource is not found
/// </summary>
public class ResourceNotFoundException : BinanceP2pException
{
    public ResourceNotFoundException(string message, string? errorCode = "NOT_FOUND")
        : base(message, errorCode) { }
}

/// <summary>
/// Thrown when validation fails
/// </summary>
public class ValidationException : BinanceP2pException
{
    public List<string> ValidationErrors { get; set; } = new();

    public ValidationException(string message, List<string> errors,
        string? errorCode = "VALIDATION_ERROR") : base(message, errorCode)
    {
        ValidationErrors = errors;
    }
}
