#nullable enable

using BinanceP2pMonitor.Utilities;

namespace BinanceP2pMonitor.Benchmarks;

/// <summary>
/// Extension methods for string type to provide additional benchmarking utilities
/// that complement the StringExtensionsBenchmarks class.
/// </summary>
public static class StringExtensionsBenchmarksExtensions
{
    /// <summary>
    /// Counts the number of words in the input string after splitting by camel case.
    /// </summary>
    /// <param name="input">The input string to analyze.</param>
    /// <returns>The number of words after splitting camel case.</returns>
    public static int CountWordsFromCamelCase(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return 0;

        var words = input.SplitCamelCase();
        return words.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    /// <summary>
    /// Determines if the input string is a valid snake_case identifier.
    /// </summary>
    /// <param name="input">The input string to validate.</param>
    /// <returns>True if the string is valid snake_case, false otherwise.</returns>
    public static bool IsValidSnakeCase(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return input.ToSnakeCase() == input &&
               input.All(c => char.IsLower(c) || c == '_' || char.IsDigit(c));
    }

    /// <summary>
    /// Truncates the string and adds ellipsis if truncation occurs.
    /// </summary>
    /// <param name="input">The input string to truncate.</param>
    /// <param name="maxLength">The maximum length before adding ellipsis.</param>
    /// <returns>The truncated string with ellipsis if needed, otherwise the original string.</returns>
    public static string TruncateWithEllipsis(this string input, int maxLength)
    {
        if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
            return input;

        return input[..(maxLength - 3)] + "...";
    }

    /// <summary>
    /// Converts a string to title case (capitalize each word).
    /// </summary>
    /// <param name="input">The input string to convert.</param>
    /// <returns>The string in title case.</returns>
    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var words = input.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                words[i] = char.ToUpperInvariant(words[i][0]) + words[i][1..].ToLowerInvariant();
            }
        }

        return string.Join(" ", words);
    }
}