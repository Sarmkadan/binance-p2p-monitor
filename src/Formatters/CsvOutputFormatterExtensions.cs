#nullable enable
using System.IO;
using System.Text;

namespace BinanceP2pMonitor.Formatters;

/// <summary>
/// Provides convenience extension methods for <see cref="CsvOutputFormatter"/>.
/// </summary>
public static class CsvOutputFormatterExtensions
{
    private static readonly UTF8Encoding _utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    /// <summary>
    /// Formats a collection of objects as CSV using only the specified columns, in the given order.
    /// </summary>
    /// <param name="formatter">The CSV formatter.</param>
    /// <param name="data">The objects to format.</param>
    /// <param name="columns">The property names to include as columns.</param>
    /// <returns>CSV text with a header row followed by one row per item.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/>, <paramref name="data"/> or <paramref name="columns"/> is null.</exception>
    public static string FormatColumns(this CsvOutputFormatter formatter, IEnumerable<object> data, params string[] columns)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(columns);

        return formatter.Format(data, columns);
    }

    /// <summary>
    /// Formats a single object as CSV and writes the result to a file, replacing any existing file.
    /// The file is written as UTF-8 without a byte order mark.
    /// </summary>
    /// <param name="formatter">The CSV formatter.</param>
    /// <param name="data">The object to format. Null produces an empty file.</param>
    /// <param name="path">The path of the file to write.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/> or <paramref name="path"/> is null.</exception>
    /// <exception cref="IOException">Thrown when the file cannot be written.</exception>
    public static void FormatToFile(this CsvOutputFormatter formatter, object? data, string path)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(path);

        File.WriteAllText(path, formatter.Format(data), _utf8NoBom);
    }

    /// <summary>
    /// Asynchronously formats a single object as CSV and writes the result to a file, replacing any existing file.
    /// The file is written as UTF-8 without a byte order mark.
    /// </summary>
    /// <param name="formatter">The CSV formatter.</param>
    /// <param name="data">The object to format. Null produces an empty file.</param>
    /// <param name="path">The path of the file to write.</param>
    /// <param name="cancellationToken">A token to cancel the write operation.</param>
    /// <returns>A task that completes when the file has been written.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/> or <paramref name="path"/> is null.</exception>
    /// <exception cref="IOException">Thrown when the file cannot be written.</exception>
    public static Task FormatToFileAsync(this CsvOutputFormatter formatter, object? data, string path, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(path);

        return File.WriteAllTextAsync(path, formatter.Format(data), _utf8NoBom, cancellationToken);
    }
}
