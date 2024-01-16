#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BinanceP2pMonitor.Exceptions;

/// <summary>
/// Extension methods for ArgumentValidationException to provide additional functionality
/// </summary>
public static class ArgumentValidationExceptionExtensions
{
    /// <summary>
    /// Creates a new ArgumentValidationException with additional validation errors merged from the provided dictionary.
    /// </summary>
    /// <param name="exception">The original exception</param>
    /// <param name="additionalErrors">Dictionary of additional validation errors to merge</param>
    /// <returns>A new ArgumentValidationException with merged validation errors</returns>
    public static ArgumentValidationException WithAdditionalErrors(this ArgumentValidationException exception, Dictionary<string, string> additionalErrors)
    {
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        if (additionalErrors == null)
        {
            throw new ArgumentNullException(nameof(additionalErrors));
        }

        var mergedErrors = new Dictionary<string, string>(exception.ValidationErrors);
        foreach (var error in additionalErrors)
        {
            mergedErrors[error.Key] = error.Value;
        }

        return new ArgumentValidationException(exception.Message, mergedErrors, exception.ErrorCode);
    }

    /// <summary>
    /// Creates a new ArgumentValidationException with a single additional validation error added.
    /// </summary>
    /// <param name="exception">The original exception</param>
    /// <param name="parameterName">Name of the parameter that failed validation</param>
    /// <param name="errorMessage">Error message describing the validation failure</param>
    /// <returns>A new ArgumentValidationException with the additional error added</returns>
    public static ArgumentValidationException WithError(this ArgumentValidationException exception, string parameterName, string errorMessage)
    {
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        if (string.IsNullOrWhiteSpace(parameterName))
        {
            throw new ArgumentException("Parameter name cannot be null or whitespace", nameof(parameterName));
        }

        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new ArgumentException("Error message cannot be null or whitespace", nameof(errorMessage));
        }

        var mergedErrors = new Dictionary<string, string>(exception.ValidationErrors);
        mergedErrors[parameterName] = errorMessage;

        return new ArgumentValidationException(exception.Message, mergedErrors, exception.ErrorCode);
    }

    /// <summary>
    /// Gets a formatted string representation of all validation errors.
    /// </summary>
    /// <param name="exception">The exception to get errors from</param>
    /// <returns>Formatted string with all validation errors, or empty string if no errors</returns>
    public static string GetAllErrorMessages(this ArgumentValidationException exception)
    {
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        if (exception.ValidationErrors.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.AppendLine("Validation Errors:");

        foreach (var error in exception.ValidationErrors)
        {
            sb.AppendLine($"  - {error.Key}: {error.Value}");
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Checks if the exception contains a validation error for the specified parameter name.
    /// </summary>
    /// <param name="exception">The exception to check</param>
    /// <param name="parameterName">Name of the parameter to check for</param>
    /// <returns>True if the parameter has a validation error, false otherwise</returns>
    public static bool HasErrorFor(this ArgumentValidationException exception, string parameterName)
    {
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        if (string.IsNullOrWhiteSpace(parameterName))
        {
            throw new ArgumentException("Parameter name cannot be null or whitespace", nameof(parameterName));
        }

        return exception.ValidationErrors.ContainsKey(parameterName);
    }

    /// <summary>
    /// Gets the validation error message for the specified parameter name.
    /// </summary>
    /// <param name="exception">The exception to get the error from</param>
    /// <param name="parameterName">Name of the parameter to get the error for</param>
    /// <returns>The error message if found, or null if no error exists for the parameter</returns>
    public static string? GetErrorMessage(this ArgumentValidationException exception, string parameterName)
    {
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        if (string.IsNullOrWhiteSpace(parameterName))
        {
            throw new ArgumentException("Parameter name cannot be null or whitespace", nameof(parameterName));
        }

        if (exception.ValidationErrors.TryGetValue(parameterName, out var errorMessage))
        {
            return errorMessage;
        }

        return null;
    }
}