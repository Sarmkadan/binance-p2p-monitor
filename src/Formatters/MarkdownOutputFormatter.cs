#nullable enable

using System.Text;
using BinanceP2pMonitor.Utilities;

namespace BinanceP2pMonitor.Formatters;

/// <summary>
/// Formats output as Markdown tables
/// </summary>
public class MarkdownOutputFormatter : IOutputFormatter
{
    public string FormatType => "markdown";

    public string Format(object? data)
    {
        if (data is null)
            return "(empty)";

        return Format(new[] { data });
    }

    public string Format(IEnumerable<object> data)
    {
        var list = data.ToList();
        if (!list.Any())
            return "(no data)";

        var headers = GetPropertyNames(list.First());
        return Format(list, headers);
    }

    public string Format(IEnumerable<object> data, IEnumerable<string> headers)
    {
        var list = data.ToList();
        var headerList = headers.ToList();

        if (!list.Any())
            return "(no data)";

        var sb = new StringBuilder();
        var rows = new List<List<string>>();

        // Build header row
        rows.Add(headerList.Cast<string>().ToList());

        // Build data rows
        foreach (var item in list)
        {
            var row = new List<string>();
            var properties = item.GetType().GetProperties();

            foreach (var header in headerList)
            {
                var prop = properties.FirstOrDefault(p => p.Name == header);
                var value = prop?.GetValue(item)?.ToString() ?? "(null)";
                row.Add(value.Truncate(50));
            }

            rows.Add(row);
        }

        // Calculate column widths
        var columnWidths = new int[headerList.Count];
        foreach (var row in rows)
        {
            for (int i = 0; i < row.Count; i++)
            {
                columnWidths[i] = Math.Max(columnWidths[i], row[i].Length);
            }
        }

        // Build markdown table
        // Header row
        var headerRow = "| ";
        for (int i = 0; i < rows[0].Count; i++)
        {
            headerRow += rows[0][i].PadRight(columnWidths[i]) + " | ";
        }
        sb.AppendLine(headerRow.TrimEnd());

        // Separator row
        var separatorRow = "|-";
        for (int i = 0; i < rows[0].Count; i++)
        {
            separatorRow += "-".PadRight(columnWidths[i] + 1, '-') + "|";
        }
        sb.AppendLine(separatorRow);

        // Data rows
        for (int i = 1; i < rows.Count; i++)
        {
            var dataRow = "| ";
            for (int j = 0; j < rows[i].Count; j++)
            {
                dataRow += rows[i][j].PadRight(columnWidths[j]) + " | ";
            }
            sb.AppendLine(dataRow.TrimEnd());
        }

        return sb.ToString();
    }

    private List<string> GetPropertyNames(object obj)
    {
        return obj.GetType()
            .GetProperties()
            .Select(p => p.Name)
            .ToList();
    }
}