#nullable enable

using System;
using System.Collections.Generic;

namespace BinanceP2pMonitor.Exceptions;

/// <summary>
/// Thrown when argument validation fails with detailed context
/// </summary>
public class ArgumentValidationException : BinanceP2pException
{
    /// <summary>
    /// Gets or sets the dictionary of validation errors keyed by parameter name.
    /// </summary>
    public Dictionary<string, string> ValidationErrors { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentValidationException"/> class
    /// with a message and a dictionary of validation errors.
    /// </summary>
    /// <param name="message">The error message describing the validation failure.</param>
    /// <param name="errors">A dictionary mapping parameter names to their error descriptions.</param>
    /// <param name="errorCode">An optional error code. Defaults to "ARGUMENT_VALIDATION_ERROR".</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="errors"/> is null.</exception>
    public ArgumentValidationException(string message, Dictionary<string, string> errors, string? errorCode = "ARGUMENT_VALIDATION_ERROR")
        : base(message, errorCode)
    {
        ValidationErrors = errors ?? throw new ArgumentNullException(nameof(errors));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentValidationException"/> class
    /// with a message and a single parameter validation error.
    /// </summary>
    /// <param name="message">The error message describing the validation failure.</param>
    /// <param name="parameterName">The name of the parameter that failed validation.</param>
    /// <param name="errorMessage">The validation error description for the parameter.</param>
    /// <param name="errorCode">An optional error code. Defaults to "ARGUMENT_VALIDATION_ERROR".</param>
    public ArgumentValidationException(string message, string parameterName, string errorMessage, string? errorCode = "ARGUMENT_VALIDATION_ERROR")
        : base(message, errorCode)
    {
        ValidationErrors[parameterName] = errorMessage;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentValidationException"/> class
    /// with a message and an inner exception.
    /// </summary>
    /// <param name="message">The error message describing the validation failure.</param>
    /// <param name="innerException">The exception that caused this validation failure.</param>
    /// <param name="errorCode">An optional error code. Defaults to "ARGUMENT_VALIDATION_ERROR".</param>
    public ArgumentValidationException(string message, Exception innerException, string? errorCode = "ARGUMENT_VALIDATION_ERROR")
        : base(message, innerException, errorCode)
    {
        ValidationErrors = new Dictionary<string, string>();
    }

    /// <summary>
    /// Gets a value indicating whether the exception is transient.
    /// </summary>
    public override bool IsTransient => false;

    /// <summary>
    /// Returns a string representation of the exception including all validation errors.
    /// </summary>
    /// <returns>A string containing the base exception information and any validation errors.</returns>
    public override string ToString()
    {
        var baseStr = base.ToString();
        var errorsInfo = ValidationErrors.Count > 0
            ? $"\nValidation Errors: {string.Join(", ", ValidationErrors.Select(kv => $"'{kv.Key}': {kv.Value}"))}"
            : string.Empty;
        return $"{baseStr}{errorsInfo}";
    }
}
