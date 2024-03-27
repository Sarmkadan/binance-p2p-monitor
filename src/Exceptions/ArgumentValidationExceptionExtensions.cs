#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace BinanceP2pMonitor.Exceptions;

/// <summary>
/// Provides extension methods for <see cref="ArgumentValidationException"/> to enhance validation error handling and inspection.
/// </summary>
public static class ArgumentValidationExceptionExtensions
{
    /// <summary>
    /// Creates a new <see cref="ArgumentValidationException"/> with additional validation errors merged from the provided dictionary.
    /// </summary>
    /// <param name="exception">The original exception.</param>
    /// <param name="additionalErrors">Dictionary of additional validation errors to merge.</param>
    /// <returns>A new <see cref="ArgumentValidationException"/> with merged validation errors.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> or <paramref name="additionalErrors"/> is <see langword="null"/>.</exception>
    public static ArgumentValidationException WithAdditionalErrors(this ArgumentValidationException exception, Dictionary<string, string> additionalErrors)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(additionalErrors);

        var mergedErrors = new Dictionary<string, string>(exception.ValidationErrors);
        foreach (var error in additionalErrors)
        {
            mergedErrors[error.Key] = error.Value;
        }

        return new ArgumentValidationException(exception.Message, mergedErrors, exception.ErrorCode);
    }

    /// <summary>
    /// Creates a new <see cref="ArgumentValidationException"/> with a single additional validation error added.
    /// </summary>
    /// <param name="exception">The original exception.</param>
    /// <param name="parameterName">Name of the parameter that failed validation.</param>
    /// <param name="errorMessage">Error message describing the validation failure.</param>
    /// <returns>A new <see cref="ArgumentValidationException"/> with the additional error added.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="parameterName"/> or <paramref name="errorMessage"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    public static ArgumentValidationException WithError(this ArgumentValidationException exception, string parameterName, string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

        var mergedErrors = new Dictionary<string, string>(exception.ValidationErrors);
        mergedErrors[parameterName] = errorMessage;

        return new ArgumentValidationException(exception.Message, mergedErrors, exception.ErrorCode);
    }

    /// <summary>
    /// Gets a formatted string representation of all validation errors.
    /// </summary>
    /// <param name="exception">The exception to get errors from.</param>
    /// <returns>Formatted string with all validation errors, or empty string if no errors.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
    public static string GetAllErrorMessages(this ArgumentValidationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (exception.ValidationErrors.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.AppendLine("Validation Errors:");

        foreach (var error in exception.ValidationErrors)
        {
            sb.AppendLine($" - {error.Key}: {error.Value}");
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Checks if the exception contains a validation error for the specified parameter name.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <param name="parameterName">Name of the parameter to check for.</param>
    /// <returns>True if the parameter has a validation error; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> or <paramref name="parameterName"/> is <see langword="null"/>.</exception>
    public static bool HasErrorFor(this ArgumentValidationException exception, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);

        return exception.ValidationErrors.ContainsKey(parameterName);
    }

    /// <summary>
    /// Gets the validation error message for the specified parameter name.
    /// </summary>
    /// <param name="exception">The exception to get the error from.</param>
    /// <param name="parameterName">Name of the parameter to get the error for.</param>
    /// <returns>The error message if found; otherwise, <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> or <paramref name="parameterName"/> is <see langword="null"/>.</exception>
    public static string? GetErrorMessage(this ArgumentValidationException exception, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);

        return exception.ValidationErrors.TryGetValue(parameterName, out var errorMessage)
            ? errorMessage
            : null;
    }
}