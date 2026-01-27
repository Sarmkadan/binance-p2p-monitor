#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Infrastructure;

/// <summary>
/// Writes colored output to console for better readability
/// </summary>
public class ConsoleOutputWriter
{
    public void WriteHeader(string text)
    {
        var originalForeground = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n{'='.ToString().PadRight(80, '=')}");
        Console.WriteLine(text.PadRight(80));
        Console.WriteLine($"{'='.ToString().PadRight(80, '=')}");
        Console.ForegroundColor = originalForeground;
    }

    public void WriteSuccess(string text)
    {
        var originalForeground = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ {text}");
        Console.ForegroundColor = originalForeground;
    }

    public void WriteError(string text)
    {
        var originalForeground = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"✗ {text}");
        Console.ForegroundColor = originalForeground;
    }

    public void WriteWarning(string text)
    {
        var originalForeground = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"⚠ {text}");
        Console.ForegroundColor = originalForeground;
    }

    public void WriteInfo(string text)
    {
        var originalForeground = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"ℹ {text}");
        Console.ForegroundColor = originalForeground;
    }

    public void WriteSection(string title)
    {
        var originalForeground = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"\n► {title}");
        Console.ForegroundColor = originalForeground;
    }

    public void WriteKeyValue(string key, string value, int keyWidth = 20)
    {
        Console.WriteLine($"{key.PadRight(keyWidth)} : {value}");
    }

    public void WriteTable(IEnumerable<Dictionary<string, string>> rows)
    {
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

    public void WriteBlankLine()
    {
        Console.WriteLine();
    }

    /// <summary>
    /// Writes pre-formatted text directly to stdout without any decoration or colour.
    /// </summary>
    public void WriteRaw(string text)
    {
        Console.WriteLine(text);
    }
}
