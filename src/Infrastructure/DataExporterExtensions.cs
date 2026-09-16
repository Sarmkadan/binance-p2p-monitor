#nullable enable

namespace BinanceP2pMonitor.Infrastructure;

/// <summary>
/// Extension methods for <see cref="DataExporter"/> providing convenience overloads for common export scenarios.
/// </summary>
public static class DataExporterExtensions
{
    /// <summary>
    /// Exports data to a JSON file within the specified directory, combining the directory and file name into a full path.
    /// </summary>
    /// <typeparam name="T">The type of data to export.</typeparam>
    /// <param name="exporter">The <see cref="DataExporter"/> instance.</param>
    /// <param name="directoryPath">The directory in which to write the file.</param>
    /// <param name="fileName">The name of the file to write.</param>
    /// <param name="data">The data to export.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exporter"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="directoryPath"/> or <paramref name="fileName"/> is <see langword="null"/> or whitespace.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
    public static Task ExportJsonAsync<T>(this DataExporter exporter, string directoryPath, string fileName, T data, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(exporter);
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        return exporter.ExportJsonAsync(Path.Combine(directoryPath, fileName), data, ct);
    }

    /// <summary>
    /// Exports rows to a CSV file within the specified directory, combining the directory and file name into a full path.
    /// </summary>
    /// <param name="exporter">The <see cref="DataExporter"/> instance.</param>
    /// <param name="directoryPath">The directory in which to write the file.</param>
    /// <param name="fileName">The name of the file to write.</param>
    /// <param name="rows">The rows to export.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exporter"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="directoryPath"/> or <paramref name="fileName"/> is <see langword="null"/> or whitespace.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="rows"/> is <see langword="null"/>.</exception>
    public static Task ExportCsvAsync(this DataExporter exporter, string directoryPath, string fileName, IEnumerable<Dictionary<string, string>> rows, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(exporter);
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        return exporter.ExportCsvAsync(Path.Combine(directoryPath, fileName), rows, ct);
    }

    /// <summary>
    /// Exports rows to a gzip-compressed CSV file within the specified directory, combining the directory and file name into a full path.
    /// </summary>
    /// <param name="exporter">The <see cref="DataExporter"/> instance.</param>
    /// <param name="directoryPath">The directory in which to write the file.</param>
    /// <param name="fileName">The name of the file to write.</param>
    /// <param name="rows">The rows to export.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exporter"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="directoryPath"/> or <paramref name="fileName"/> is <see langword="null"/> or whitespace.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="rows"/> is <see langword="null"/>.</exception>
    public static Task ExportCsvGzAsync(this DataExporter exporter, string directoryPath, string fileName, IEnumerable<Dictionary<string, string>> rows, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(exporter);
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        return exporter.ExportCsvGzAsync(Path.Combine(directoryPath, fileName), rows, ct);
    }

    /// <summary>
    /// Exports data to a gzip-compressed JSON file within the specified directory, combining the directory and file name into a full path.
    /// </summary>
    /// <typeparam name="T">The type of data to export.</typeparam>
    /// <param name="exporter">The <see cref="DataExporter"/> instance.</param>
    /// <param name="directoryPath">The directory in which to write the file.</param>
    /// <param name="fileName">The name of the file to write.</param>
    /// <param name="data">The data to export.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exporter"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="directoryPath"/> or <paramref name="fileName"/> is <see langword="null"/> or whitespace.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
    public static Task ExportJsonGzAsync<T>(this DataExporter exporter, string directoryPath, string fileName, T data, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(exporter);
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        return exporter.ExportJsonGzAsync(Path.Combine(directoryPath, fileName), data, ct);
    }

    /// <summary>
    /// Generates an export summary for the specified record count and export type.
    /// </summary>
    /// <param name="exporter">The <see cref="DataExporter"/> instance.</param>
    /// <param name="recordCount">The number of records that were exported.</param>
    /// <param name="exportType">The type of export that was performed.</param>
    /// <returns>A formatted summary string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exporter"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="exportType"/> is <see langword="null"/> or whitespace.</exception>
    public static string GenerateSummary(this DataExporter exporter, int recordCount, string exportType)
    {
        ArgumentNullException.ThrowIfNull(exporter);
        return exporter.GenerateSummary(recordCount, exportType);
    }
}