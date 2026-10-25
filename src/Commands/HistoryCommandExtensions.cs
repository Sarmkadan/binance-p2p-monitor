#nullable enable

using BinanceP2pMonitor.Formatters;

namespace BinanceP2pMonitor.Commands;

/// <summary>
/// Extension methods for HistoryCommand providing additional helpers for format lookup, day parsing and output titles
/// </summary>
public static class HistoryCommandExtensions
{
    private static readonly string[] SupportedFormats = { "table", "json", "csv", "markdown" };

    private const string DefaultDays = "7";
    private const int HoursPerDay = 24;

    /// <summary>
    /// Gets the output formats accepted by the history command
    /// </summary>
    /// <param name="command">The HistoryCommand instance</param>
    /// <returns>Read-only list of supported format names</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static IReadOnlyList<string> GetSupportedFormats(this HistoryCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        return SupportedFormats;
    }

    /// <summary>
    /// Checks whether a format name is accepted by the history command (case-insensitive)
    /// </summary>
    /// <param name="command">The HistoryCommand instance</param>
    /// <param name="format">The format name to check</param>
    /// <returns><c>true</c> if the format is supported; otherwise, <c>false</c></returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static bool IsSupportedFormat(this HistoryCommand command, string? format)
    {
        ArgumentNullException.ThrowIfNull(command);

        return !string.IsNullOrWhiteSpace(format)
            && SupportedFormats.Contains(format, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Finds the output formatter whose <see cref="IOutputFormatter.FormatType"/> matches the requested format
    /// (case-insensitive), the same way HistoryCommand selects its formatter
    /// </summary>
    /// <param name="command">The HistoryCommand instance</param>
    /// <param name="formatters">Available output formatters</param>
    /// <param name="format">The requested format name</param>
    /// <returns>The matching formatter, or <c>null</c> if none matches</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> or <paramref name="formatters"/> is null</exception>
    public static IOutputFormatter? FindFormatter(this HistoryCommand command, IEnumerable<IOutputFormatter> formatters, string? format)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(formatters);

        return formatters.FirstOrDefault(f => f.FormatType.Equals(format, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Splits the help text of HistoryCommand into individual lines, without trailing blank lines
    /// </summary>
    /// <param name="command">The HistoryCommand instance</param>
    /// <returns>Read-only list of help text lines</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static IReadOnlyList<string> GetHelpLines(this HistoryCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var lines = command.GetHelp()
            .Split('\n')
            .Select(line => line.TrimEnd('\r'))
            .ToList();

        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1]))
        {
            lines.RemoveAt(lines.Count - 1);
        }

        return lines;
    }

    /// <summary>
    /// Returns a one-line summary of the history command in the form "name: description"
    /// </summary>
    /// <param name="command">The HistoryCommand instance</param>
    /// <returns>Formatted summary line</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static string GetSummaryLine(this HistoryCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        return $"{command.Name}: {command.Description}";
    }

    /// <summary>
    /// Attempts to read the <c>--days</c> option the same way HistoryCommand does
    /// (a positive integer, defaulting to 7 when absent)
    /// </summary>
    /// <param name="command">The HistoryCommand instance</param>
    /// <param name="context">The command context containing the options</param>
    /// <param name="days">The parsed number of days when the method returns <c>true</c>; otherwise 0</param>
    /// <returns><c>true</c> if the value parses to a positive integer; otherwise, <c>false</c></returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> or <paramref name="context"/> is null</exception>
    public static bool TryGetDays(this HistoryCommand command, CommandContext context, out int days)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(context);

        if (int.TryParse(context.GetOption("days", DefaultDays), out days) && days > 0)
        {
            return true;
        }

        days = 0;
        return false;
    }

    /// <summary>
    /// Converts a number of days into the number of hourly records requested from the history service
    /// </summary>
    /// <param name="command">The HistoryCommand instance</param>
    /// <param name="days">Number of days; must be positive</param>
    /// <returns>The number of hours covering <paramref name="days"/> days</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="days"/> is not positive</exception>
    public static int GetHoursForDays(this HistoryCommand command, int days)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(days);

        return days * HoursPerDay;
    }

    /// <summary>
    /// Builds the section title HistoryCommand shows above the price data table
    /// </summary>
    /// <param name="command">The HistoryCommand instance</param>
    /// <param name="asset">Asset code, e.g. BTC</param>
    /// <param name="fiat">Fiat code, e.g. USDT</param>
    /// <param name="days">Number of days covered</param>
    /// <returns>Title in the form "Price Data (ASSET/FIAT) - Last N Days"</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static string GetDataSectionTitle(this HistoryCommand command, string asset, string fiat, int days)
    {
        ArgumentNullException.ThrowIfNull(command);

        return $"Price Data ({asset}/{fiat}) - Last {days} Days";
    }

    /// <summary>
    /// Builds the section title HistoryCommand shows above the statistical analysis
    /// </summary>
    /// <param name="command">The HistoryCommand instance</param>
    /// <param name="asset">Asset code, e.g. BTC</param>
    /// <param name="fiat">Fiat code, e.g. USDT</param>
    /// <param name="days">Number of days covered</param>
    /// <returns>Title in the form "Statistical Analysis (ASSET/FIAT, last N days)"</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static string GetStatsSectionTitle(this HistoryCommand command, string asset, string fiat, int days)
    {
        ArgumentNullException.ThrowIfNull(command);

        return $"Statistical Analysis ({asset}/{fiat}, last {days} days)";
    }
}
