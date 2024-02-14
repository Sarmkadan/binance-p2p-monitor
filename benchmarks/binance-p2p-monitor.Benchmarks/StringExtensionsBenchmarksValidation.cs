#nullable enable

using System.Globalization;

namespace BinanceP2pMonitor.Benchmarks;

/// <summary>
/// Provides validation helpers for <see cref="StringExtensionsBenchmarks"/> instances.
/// </summary>
public static class StringExtensionsBenchmarksValidation
{
    /// <summary>
    /// Validates a <see cref="StringExtensionsBenchmarks"/> instance and returns any problems found.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>An immutable list of human-readable problem descriptions; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this StringExtensionsBenchmarks? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate SplitCamelCase - should be a non-empty string for valid camelCase input
        if (string.IsNullOrWhiteSpace(value.SplitCamelCase))
        {
            problems.Add($"SplitCamelCase returned null or whitespace");
        }
        else if (value.SplitCamelCase == PascalInput)
        {
            // This is expected behavior, so not a problem
        }

        // Validate ToSnakeCase - should convert PascalCase to snake_case
        if (string.IsNullOrWhiteSpace(value.ToSnakeCase))
        {
            problems.Add($"ToSnakeCase returned null or whitespace");
        }
        else if (value.ToSnakeCase != "binance_price_monitoring_service")
        {
            problems.Add($"ToSnakeCase returned unexpected value: '{value.ToSnakeCase}'");
        }

        // Validate ToPascalCase - should convert snake_case to PascalCase
        if (string.IsNullOrWhiteSpace(value.ToPascalCase))
        {
            problems.Add($"ToPascalCase returned null or whitespace");
        }
        else if (value.ToPascalCase != PascalInput)
        {
            problems.Add($"ToPascalCase returned unexpected value: '{value.ToPascalCase}'");
        }

        // Validate Truncate_Triggered - should truncate to 30 chars
        if (value.Truncate_Triggered.Length != 30)
        {
            problems.Add($"Truncate_Triggered returned string with length {value.Truncate_Triggered.Length}, expected 30");
        }
        else if (!value.Truncate_Triggered.EndsWith("..."))
        {
            problems.Add($"Truncate_Triggered did not append suffix correctly");
        }

        // Validate Truncate_NoOp - should not truncate
        if (value.Truncate_NoOp.Length != LongText.Length)
        {
            problems.Add($"Truncate_NoOp changed string length from {LongText.Length} to {value.Truncate_NoOp.Length}");
        }
        else if (value.Truncate_NoOp != LongText)
        {
            problems.Add($"Truncate_NoOp returned unexpected value");
        }

        // Validate ToDecimalOrNull_Valid - should parse valid decimal
        if (!value.ToDecimalOrNull_Valid.HasValue)
        {
            problems.Add($"ToDecimalOrNull_Valid returned null instead of valid decimal");
        }
        else if (value.ToDecimalOrNull_Valid != 42345.6789m)
        {
            problems.Add($"ToDecimalOrNull_Valid returned {value.ToDecimalOrNull_Valid} instead of 42345.6789");
        }

        // Validate ToDecimalOrNull_Invalid - should return null for invalid input
        if (value.ToDecimalOrNull_Invalid.HasValue)
        {
            problems.Add($"ToDecimalOrNull_Invalid returned {value.ToDecimalOrNull_Invalid} instead of null");
        }

        // Validate ToIntOrNull_Valid - should parse valid integer
        if (!value.ToIntOrNull_Valid.HasValue)
        {
            problems.Add($"ToIntOrNull_Valid returned null instead of valid integer");
        }
        else if (value.ToIntOrNull_Valid != 98765)
        {
            problems.Add($"ToIntOrNull_Valid returned {value.ToIntOrNull_Valid} instead of 98765");
        }

        // Validate Mask - should mask all but first 4 characters
        if (value.Mask.Length != "sk-live-abcdefghijklmnopqrstuvwxyz".Length)
        {
            problems.Add($"Mask returned string with unexpected length: {value.Mask.Length}");
        }
        else if (value.Mask.StartsWith("sk-l") is false)
        {
            problems.Add($"Mask did not preserve first 4 characters correctly");
        }
        else if (value.Mask.Contains("a") || value.Mask.Contains("b") || value.Mask.Contains("c"))
        {
            problems.Add($"Mask did not mask all characters after first 4");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="StringExtensionsBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    public static bool IsValid(this StringExtensionsBenchmarks? value)
        => value?.Validate().Count is 0 or null;

    /// <summary>
    /// Ensures that a <see cref="StringExtensionsBenchmarks"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, with a message listing all problems.</exception>
    public static void EnsureValid(this StringExtensionsBenchmarks? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
            return;

        throw new ArgumentException(
            $"StringExtensionsBenchmarks instance is invalid:{Environment.NewLine}" +
            string.Join(Environment.NewLine, problems.Select((p, i) => $"  {i + 1}. {p}")));
    }

    private const string PascalInput = "BinancePriceMonitoringService";
    private const string LongText = "This is a longer string that may need to be truncated for display purposes in the console output";
}