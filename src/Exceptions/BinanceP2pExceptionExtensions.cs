#nullable enable

namespace BinanceP2pMonitor.Exceptions;

/// <summary>
/// Extension methods for BinanceP2pException and its derived types
/// </summary>
public static class BinanceP2pExceptionExtensions
{
    /// <summary>
    /// Determines if the exception is a fatal error that should not be retried
    /// </summary>
    /// <param name="exception">The exception to check</param>
    /// <returns>True if the exception is fatal (configuration, validation, or resource not found)</returns>
    public static bool IsFatal(this BinanceP2pException exception)
    {
        return exception switch
        {
            ConfigurationException => true,
            ValidationException => true,
            ResourceNotFoundException => true,
            _ => false
        };
    }

    /// <summary>
    /// Determines if the exception is a transient error that may succeed on retry
    /// </summary>
    /// <param name="exception">The exception to check</param>
    /// <returns>True if the exception is likely transient (API errors, data access errors)</returns>
    public static bool IsTransient(this BinanceP2pException exception)
    {
        return exception switch
        {
            ApiException apiEx => apiEx.HttpStatusCode is null or >= 500,
            DataAccessException => true,
            InvalidPriceException => false,
            InvalidAlertException => false,
            _ => false
        };
    }

    /// <summary>
    /// Gets a user-friendly error message for the exception
    /// </summary>
    /// <param name="exception">The exception</param>
    /// <returns>A user-friendly error message</returns>
    public static string GetFriendlyMessage(this BinanceP2pException exception)
    {
        return exception switch
        {
            ValidationException validationEx =>
                $"Validation failed: {string.Join(", ", validationEx.ValidationErrors)}",
            ApiException apiEx => apiEx.HttpStatusCode.HasValue
                ? $"API request failed (Status: {apiEx.HttpStatusCode}): {apiEx.Message}"
                : $"API request failed: {apiEx.Message}",
            ConfigurationException => $"Configuration error: {exception.Message}",
            ResourceNotFoundException => $"Resource not found: {exception.Message}",
            DataAccessException => $"Data access error: {exception.Message}",
            InvalidPriceException => $"Invalid price data: {exception.Message}",
            InvalidAlertException => $"Invalid alert configuration: {exception.Message}",
            _ => exception.Message
        };
    }

    /// <summary>
    /// Adds context to an exception if it has a Context dictionary
    /// </summary>
    /// <param name="exception">The exception to add context to</param>
    /// <param name="key">Context key</param>
    /// <param name="value">Context value</param>
    /// <returns>The same exception instance for method chaining</returns>
    public static T AddContext<T>(this T exception, string key, object value) where T : BinanceP2pException
    {
        if (exception.Context is null)
        {
            exception.Context = new Dictionary<string, object>();
        }

        exception.Context[key] = value;
        return exception;
    }
}