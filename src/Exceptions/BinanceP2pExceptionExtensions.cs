#nullable enable

namespace BinanceP2pMonitor.Exceptions;

/// <summary>
/// Extension methods for <see cref="BinanceP2pException"/> and its derived types.
/// Provides utility methods for exception classification and enrichment.
/// </summary>
public static class BinanceP2pExceptionExtensions
{
    /// <summary>
    /// Determines if the exception is a fatal error that should not be retried.
    /// Fatal exceptions represent configuration errors, validation failures, or missing resources
    /// that are unlikely to succeed on subsequent attempts.
    /// </summary>
    /// <param name="exception">The exception to check. Must not be <see langword="null"/>.</param>
    /// <returns>True if the exception is fatal (configuration, validation, or resource not found); otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
    public static bool IsFatal(this BinanceP2pException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            ConfigurationException => true,
            ValidationException => true,
            ResourceNotFoundException => true,
            _ => false
        };
    }

    /// <summary>
    /// Determines if the exception is a transient error that may succeed on retry.
    /// Transient exceptions are typically network-related, rate-limiting, or temporary service issues.
    /// </summary>
    /// <param name="exception">The exception to check. Must not be <see langword="null"/>.</param>
    /// <returns>
    /// True if the exception is likely transient:
    /// <list type="bullet">
    /// <item><see cref="ApiException"/> with null status code or 5xx server errors</item>
    /// <item><see cref="DataAccessException"/></item>
    /// </list>
    /// False for validation, configuration, and business rule violations.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
    public static bool IsTransient(this BinanceP2pException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

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
    /// Gets a user-friendly error message for the exception.
    /// </summary>
    /// <param name="exception">The exception to format. Must not be <see langword="null"/>.</param>
    /// <returns>A user-friendly error message suitable for display to end users.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
    public static string GetFriendlyMessage(this BinanceP2pException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

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
    /// Adds context to an exception if it has a Context dictionary.
    /// If the Context dictionary does not exist, it is initialized.
    /// </summary>
    /// <typeparam name="T">The type of exception, constrained to <see cref="BinanceP2pException"/>.</typeparam>
    /// <param name="exception">The exception to add context to. Must not be <see langword="null"/>.</param>
    /// <param name="key">Context key. Must not be <see langword="null"/> or empty.</param>
    /// <param name="value">Context value.</param>
    /// <returns>The same exception instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="exception"/> is <see langword="null"/>,
    /// or when <paramref name="key"/> is <see langword="null"/> or empty.
    /// </exception>
    public static T AddContext<T>(this T exception, string key, object value) where T : BinanceP2pException
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(key);

        exception.Context ??= new Dictionary<string, object>();
        exception.Context[key] = value;
        return exception;
    }
}