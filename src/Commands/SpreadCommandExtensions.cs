#nullable enable

using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Commands;

/// <summary>
/// Extension methods for SpreadCommand providing additional helpers for spread display and filtering
/// </summary>
public static class SpreadCommandExtensions
{
    private static readonly string[] SupportedFormats = { "table", "json", "markdown" };

    /// <summary>
    /// Gets the output formats accepted by the spread command
    /// </summary>
    /// <param name="command">The SpreadCommand instance</param>
    /// <returns>Read-only list of supported format names</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static IReadOnlyList<string> GetSupportedFormats(this SpreadCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        return SupportedFormats;
    }

    /// <summary>
    /// Checks whether a format name is accepted by the spread command (case-insensitive)
    /// </summary>
    /// <param name="command">The SpreadCommand instance</param>
    /// <param name="format">The format name to check</param>
    /// <returns><c>true</c> if the format is supported; otherwise, <c>false</c></returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static bool IsSupportedFormat(this SpreadCommand command, string? format)
    {
        ArgumentNullException.ThrowIfNull(command);

        return !string.IsNullOrWhiteSpace(format)
            && SupportedFormats.Contains(format, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Splits the help text of SpreadCommand into individual lines, without trailing blank lines
    /// </summary>
    /// <param name="command">The SpreadCommand instance</param>
    /// <returns>Read-only list of help text lines</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static IReadOnlyList<string> GetHelpLines(this SpreadCommand command)
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
    /// Returns a one-line summary of the spread command in the form "name: description"
    /// </summary>
    /// <param name="command">The SpreadCommand instance</param>
    /// <returns>Formatted summary line</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static string GetSummaryLine(this SpreadCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        return $"{command.Name}: {command.Description}";
    }

    /// <summary>
    /// Parses a <c>--pair</c> value in the form ASSET/FIAT the same way SpreadCommand splits it
    /// (on '/' or '\', ignoring empty segments), trimming both parts
    /// </summary>
    /// <param name="command">The SpreadCommand instance</param>
    /// <param name="pair">The raw pair value, for example "BTC/USD"</param>
    /// <param name="asset">The trimmed asset code when the method returns <c>true</c>; otherwise an empty string</param>
    /// <param name="fiat">The trimmed fiat code when the method returns <c>true</c>; otherwise an empty string</param>
    /// <returns><c>true</c> if the value contains exactly two non-empty parts; otherwise, <c>false</c></returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static bool TryParsePair(this SpreadCommand command, string? pair, out string asset, out string fiat)
    {
        ArgumentNullException.ThrowIfNull(command);

        asset = string.Empty;
        fiat = string.Empty;

        if (string.IsNullOrWhiteSpace(pair))
        {
            return false;
        }

        var parts = pair.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        var trimmedAsset = parts[0].Trim();
        var trimmedFiat = parts[1].Trim();
        if (trimmedAsset.Length == 0 || trimmedFiat.Length == 0)
        {
            return false;
        }

        asset = trimmedAsset;
        fiat = trimmedFiat;
        return true;
    }

    /// <summary>
    /// Checks whether a spread matches the optional <c>--asset</c> and <c>--fiat</c> filters (case-insensitive).
    /// An empty or null filter matches everything.
    /// </summary>
    /// <param name="command">The SpreadCommand instance</param>
    /// <param name="spread">The spread to check</param>
    /// <param name="assetFilter">Asset code to match, or null/empty for any asset</param>
    /// <param name="fiatFilter">Fiat code to match, or null/empty for any fiat currency</param>
    /// <returns><c>true</c> if the spread passes both filters; otherwise, <c>false</c></returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> or <paramref name="spread"/> is null</exception>
    public static bool MatchesFilters(this SpreadCommand command, Spread spread, string? assetFilter, string? fiatFilter)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(spread);

        if (!string.IsNullOrEmpty(assetFilter) && !spread.Asset.Equals(assetFilter, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(fiatFilter) && !spread.Fiat.Equals(fiatFilter, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }
}
