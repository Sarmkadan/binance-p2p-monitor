#nullable enable
namespace BinanceP2pMonitor.Exceptions;

/// <summary>
/// Base exception for all application-specific errors
/// </summary>
public abstract class BinanceP2pException : Exception
{
    /// <summary>
    /// Gets a value indicating whether the exception is transient (e.g. can be retried).
    /// </summary>
    public abstract bool IsTransient { get; }

    /// <summary>
    /// Gets or sets the application-specific error code (e.g. "INVALID_PRICE").
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets additional diagnostic key/value pairs describing the error.
    /// </summary>
    public Dictionary<string, object>? Context { get; set; }

    /// <summary>
    /// Initializes a new instance with a message, optional error code and optional context.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errorCode">An optional application-specific error code.</param>
    /// <param name="context">Optional diagnostic key/value pairs.</param>
    public BinanceP2pException(string message, string? errorCode = null,
        Dictionary<string, object>? context = null) : base(message)
    {
        ErrorCode = errorCode;
        Context = context;
    }

    /// <summary>
    /// Initializes a new instance with a message, an inner exception and an optional error code.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The exception that caused this error.</param>
    /// <param name="errorCode">An optional application-specific error code.</param>
    public BinanceP2pException(string message, Exception innerException,
        string? errorCode = null) : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Returns a string representation including context entries and the error code, if present.
    /// </summary>
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
    /// <summary>
    /// Initializes a new instance with the specified error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errorCode">The error code; defaults to "INVALID_PRICE".</param>
    public InvalidPriceException(string message, string? errorCode = "INVALID_PRICE")
        : base(message, errorCode) { }

    /// <summary>
    /// Gets a value indicating whether the exception is transient.
    /// </summary>
    public override bool IsTransient => false;
}

/// <summary>
/// Thrown when alert configuration is invalid
/// </summary>
public class InvalidAlertException : BinanceP2pException
{
    /// <summary>
    /// Initializes a new instance with the specified error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errorCode">The error code; defaults to "INVALID_ALERT".</param>
    public InvalidAlertException(string message, string? errorCode = "INVALID_ALERT")
        : base(message, errorCode) { }

    /// <summary>
    /// Gets a value indicating whether the exception is transient.
    /// </summary>
    public override bool IsTransient => false;
}

/// <summary>
/// Thrown when database operation fails
/// </summary>
public class DataAccessException : BinanceP2pException
{
    /// <summary>
    /// Initializes a new instance with a message and the underlying database exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The database exception that caused this error.</param>
    /// <param name="errorCode">The error code; defaults to "DB_ERROR".</param>
    public DataAccessException(string message, Exception innerException,
        string? errorCode = "DB_ERROR") : base(message, innerException, errorCode) { }

    /// <summary>
    /// Gets a value indicating whether the exception is transient.
    /// </summary>
    public override bool IsTransient => true;
}

/// <summary>
/// Thrown when external API call fails
/// </summary>
public class ApiException : BinanceP2pException
{
    /// <summary>
    /// Gets or sets the HTTP status code returned by the external API, if any.
    /// </summary>
    public int? HttpStatusCode { get; set; }

    /// <summary>
    /// Initializes a new instance with a message and optional HTTP status code.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code returned by the API, if any.</param>
    /// <param name="errorCode">The error code; defaults to "API_ERROR".</param>
    public ApiException(string message, int? statusCode = null,
        string? errorCode = "API_ERROR") : base(message, errorCode)
    {
        HttpStatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance with a message, an inner exception and optional HTTP status code.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The exception that caused this error.</param>
    /// <param name="errorCode">An optional application-specific error code.</param>
    /// <param name="statusCode">The HTTP status code returned by the API, if any.</param>
    public ApiException(string message, Exception innerException, string? errorCode = null, int? statusCode = null)
        : base(message, innerException, errorCode)
    {
        HttpStatusCode = statusCode;
    }

    /// <summary>
    /// Gets a value indicating whether the exception is transient.
    /// </summary>
    public override bool IsTransient =>
        HttpStatusCode.HasValue &&
        (HttpStatusCode.Value == 429 || // Too Many Requests
         HttpStatusCode.Value == 408 || // Request Timeout
         (HttpStatusCode.Value >= 500 && HttpStatusCode.Value <= 599)); // 5xx Server Errors
}

/// <summary>
/// Thrown when configuration is missing or invalid
/// </summary>
public class ConfigurationException : BinanceP2pException
{
    /// <summary>
    /// Initializes a new instance with the specified error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errorCode">The error code; defaults to "CONFIG_ERROR".</param>
    public ConfigurationException(string message, string? errorCode = "CONFIG_ERROR")
        : base(message, errorCode) { }

    /// <summary>
    /// Gets a value indicating whether the exception is transient.
    /// </summary>
    public override bool IsTransient => false;
}

/// <summary>
/// Thrown when resource is not found
/// </summary>
public class ResourceNotFoundException : BinanceP2pException
{
    /// <summary>
    /// Initializes a new instance with the specified error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errorCode">The error code; defaults to "NOT_FOUND".</param>
    public ResourceNotFoundException(string message, string? errorCode = "NOT_FOUND")
        : base(message, errorCode) { }

    /// <summary>
    /// Gets a value indicating whether the exception is transient.
    /// </summary>
    public override bool IsTransient => false;
}

/// <summary>
/// Thrown when validation fails
/// </summary>
public class ValidationException : BinanceP2pException
{
    /// <summary>
    /// Gets or sets the individual validation error messages.
    /// </summary>
    public List<string> ValidationErrors { get; set; } = new();

    /// <summary>
    /// Initializes a new instance with a message and the list of validation errors.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errors">The individual validation error messages.</param>
    /// <param name="errorCode">The error code; defaults to "VALIDATION_ERROR".</param>
    public ValidationException(string message, List<string> errors,
        string? errorCode = "VALIDATION_ERROR") : base(message, errorCode)
    {
        ValidationErrors = errors;
    }

    /// <summary>
    /// Gets a value indicating whether the exception is transient.
    /// </summary>
    public override bool IsTransient => false;
}
