#nullable enable
using System.IO;
using System.Text;

namespace BinanceP2pMonitor.Formatters;

/// <summary>
/// Provides convenience extension methods for <see cref="MarkdownOutputFormatter"/>.
/// </summary>
public static class MarkdownOutputFormatterExtensions
{
    private static readonly UTF8Encoding _utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    /// <summary>
    /// Formats a collection of objects as a Markdown table using only the specified columns, in the given order.
    /// </summary>
    /// <param name="formatter">The Markdown formatter.</param>
    /// <param name="data">The objects to format.</param>
    /// <param name="columns">The property names to include as columns.</param>
    /// <returns>A Markdown table with a header row followed by one row per item.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/>, <paramref name="data"/> or <paramref name="columns"/> is null.</exception>
    public static string FormatColumns(this MarkdownOutputFormatter formatter, IEnumerable<object> data, params string[] columns)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(columns);

        return formatter.Format(data, columns);
    }

    /// <summary>
    /// Formats a collection of objects as a Markdown table preceded by a level-two heading.
    /// </summary>
    /// <param name="formatter">The Markdown formatter.</param>
    /// <param name="data">The objects to format.</param>
    /// <param name="title">The heading text written above the table.</param>
    /// <returns>The heading, a blank line, and the formatted Markdown table.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/>, <paramref name="data"/> or <paramref name="title"/> is null.</exception>
    public static string FormatWithHeading(this MarkdownOutputFormatter formatter, IEnumerable<object> data, string title)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(title);

        return $"## {title}{Environment.NewLine}{Environment.NewLine}{formatter.Format(data)}";
    }

    /// <summary>
    /// Formats a collection of objects as a Markdown table, or returns <paramref name="emptyMessage"/> when there is nothing to show.
    /// </summary>
    /// <param name="formatter">The Markdown formatter.</param>
    /// <param name="data">The objects to format. Null or an empty collection yields <paramref name="emptyMessage"/>.</param>
    /// <param name="emptyMessage">The text returned when there is no data.</param>
    /// <returns>The formatted Markdown table, or <paramref name="emptyMessage"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/> or <paramref name="emptyMessage"/> is null.</exception>
    public static string FormatOrMessage(this MarkdownOutputFormatter formatter, IEnumerable<object>? data, string emptyMessage)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(emptyMessage);

        var list = data?.ToList();
        if (list is null || list.Count == 0)
            return emptyMessage;

        return formatter.Format(list);
    }

    /// <summary>
    /// Formats a single object as a Markdown table and writes the result to a <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="formatter">The Markdown formatter.</param>
    /// <param name="data">The object to format. Null is written as "(empty)".</param>
    /// <param name="writer">The writer to receive the formatted Markdown.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/> or <paramref name="writer"/> is null.</exception>
    /// <exception cref="IOException">Thrown when the writer cannot be written to.</exception>
    public static void WriteTo(this MarkdownOutputFormatter formatter, object? data, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(writer);

        writer.Write(formatter.Format(data));
    }

    /// <summary>
    /// Formats a single object as a Markdown table and writes the result to a file, replacing any existing file.
    /// The file is written as UTF-8 without a byte order mark.
    /// </summary>
    /// <param name="formatter">The Markdown formatter.</param>
    /// <param name="data">The object to format. Null is written as "(empty)".</param>
    /// <param name="path">The path of the file to write.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/> or <paramref name="path"/> is null.</exception>
    /// <exception cref="IOException">Thrown when the file cannot be written.</exception>
    public static void FormatToFile(this MarkdownOutputFormatter formatter, object? data, string path)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(path);

        File.WriteAllText(path, formatter.Format(data), _utf8NoBom);
    }
}
