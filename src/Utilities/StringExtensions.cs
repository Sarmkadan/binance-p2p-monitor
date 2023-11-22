// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Utilities;

/// <summary>
/// Extension methods for string operations
/// </summary>
public static class StringExtensions
{
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
        var result = System.Text.RegularExpressions.Regex.Replace(
            str,
            "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))",
            "$1 ");

        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(result);
    }

    /// <summary>
    /// Converts PascalCase to snake_case
    /// </summary>
    public static string ToSnakeCase(this string str)
    {
        return System.Text.RegularExpressions.Regex.Replace(
            str,
            "(?<!^)([A-Z])",
            "_$1").ToLowerInvariant();
    }

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
    /// Safely parses string to decimal
    /// </summary>
    public static decimal? ToDecimalOrNull(this string? str)
    {
        return string.IsNullOrWhiteSpace(str)
            ? null
            : decimal.TryParse(str, out var result) ? result : null;
    }

    /// <summary>
    /// Safely parses string to int
    /// </summary>
    public static int? ToIntOrNull(this string? str)
    {
        return string.IsNullOrWhiteSpace(str)
            ? null
            : int.TryParse(str, out var result) ? result : null;
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
