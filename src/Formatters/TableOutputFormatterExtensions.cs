#nullable enable
using System.IO;

namespace BinanceP2pMonitor.Formatters;

/// <summary>
/// Provides convenience extension methods for <see cref="TableOutputFormatter"/>.
/// </summary>
public static class TableOutputFormatterExtensions
{
    /// <summary>
    /// Formats a collection of objects as a table using only the specified columns, in the given order.
    /// </summary>
    /// <param name="formatter">The table formatter.</param>
    /// <param name="data">The objects to format.</param>
    /// <param name="columns">The property names to include as columns.</param>
    /// <returns>A table with a header row followed by one row per item.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/>, <paramref name="data"/> or <paramref name="columns"/> is null.</exception>
    public static string FormatColumns(this TableOutputFormatter formatter, IEnumerable<object> data, params string[] columns)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(columns);

        return formatter.Format(data, columns);
    }

    /// <summary>
    /// Formats a collection of objects as a table preceded by a title line.
    /// </summary>
    /// <param name="formatter">The table formatter.</param>
    /// <param name="data">The objects to format.</param>
    /// <param name="title">The title written above the table.</param>
    /// <returns>The title, a line break, and the formatted table.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/>, <paramref name="data"/> or <paramref name="title"/> is null.</exception>
    public static string FormatWithTitle(this TableOutputFormatter formatter, IEnumerable<object> data, string title)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(title);

        return $"{title}{Environment.NewLine}{formatter.Format(data)}";
    }

    /// <summary>
    /// Formats a collection of objects as a table, or returns <paramref name="emptyMessage"/> when there is nothing to show.
    /// </summary>
    /// <param name="formatter">The table formatter.</param>
    /// <param name="data">The objects to format. Null or an empty collection yields <paramref name="emptyMessage"/>.</param>
    /// <param name="emptyMessage">The text returned when there is no data.</param>
    /// <returns>The formatted table, or <paramref name="emptyMessage"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/> or <paramref name="emptyMessage"/> is null.</exception>
    public static string FormatOrMessage(this TableOutputFormatter formatter, IEnumerable<object>? data, string emptyMessage)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(emptyMessage);

        var list = data?.ToList();
        if (list is null || list.Count == 0)
            return emptyMessage;

        return formatter.Format(list);
    }

    /// <summary>
    /// Formats a single object as a table and writes the result to a <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="formatter">The table formatter.</param>
    /// <param name="data">The object to format. Null is written as "(empty)".</param>
    /// <param name="writer">The writer to receive the formatted table.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/> or <paramref name="writer"/> is null.</exception>
    /// <exception cref="IOException">Thrown when the writer cannot be written to.</exception>
    public static void WriteTo(this TableOutputFormatter formatter, object? data, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(writer);

        writer.Write(formatter.Format(data));
    }
}
