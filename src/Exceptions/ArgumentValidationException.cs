#nullable enable

using System;
using System.Collections.Generic;

namespace BinanceP2pMonitor.Exceptions;

/// <summary>
/// Thrown when argument validation fails with detailed context
/// </summary>
public class ArgumentValidationException : BinanceP2pException
{
    public Dictionary<string, string> ValidationErrors { get; set; } = new();

    public ArgumentValidationException(string message, Dictionary<string, string> errors, string? errorCode = "ARGUMENT_VALIDATION_ERROR")
        : base(message, errorCode)
    {
        ValidationErrors = errors ?? throw new ArgumentNullException(nameof(errors));
    }

    public ArgumentValidationException(string message, string parameterName, string errorMessage, string? errorCode = "ARGUMENT_VALIDATION_ERROR")
        : base(message, errorCode)
    {
        ValidationErrors[parameterName] = errorMessage;
    }

    public ArgumentValidationException(string message, Exception innerException, string? errorCode = "ARGUMENT_VALIDATION_ERROR")
        : base(message, innerException, errorCode)
    {
        ValidationErrors = new Dictionary<string, string>();
    }

    public override string ToString()
    {
        var baseStr = base.ToString();
        var errorsInfo = ValidationErrors.Count > 0
            ? $"\nValidation Errors: {string.Join(", ", ValidationErrors.Select(kv => $"'{kv.Key}': {kv.Value}"))}"
            : string.Empty;
        return $"{baseStr}{errorsInfo}";
    }
}
