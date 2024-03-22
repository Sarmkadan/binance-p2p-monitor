#nullable enable

namespace BinanceP2pMonitor.Infrastructure;

/// <summary>
/// Extension methods for <see cref="ConsoleOutputWriter"/> providing additional formatting and output utilities.
/// </summary>
public static class ConsoleOutputWriterExtensions
{
    /// <summary>
    /// Writes a formatted success message with additional context.
    /// </summary>
    /// <param name="writer">The <see cref="ConsoleOutputWriter"/> instance.</param>
    /// <param name="message">The main success message.</param>
    /// <param name="context">Additional context information.</param>
    /// <exception cref="ArgumentNullException"><paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="message"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> is <see langword="null"/>.</exception>
    public static void WriteSuccessWithContext(this ConsoleOutputWriter writer, string message, string context)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(context);

        writer.WriteSuccess($"{message} {context}");
    }

    /// <summary>
    /// Writes a formatted error message with error code.
    /// </summary>
    /// <param name="writer">The <see cref="ConsoleOutputWriter"/> instance.</param>
    /// <param name="message">The error message.</param>
    /// <param name="errorCode">The error code or identifier.</param>
    /// <exception cref="ArgumentNullException"><paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="message"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="errorCode"/> is <see langword="null"/>.</exception>
    public static void WriteErrorWithCode(this ConsoleOutputWriter writer, string message, string errorCode)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(errorCode);

        writer.WriteError($"[{errorCode}] {message}");
    }

    /// <summary>
    /// Writes a formatted warning message with source information.
    /// </summary>
    /// <param name="writer">The <see cref="ConsoleOutputWriter"/> instance.</param>
    /// <param name="message">The warning message.</param>
    /// <param name="source">The source component or module.</param>
    /// <exception cref="ArgumentNullException"><paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="message"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static void WriteWarningWithSource(this ConsoleOutputWriter writer, string message, string source)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(source);

        writer.WriteWarning($"[{source}] {message}");
    }

    /// <summary>
    /// Writes a formatted info message with timestamp.
    /// </summary>
    /// <param name="writer">The <see cref="ConsoleOutputWriter"/> instance.</param>
    /// <param name="message">The information message.</param>
    /// <param name="timestamp">Optional timestamp to display. If <see langword="null"/>, uses current time.</param>
    /// <exception cref="ArgumentNullException"><paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="message"/> is <see langword="null"/>.</exception>
    public static void WriteInfoWithTimestamp(this ConsoleOutputWriter writer, string message, DateTime? timestamp = null)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(message);

        var time = timestamp?.ToString("yyyy-MM-dd HH:mm:ss") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        writer.WriteInfo($"[{time}] {message}");
    }

    /// <summary>
    /// Writes a section header with optional subtitle.
    /// </summary>
    /// <param name="writer">The <see cref="ConsoleOutputWriter"/> instance.</param>
    /// <param name="title">The main title.</param>
    /// <param name="subtitle">Optional subtitle. If not null or whitespace, will be displayed below the title.</param>
    /// <exception cref="ArgumentNullException"><paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="title"/> is <see langword="null"/>.</exception>
    public static void WriteSectionWithSubtitle(this ConsoleOutputWriter writer, string title, string? subtitle = null)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(title);

        writer.WriteSection(title);

        if (!string.IsNullOrWhiteSpace(subtitle))
        {
            var originalForeground = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($" {subtitle}");
            Console.ForegroundColor = originalForeground;
        }
    }

    /// <summary>
    /// Writes a key-value pair with formatted value highlighting.
    /// </summary>
    /// <param name="writer">The <see cref="ConsoleOutputWriter"/> instance.</param>
    /// <param name="key">The key/label.</param>
    /// <param name="value">The value to display.</param>
    /// <param name="keyWidth">Width for key column. Default is 20.</param>
    /// <param name="isHighlighted">Whether to highlight the value using green color.</param>
    /// <exception cref="ArgumentNullException"><paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="keyWidth"/> is less than 1.</exception>
    public static void WriteKeyValueHighlighted(this ConsoleOutputWriter writer, string key, string value, int keyWidth = 20, bool isHighlighted = false)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentOutOfRangeException.ThrowIfLessThan(keyWidth, 1);

        var originalForeground = Console.ForegroundColor;

        if (isHighlighted)
        {
            Console.ForegroundColor = ConsoleColor.Green;
        }

        Console.WriteLine($"{key.PadRight(keyWidth)} : {value}");

        if (isHighlighted)
        {
            Console.ForegroundColor = originalForeground;
        }
    }

    /// <summary>
    /// Writes a blank line with configurable spacing.
    /// </summary>
    /// <param name="writer">The <see cref="ConsoleOutputWriter"/> instance.</param>
    /// <param name="count">Number of blank lines to write. Must be non-negative.</param>
    /// <exception cref="ArgumentNullException"><paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
    public static void WriteBlankLines(this ConsoleOutputWriter writer, int count)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        for (int i = 0; i < count; i++)
        {
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Writes a separator line with custom title.
    /// </summary>
    /// <param name="writer">The <see cref="ConsoleOutputWriter"/> instance.</param>
    /// <param name="title">Optional title to display in separator. If provided, creates a centered title section.</param>
    /// <param name="character">Character to use for separator. Default is '-'.</param>
    /// <exception cref="ArgumentNullException"><paramref name="writer"/> is <see langword="null"/>.</exception>
    public static void WriteSeparator(this ConsoleOutputWriter writer, string? title = null, char character = '-')
    {
        ArgumentNullException.ThrowIfNull(writer);

        if (string.IsNullOrWhiteSpace(title))
        {
            Console.WriteLine(new string(character, 80));
        }
        else
        {
            var originalForeground = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n{new string(character, 30)} {title} {new string(character, 30)}");
            Console.ForegroundColor = originalForeground;
        }
    }

    /// <summary>
    /// Writes a progress indicator with percentage.
    /// </summary>
    /// <param name="writer">The <see cref="ConsoleOutputWriter"/> instance.</param>
    /// <param name="current">Current progress value. Must be non-negative.</param>
    /// <param name="total">Total value. Must be positive.</param>
    /// <param name="prefix">Optional prefix text to display before the progress bar.</param>
    /// <exception cref="ArgumentNullException"><paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="current"/> is negative.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="total"/> is not positive.</exception>
    public static void WriteProgress(this ConsoleOutputWriter writer, int current, int total, string? prefix = null)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentOutOfRangeException.ThrowIfNegative(current);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(total, 0);

        var percentage = (int)Math.Round((double)current / total * 100);
        var barLength = 40;
        var completedLength = (int)Math.Round((double)current / total * barLength);
        var bar = new string('█', completedLength) + new string('░', barLength - completedLength);

        var message = prefix != null
            ? $"{prefix} [{bar}] {current}/{total} ({percentage}%)"
            : $"[{bar}] {current}/{total} ({percentage}%)";

        writer.WriteInfo(message);
    }
}
