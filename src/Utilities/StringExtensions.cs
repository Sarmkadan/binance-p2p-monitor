#nullable enable
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
    /// <param name="str">The string to truncate</param>
    /// <param name="maxLength">Maximum length of the result</param>
    /// <param name="suffix">Suffix to append when truncating (default: "...")</param>
    /// <returns>The truncated string or original if within maxLength</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxLength"/> is negative</exception>
    public static string Truncate(this string? str, int maxLength, string suffix = "...")
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);
        ArgumentNullException.ThrowIfNull(suffix);

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
    /// <param name="str">The string to split</param>
    /// <returns>The string with spaces inserted between camelCase/PascalCase words</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is null</exception>
    public static string SplitCamelCase(this string str)
    {
        ArgumentNullException.ThrowIfNull(str);
        var result = _camelCaseRegex.Replace(str, "$1 ");
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(result);
    }

    /// <summary>
    /// Converts PascalCase to snake_case
    /// </summary>
    /// <param name="str">The string to convert</param>
    /// <returns>The snake_case representation</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is null</exception>
    public static string ToSnakeCase(this string str)
    {
        ArgumentNullException.ThrowIfNull(str);
        return _snakeCaseRegex.Replace(str, "_$1").ToLowerInvariant();
    }

    /// <summary>
    /// Converts snake_case to PascalCase
    /// </summary>
    /// <param name="str">The string to convert</param>
    /// <returns>The PascalCase representation</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="str"/> contains empty segments after splitting</exception>
    public static string ToPascalCase(this string str)
    {
        ArgumentNullException.ThrowIfNull(str);
        var parts = str.Split('_');
        return string.Concat(parts.Select(p =>
        {
            if (string.IsNullOrEmpty(p))
                throw new ArgumentException("String contains empty segments after splitting by '_'", nameof(str));
            return char.ToUpperInvariant(p[0]) + p[1..];
        }));
    }

    /// <summary>
    /// Checks if string contains any of the search terms
    /// </summary>
    /// <param name="str">The string to search in</param>
    /// <param name="comparison">String comparison type</param>
    /// <param name="searchTerms">Terms to search for</param>
    /// <returns>True if any term is found; otherwise false</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> or <paramref name="searchTerms"/> is null</exception>
    public static bool ContainsAny(this string str, StringComparison comparison, params string[] searchTerms)
    {
        ArgumentNullException.ThrowIfNull(str);
        ArgumentNullException.ThrowIfNull(searchTerms);
        return searchTerms.Any(term => str.Contains(term, comparison));
    }

    /// <summary>
    /// Checks if string represents a numeric value
    /// </summary>
    /// <param name="str">The string to check</param>
    /// <returns>True if the string contains only digits; otherwise false</returns>
    /// <remarks>
    /// This method only checks for positive integers. For decimal numbers or negative values,
    /// use <see cref="ToDecimalOrNull"/> or <see cref="ToIntOrNull"/> methods instead.
    /// </remarks>
    public static bool IsNumeric(this string str)
    {
        ArgumentNullException.ThrowIfNull(str);
        return str.All(char.IsDigit);
    }

    /// <summary>
    /// Safely parses string to decimal. Uses ReadOnlySpan overload to avoid redundant string allocation.
    /// </summary>
    /// <param name="str">The string to parse</param>
    /// <returns>The parsed decimal value or null if parsing fails or input is null/whitespace</returns>
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
    /// <param name="str">The string to parse</param>
    /// <returns>The parsed integer value or null if parsing fails or input is null/whitespace</returns>
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
    /// <param name="str">The string to mask</param>
    /// <param name="showChars">Number of characters to leave unmasked at the start</param>
    /// <returns>The masked string</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is null</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="showChars"/> is negative</exception>
    public static string Mask(this string str, int showChars = 4)
    {
        ArgumentNullException.ThrowIfNull(str);
        ArgumentOutOfRangeException.ThrowIfNegative(showChars);

        if (str.Length <= showChars)
            return new string('*', str.Length);

        return str[..showChars] + new string('*', str.Length - showChars);
    }
}