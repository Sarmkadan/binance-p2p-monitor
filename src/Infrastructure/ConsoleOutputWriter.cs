#nullable enable
namespace BinanceP2pMonitor.Infrastructure;

/// <summary>
/// Writes colored output to console for better readability
/// </summary>
public sealed class ConsoleOutputWriter
{
    /// <summary>
    /// Writes a cyan header surrounded by separator lines.
    /// </summary>
    /// <param name="text">The header text to write.</param>
    public void WriteHeader(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var originalForeground = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n{'='.ToString().PadRight(80, '=')}");
        Console.WriteLine(text.PadRight(80));
        Console.WriteLine($"{'='.ToString().PadRight(80, '=')}");
        Console.ForegroundColor = originalForeground;
    }

    /// <summary>
    /// Writes a success message in green.
    /// </summary>
    /// <param name="text">The success message to write.</param>
    public void WriteSuccess(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var originalForeground = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ {text}");
        Console.ForegroundColor = originalForeground;
    }

    /// <summary>
    /// Writes an error message to standard error in red.
    /// </summary>
    /// <param name="text">The error message to write.</param>
    public void WriteError(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var originalForeground = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"✗ {text}");
        Console.ForegroundColor = originalForeground;
    }

    /// <summary>
    /// Writes a warning message in yellow.
    /// </summary>
    /// <param name="text">The warning message to write.</param>
    public void WriteWarning(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var originalForeground = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"⚠ {text}");
        Console.ForegroundColor = originalForeground;
    }

    /// <summary>
    /// Writes an informational message in blue.
    /// </summary>
    /// <param name="text">The informational message to write.</param>
    public void WriteInfo(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var originalForeground = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"ℹ {text}");
        Console.ForegroundColor = originalForeground;
    }

    /// <summary>
    /// Writes a section title in magenta.
    /// </summary>
    /// <param name="title">The section title to write.</param>
    public void WriteSection(string title)
    {
        ArgumentNullException.ThrowIfNull(title);
        var originalForeground = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"\n► {title}");
        Console.ForegroundColor = originalForeground;
    }

    /// <summary>
    /// Writes a key and value with the key padded to a specified width.
    /// </summary>
    /// <param name="key">The key to write.</param>
    /// <param name="value">The value associated with the key.</param>
    /// <param name="keyWidth">The width to which the key is padded.</param>
    public void WriteKeyValue(string key, string value, int keyWidth = 20)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);
        Console.WriteLine($"{key.PadRight(keyWidth)} : {value}");
    }

    /// <summary>
    /// Writes rows of key-value pairs as a table.
    /// </summary>
    /// <param name="rows">The rows to write, with dictionary keys used as column headers.</param>
    public void WriteTable(IEnumerable<Dictionary<string, string>> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);
        if (!rows.Any())
        {
            WriteInfo("(no data)");
            return;
        }

        var columnWidths = new Dictionary<string, int>();
        var headers = rows.First().Keys.ToList();

        // Calculate column widths
        foreach (var header in headers)
            columnWidths[header] = header.Length;

        foreach (var row in rows)
        {
            foreach (var (key, value) in row)
            {
                if (columnWidths.ContainsKey(key))
                    columnWidths[key] = Math.Max(columnWidths[key], value.Length);
            }
        }

        // Write header
        var headerLine = "| " + string.Join(" | ", headers.Select(h => h.PadRight(columnWidths[h]))) + " |";
        Console.WriteLine(headerLine);
        Console.WriteLine("+-" + string.Join("-+-", columnWidths.Values.Select(w => new string('-', w))) + "-+");

        // Write rows
        foreach (var row in rows)
        {
            var line = "| " + string.Join(" | ", headers.Select(h => row[h].PadRight(columnWidths[h]))) + " |";
            Console.WriteLine(line);
        }
    }

    /// <summary>
    /// Writes a blank line.
    /// </summary>
    public void WriteBlankLine()
    {
        Console.WriteLine();
    }

    /// <summary>
    /// Writes pre-formatted text directly to stdout without any decoration or colour.
    /// </summary>
    public void WriteRaw(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        Console.WriteLine(text);
    }
}
