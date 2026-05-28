#nullable enable
namespace BinanceP2pMonitor.Formatters;

/// <summary>
/// Formats output as a formatted table
/// </summary>
public class TableOutputFormatter : IOutputFormatter
{
    public string FormatType => "table";

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

        var sb = new System.Text.StringBuilder();
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

        // Build table
        var separatorLine = "+" + string.Join("+", columnWidths.Select(w => new string('-', w + 2))) + "+";
        sb.AppendLine(separatorLine);

        // Header row
        var headerRow = "|";
        for (int i = 0; i < rows[0].Count; i++)
        {
            headerRow += $" {rows[0][i].PadRight(columnWidths[i])} |";
        }
        sb.AppendLine(headerRow);
        sb.AppendLine(separatorLine);

        // Data rows
        for (int i = 1; i < rows.Count; i++)
        {
            var dataRow = "|";
            for (int j = 0; j < rows[i].Count; j++)
            {
                dataRow += $" {rows[i][j].PadRight(columnWidths[j])} |";
            }
            sb.AppendLine(dataRow);
        }

        sb.AppendLine(separatorLine);

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
