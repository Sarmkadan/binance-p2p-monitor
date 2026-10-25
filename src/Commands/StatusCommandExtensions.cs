#nullable enable

using BinanceP2pMonitor.Formatters;
using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Commands;

/// <summary>
/// Extension methods for StatusCommand providing additional helpers for format lookup and asset filtering
/// </summary>
public static class StatusCommandExtensions
{
    private static readonly string[] SupportedFormats = { "table", "json", "markdown" };

    /// <summary>
    /// Gets the output formats accepted by the status command
    /// </summary>
    /// <param name="command">The StatusCommand instance</param>
    /// <returns>Read-only list of supported format names</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static IReadOnlyList<string> GetSupportedFormats(this StatusCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        return SupportedFormats;
    }

    /// <summary>
    /// Checks whether a format name is accepted by the status command (case-insensitive)
    /// </summary>
    /// <param name="command">The StatusCommand instance</param>
    /// <param name="format">The format name to check</param>
    /// <returns><c>true</c> if the format is supported; otherwise, <c>false</c></returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static bool IsSupportedFormat(this StatusCommand command, string? format)
    {
        ArgumentNullException.ThrowIfNull(command);

        return !string.IsNullOrWhiteSpace(format)
            && SupportedFormats.Contains(format, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Finds the output formatter whose <see cref="IOutputFormatter.FormatType"/> matches the requested format
    /// (case-insensitive), the same way StatusCommand selects its formatter
    /// </summary>
    /// <param name="command">The StatusCommand instance</param>
    /// <param name="formatters">Available output formatters</param>
    /// <param name="format">The requested format name</param>
    /// <returns>The matching formatter, or <c>null</c> if none matches</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> or <paramref name="formatters"/> is null</exception>
    public static IOutputFormatter? FindFormatter(this StatusCommand command, IEnumerable<IOutputFormatter> formatters, string? format)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(formatters);

        return formatters.FirstOrDefault(f => f.FormatType.Equals(format, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Splits the help text of StatusCommand into individual lines, without trailing blank lines
    /// </summary>
    /// <param name="command">The StatusCommand instance</param>
    /// <returns>Read-only list of help text lines</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static IReadOnlyList<string> GetHelpLines(this StatusCommand command)
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
    /// Returns a one-line summary of the status command in the form "name: description"
    /// </summary>
    /// <param name="command">The StatusCommand instance</param>
    /// <returns>Formatted summary line</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static string GetSummaryLine(this StatusCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        return $"{command.Name}: {command.Description}";
    }

    /// <summary>
    /// Reads the <c>--asset</c> option the same way StatusCommand does; an empty value means no filter
    /// </summary>
    /// <param name="command">The StatusCommand instance</param>
    /// <param name="context">The command context containing the options</param>
    /// <returns>The asset filter, or <c>null</c> if <c>--asset</c> is absent or empty</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> or <paramref name="context"/> is null</exception>
    public static string? GetAssetFilter(this StatusCommand command, CommandContext context)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(context);

        var assetFilter = context.GetOption("asset");
        return string.IsNullOrEmpty(assetFilter) ? null : assetFilter;
    }

    /// <summary>
    /// Checks whether a price passes the asset filter used by StatusCommand (case-insensitive).
    /// A null or empty filter matches every price.
    /// </summary>
    /// <param name="command">The StatusCommand instance</param>
    /// <param name="price">The price to check</param>
    /// <param name="assetFilter">Asset code to match, or null/empty for any asset</param>
    /// <returns><c>true</c> if the price should be displayed; otherwise, <c>false</c></returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> or <paramref name="price"/> is null</exception>
    public static bool MatchesAssetFilter(this StatusCommand command, Price price, string? assetFilter)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(price);

        return string.IsNullOrEmpty(assetFilter)
            || price.Asset.Equals(assetFilter, StringComparison.OrdinalIgnoreCase);
    }
}
