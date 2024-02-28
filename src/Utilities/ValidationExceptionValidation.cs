#nullable enable

namespace BinanceP2pMonitor.Utilities;

/// <summary>
/// Provides validation helpers for <see cref="ValidationException"/> instances
/// </summary>
public static class ValidationExceptionValidation
{
    /// <summary>
    /// Validates a <see cref="ValidationException"/> instance and returns a list of validation problems.
    /// </summary>
    /// <param name="value">The exception to validate</param>
    /// <returns>An immutable list of human-readable validation problems, or empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this ValidationException? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.Errors == null)
        {
            problems.Add("Errors collection cannot be null");
            return problems.AsReadOnly();
        }

        if (value.Errors.Count == 0)
        {
            problems.Add("Errors collection cannot be empty");
        }

        for (var i = 0; i < value.Errors.Count; i++)
        {
            var error = value.Errors[i];
            if (string.IsNullOrWhiteSpace(error))
            {
                problems.Add($"Errors[{i}] cannot be null, empty, or whitespace");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ValidationException"/> is valid.
    /// </summary>
    /// <param name="value">The exception to check</param>
    /// <returns>true if valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static bool IsValid(this ValidationException? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.Errors is not null
            && value.Errors.Count > 0
            && value.Errors.All(e => !string.IsNullOrWhiteSpace(e));
    }

    /// <summary>
    /// Ensures that the specified <see cref="ValidationException"/> is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The exception to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, with a detailed message</exception>
    public static void EnsureValid(this ValidationException? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"ValidationException is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
    }
}
