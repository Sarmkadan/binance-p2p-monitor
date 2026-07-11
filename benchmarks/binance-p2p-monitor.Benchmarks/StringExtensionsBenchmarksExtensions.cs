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
    /// Counts the number of words in a camelCase string.
    /// </summary>
    /// <param name="input">The input string to analyze. Cannot be null.</param>
    /// <returns>The number of words in the camelCase string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static int CountWordsFromCamelCase(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrWhiteSpace(input))
            return 0;

        int count = 1;
        for (int i = 1; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]) && !char.IsUpper(input[i - 1]))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Determines if the input string is a valid snake_case identifier.
    /// </summary>
    /// <param name="input">The input string to validate. Cannot be null.</param>
    /// <returns>True if the string is valid snake_case, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static bool IsValidSnakeCase(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrWhiteSpace(input))
            return false;

        return input.All(c => char.IsLower(c) || c == '_' || char.IsDigit(c)) &&
               !input.StartsWith('_') &&
               !input.EndsWith('_') &&
               !input.Contains("__");
    }

    /// <summary>
    /// Truncates the string and adds ellipsis if truncation occurs.
    /// </summary>
    /// <param name="input">The input string to truncate. Cannot be null.</param>
    /// <param name="maxLength">The maximum length before adding ellipsis. Must be positive.</param>
    /// <returns>The truncated string with ellipsis if needed, otherwise the original string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxLength"/> is less than 3.</exception>
    public static string TruncateWithEllipsis(this string input, int maxLength)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxLength, 3);

        if (input.Length <= maxLength)
            return input;

        return input[..(maxLength - 3)] + "...";
    }

    /// <summary>
    /// Converts a string to title case (capitalize each word).
    /// </summary>
    /// <param name="input">The input string to convert. Cannot be null.</param>
    /// <returns>The string in title case.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static string ToTitleCase(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

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