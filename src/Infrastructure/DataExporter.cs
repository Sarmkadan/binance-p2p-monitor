#nullable enable
namespace BinanceP2pMonitor.Infrastructure;

/// <summary>
/// Exports data in various formats
/// </summary>
public class DataExporter
{
    private readonly ILogger<DataExporter> _logger;

    public DataExporter(ILogger<DataExporter> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Exports data to JSON file
    /// </summary>
    public async Task ExportJsonAsync<T>(string filePath, T data, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Exporting data to JSON: {FilePath}", filePath);
            var json = JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json, ct).ConfigureAwait(false);
            _logger.LogInformation("Exported to {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export JSON");
            throw;
        }
    }

    /// <summary>
    /// Exports to CSV format
    /// </summary>
    public async Task ExportCsvAsync(string filePath, IEnumerable<Dictionary<string, string>> rows, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Exporting data to CSV: {FilePath}", filePath);

            var rowList = rows.ToList();
            if (!rowList.Any())
            {
                await File.WriteAllTextAsync(filePath, string.Empty, ct).ConfigureAwait(false);
                return;
            }

            var headers = rowList.First().Keys.ToList();
            var csv = new System.Text.StringBuilder();

            csv.AppendLine(string.Join(",", headers.Select(EscapeCsv)));

            foreach (var row in rowList)
            {
                var values = headers.Select(h => row.TryGetValue(h, out var value) ? EscapeCsv(value) : string.Empty);
                csv.AppendLine(string.Join(",", values));
            }

            await File.WriteAllTextAsync(filePath, csv.ToString(), ct).ConfigureAwait(false);
            _logger.LogInformation("Exported {Count} rows to {FilePath}", rowList.Count, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export CSV");
            throw;
        }
    }

    /// <summary>
    /// Generates an export summary
    /// </summary>
    public string GenerateSummary(int recordCount, string exportType)
    {
        var report = new System.Text.StringBuilder();
        report.AppendLine($"Export Summary: {exportType}");
        report.AppendLine($"Records: {recordCount}");
        report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        return report.ToString();
    }

    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "\"\"";

        if (value.Contains("\"") || value.Contains(",") || value.Contains("\n"))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }
}
