#nullable enable

namespace BinanceP2pMonitor.Infrastructure;

/// <summary>
/// Extension methods for <see cref="ConfigurationValidator"/> and related configuration types
/// </summary>
public static class ConfigurationValidatorExtensions
{
    /// <summary>
    /// Determines whether the configuration is valid (no validation errors)
    /// </summary>
    /// <param name="validator">The configuration validator</param>
    /// <returns>True if validation produced no errors; otherwise, false</returns>
    /// <exception cref="ArgumentNullException"><paramref name="validator"/> is <see langword="null"/></exception>
    public static bool IsValid(this ConfigurationValidator validator)
    {
        ArgumentNullException.ThrowIfNull(validator);

        return !validator.Validate().Any();
    }

    /// <summary>
    /// Gets a human-readable summary of the validation result
    /// </summary>
    /// <param name="validator">The configuration validator</param>
    /// <returns>A formatted summary describing whether validation passed or listing the errors</returns>
    /// <exception cref="ArgumentNullException"><paramref name="validator"/> is <see langword="null"/></exception>
    public static string GetValidationSummary(this ConfigurationValidator validator)
    {
        ArgumentNullException.ThrowIfNull(validator);

        var errors = validator.Validate();
        return errors.Any()
            ? $"Configuration validation failed with {errors.Count} error(s):{Environment.NewLine}{errors.FormatErrors()}"
            : "Configuration validation passed";
    }

    /// <summary>
    /// Formats a collection of validation errors as an indented, newline-delimited string
    /// </summary>
    /// <param name="errors">The validation errors to format</param>
    /// <returns>A formatted string with each error on its own line, or an empty string if there are no errors</returns>
    /// <exception cref="ArgumentNullException"><paramref name="errors"/> is <see langword="null"/></exception>
    public static string FormatErrors(this IEnumerable<string> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        return string.Join(Environment.NewLine, errors.Select(e => $"  - {e}"));
    }
}