#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Text.RegularExpressions;

namespace BinanceP2pMonitor.Utilities;

/// <summary>
/// Extension methods for string operations
/// </summary>
public static class StringExtensions
{
    // Compiled once at startup; avoids per-call regex interpretation overhead.
    private static readonly Regex _camelCaseRegex = new(
        @"([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100));

    private static readonly Regex _snakeCaseRegex = new(
        @"(?<!^)([A-Z])",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100));

    /// <summary>
    /// Safely truncates string to maximum length with optional suffix
    /// </summary>
    public static string Truncate(this string? str, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(str))
            return string.Empty;

        if (str.Length <= maxLength)
            return str;

        var truncateLength = Math.Max(0, maxLength - suffix.Length);
        return str[..truncateLength] + suffix;
    }

    /// <summary>
    /// Splits string by camelCase or PascalCase
    /// </summary>
    public static string SplitCamelCase(this string str)
    {
        var result = _camelCaseRegex.Replace(str, "$1 ");
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(result);
    }

    /// <summary>
    /// Converts PascalCase to snake_case
    /// </summary>
    public static string ToSnakeCase(this string str)
        => _snakeCaseRegex.Replace(str, "_$1").ToLowerInvariant();

    /// <summary>
    /// Converts snake_case to PascalCase
    /// </summary>
    public static string ToPascalCase(this string str)
    {
        var parts = str.Split('_');
        return string.Concat(parts.Select(p => char.ToUpper(p[0]) + p[1..]));
    }

    /// <summary>
    /// Checks if string contains any of the search terms
    /// </summary>
    public static bool ContainsAny(this string str, StringComparison comparison, params string[] searchTerms)
    {
        return searchTerms.Any(term => str.Contains(term, comparison));
    }

    /// <summary>
    /// Checks if string is numeric
    /// </summary>
    public static bool IsNumeric(this string str)
    {
        return !string.IsNullOrEmpty(str) && str.All(char.IsDigit);
    }

    /// <summary>
    /// Safely parses string to decimal. Uses ReadOnlySpan overload to avoid redundant string allocation.
    /// </summary>
    public static decimal? ToDecimalOrNull(this string? str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return null;
        return decimal.TryParse(str.AsSpan(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
    }

    /// <summary>
    /// Safely parses string to int. Uses ReadOnlySpan overload to avoid redundant string allocation.
    /// </summary>
    public static int? ToIntOrNull(this string? str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return null;
        return int.TryParse(str.AsSpan(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
    }

    /// <summary>
    /// Masks sensitive string parts (e.g., API key)
    /// </summary>
    public static string Mask(this string str, int showChars = 4)
    {
        if (str.Length <= showChars)
            return new string('*', str.Length);

        return str[..showChars] + new string('*', str.Length - showChars);
    }
}
