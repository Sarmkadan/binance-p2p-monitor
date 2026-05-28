#nullable enable
namespace BinanceP2pMonitor.Formatters;

/// <summary>
/// Formats output as CSV
/// </summary>
public class CsvOutputFormatter : IOutputFormatter
{
    public string FormatType => "csv";

    public string Format(object? data)
    {
        if (data is null)
            return string.Empty;

        return Format(new[] { data });
    }

    public string Format(IEnumerable<object> data)
    {
        var list = data.ToList();
        if (!list.Any())
            return string.Empty;

        var headers = GetPropertyNames(list.First());
        return Format(list, headers);
    }

    public string Format(IEnumerable<object> data, IEnumerable<string> headers)
    {
        var list = data.ToList();
        var headerList = headers.ToList();

        var sb = new System.Text.StringBuilder();

        // Write header row
        sb.AppendLine(string.Join(",", headerList.Select(EscapeCsv)));

        // Write data rows
        foreach (var item in list)
        {
            var values = new List<string>();
            var properties = item.GetType().GetProperties();

            foreach (var header in headerList)
            {
                var prop = properties.FirstOrDefault(p => p.Name == header);
                var value = prop?.GetValue(item)?.ToString() ?? string.Empty;
                values.Add(EscapeCsv(value));
            }

            sb.AppendLine(string.Join(",", values));
        }

        return sb.ToString();
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

    private List<string> GetPropertyNames(object obj)
    {
        return obj.GetType()
            .GetProperties()
            .Select(p => p.Name)
            .ToList();
    }
}
