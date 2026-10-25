#nullable enable
using System.IO;
using System.Text;

namespace BinanceP2pMonitor.Formatters;

/// <summary>
/// Provides convenience extension methods for <see cref="JsonOutputFormatter"/>.
/// </summary>
public static class JsonOutputFormatterExtensions
{
    private static readonly UTF8Encoding _utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    /// <summary>
    /// Formats a single object as single-line (non-indented) JSON, suitable for log lines and line-delimited output.
    /// </summary>
    /// <param name="formatter">The JSON formatter.</param>
    /// <param name="data">The object to format.</param>
    /// <returns>Compact JSON text, or an error object with the exception message if serialization fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/> is null.</exception>
    public static string FormatCompact(this JsonOutputFormatter formatter, object? data)
    {
        ArgumentNullException.ThrowIfNull(formatter);

        if (data is null)
            return "null";

        try
        {
            return JsonSerializer.Serialize(data);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Formats a single object as JSON and writes the result to a file, replacing any existing file.
    /// The file is written as UTF-8 without a byte order mark.
    /// </summary>
    /// <param name="formatter">The JSON formatter.</param>
    /// <param name="data">The object to format. Null produces the text <c>null</c>.</param>
    /// <param name="path">The path of the file to write.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/> or <paramref name="path"/> is null.</exception>
    /// <exception cref="IOException">Thrown when the file cannot be written.</exception>
    public static void FormatToFile(this JsonOutputFormatter formatter, object? data, string path)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(path);

        File.WriteAllText(path, formatter.Format(data), _utf8NoBom);
    }

    /// <summary>
    /// Asynchronously formats a single object as JSON and writes the result to a file, replacing any existing file.
    /// The file is written as UTF-8 without a byte order mark.
    /// </summary>
    /// <param name="formatter">The JSON formatter.</param>
    /// <param name="data">The object to format. Null produces the text <c>null</c>.</param>
    /// <param name="path">The path of the file to write.</param>
    /// <param name="cancellationToken">A token to cancel the write operation.</param>
    /// <returns>A task that completes when the file has been written.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/> or <paramref name="path"/> is null.</exception>
    /// <exception cref="IOException">Thrown when the file cannot be written.</exception>
    public static Task FormatToFileAsync(this JsonOutputFormatter formatter, object? data, string path, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(path);

        return File.WriteAllTextAsync(path, formatter.Format(data), _utf8NoBom, cancellationToken);
    }
}
