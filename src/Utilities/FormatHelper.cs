// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Utilities;

/// <summary>
/// Utility class for formatting data for display and output
/// </summary>
public static class FormatHelper
{
    /// <summary>
    /// Formats a price with commas and decimal places
    /// </summary>
    public static string FormatCurrency(decimal amount, int decimalPlaces = 2)
    {
        return amount.ToString($"N{decimalPlaces}");
    }

    /// <summary>
    /// Formats a percentage with specified decimal places
    /// </summary>
    public static string FormatPercentage(decimal percentage, int decimalPlaces = 2)
    {
        return $"{percentage.ToString($"F{decimalPlaces}")}%";
    }

    /// <summary>
    /// Formats a timestamp in human-readable format
    /// </summary>
    public static string FormatTimestamp(DateTime dateTime, string format = "yyyy-MM-dd HH:mm:ss UTC")
    {
        return dateTime.ToString(format);
    }

    /// <summary>
    /// Formats time elapsed since a given date
    /// </summary>
    public static string FormatTimeAgo(DateTime dateTime)
    {
        var elapsed = DateTime.UtcNow - dateTime;

        return elapsed.TotalSeconds < 60 ? "just now"
            : elapsed.TotalMinutes < 60 ? $"{(int)elapsed.TotalMinutes}m ago"
            : elapsed.TotalHours < 24 ? $"{(int)elapsed.TotalHours}h ago"
            : $"{(int)elapsed.TotalDays}d ago";
    }

    /// <summary>
    /// Formats a large number with abbreviation (K, M, B)
    /// </summary>
    public static string FormatLargeNumber(long number)
    {
        return number switch
        {
            >= 1_000_000_000 => $"{number / 1_000_000_000.0:F1}B",
            >= 1_000_000 => $"{number / 1_000_000.0:F1}M",
            >= 1_000 => $"{number / 1_000.0:F1}K",
            _ => number.ToString()
        };
    }

    /// <summary>
    /// Formats a trading pair identifier
    /// </summary>
    public static string FormatTradingPair(string asset, string fiat)
    {
        return $"{asset.ToUpper()}/{fiat.ToUpper()}";
    }

    /// <summary>
    /// Formats alert description for display
    /// </summary>
    public static string FormatAlertDescription(string assetPair, string conditionText, decimal threshold)
    {
        return $"Alert on {assetPair}: {conditionText} {FormatPercentage(threshold)}";
    }

    /// <summary>
    /// Formats a price change indicator (with arrow and color codes for terminal)
    /// </summary>
    public static string FormatPriceChange(decimal changePercent, bool includeColorCodes = false)
    {
        var indicator = changePercent > 0 ? "↑" : changePercent < 0 ? "↓" : "→";
        var formatted = FormatPercentage(Math.Abs(changePercent), 2);

        if (!includeColorCodes)
            return $"{indicator} {formatted}";

        // ANSI color codes for terminal
        var color = changePercent > 0 ? "[32m" : changePercent < 0 ? "[31m" : "[33m";
        var reset = "[0m";

        return $"{color}{indicator} {formatted}{reset}";
    }

    /// <summary>
    /// Formats a market pair with additional data
    /// </summary>
    public static string FormatMarketInfo(string asset, string fiat, decimal price, decimal change)
    {
        return $"{FormatTradingPair(asset, fiat)}: {FormatCurrency(price)} {FormatPriceChange(change)}";
    }

    /// <summary>
    /// Breaks a long string into lines with specified max width
    /// </summary>
    public static List<string> WrapText(string text, int maxWidth = 80)
    {
        var lines = new List<string>();
        var words = text.Split(' ');
        var currentLine = string.Empty;

        foreach (var word in words)
        {
            if ((currentLine + " " + word).Length > maxWidth)
            {
                if (!string.IsNullOrEmpty(currentLine))
                    lines.Add(currentLine);

                currentLine = word;
            }
            else
            {
                currentLine += (string.IsNullOrEmpty(currentLine) ? string.Empty : " ") + word;
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
            lines.Add(currentLine);

        return lines;
    }
}
