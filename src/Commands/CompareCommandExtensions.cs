#nullable enable

using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Commands;

/// <summary>
/// Extension methods for CompareCommand providing additional helpers for price comparison
/// </summary>
public static class CompareCommandExtensions
{
    private static readonly string[] SupportedFormats = { "table", "json", "markdown" };

    /// <summary>
    /// Gets the output formats accepted by the compare command
    /// </summary>
    /// <param name="command">The CompareCommand instance</param>
    /// <returns>Read-only list of supported format names</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static IReadOnlyList<string> GetSupportedFormats(this CompareCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        return SupportedFormats;
    }

    /// <summary>
    /// Checks whether a format name is accepted by the compare command (case-insensitive)
    /// </summary>
    /// <param name="command">The CompareCommand instance</param>
    /// <param name="format">The format name to check</param>
    /// <returns><c>true</c> if the format is supported; otherwise, <c>false</c></returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static bool IsSupportedFormat(this CompareCommand command, string? format)
    {
        ArgumentNullException.ThrowIfNull(command);

        return !string.IsNullOrWhiteSpace(format)
            && SupportedFormats.Contains(format, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks that two fiat codes are both provided and refer to different currencies
    /// </summary>
    /// <param name="command">The CompareCommand instance</param>
    /// <param name="fromFiat">First fiat currency code</param>
    /// <param name="toFiat">Second fiat currency code</param>
    /// <returns><c>true</c> if both codes are set and differ (case-insensitive); otherwise, <c>false</c></returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static bool IsDistinctFiatPair(this CompareCommand command, string? fromFiat, string? toFiat)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(fromFiat) || string.IsNullOrWhiteSpace(toFiat))
        {
            return false;
        }

        return !fromFiat.Trim().Equals(toFiat.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Calculates the percentage difference of <paramref name="price1"/> relative to <paramref name="price2"/>
    /// </summary>
    /// <param name="command">The CompareCommand instance</param>
    /// <param name="price1">The price being compared</param>
    /// <param name="price2">The reference price</param>
    /// <returns>The percentage difference, or 0 when <paramref name="price2"/> is zero</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static decimal CalculateDifferencePercent(this CompareCommand command, decimal price1, decimal price2)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (price2 == 0)
        {
            return 0;
        }

        return ((price1 - price2) / price2) * 100;
    }

    /// <summary>
    /// Calculates a ratio between two values, returning 0 instead of throwing when the denominator is zero
    /// </summary>
    /// <param name="command">The CompareCommand instance</param>
    /// <param name="numerator">The numerator</param>
    /// <param name="denominator">The denominator</param>
    /// <returns>The ratio, or 0 when <paramref name="denominator"/> is zero</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static decimal CalculateSafeRatio(this CompareCommand command, decimal numerator, decimal denominator)
    {
        ArgumentNullException.ThrowIfNull(command);

        return denominator == 0 ? 0 : numerator / denominator;
    }

    /// <summary>
    /// Determines which fiat currency offers the cheaper buy price
    /// </summary>
    /// <param name="command">The CompareCommand instance</param>
    /// <param name="priceFrom">Price quote for the first fiat currency</param>
    /// <param name="priceTo">Price quote for the second fiat currency</param>
    /// <returns>The fiat code with the lower buy price; the second currency on a tie</returns>
    /// <exception cref="ArgumentNullException">Thrown if any argument is null</exception>
    public static string GetBestBuyFiat(this CompareCommand command, Price priceFrom, Price priceTo)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(priceFrom);
        ArgumentNullException.ThrowIfNull(priceTo);

        return priceFrom.BuyPrice < priceTo.BuyPrice ? priceFrom.Fiat : priceTo.Fiat;
    }

    /// <summary>
    /// Determines which fiat currency offers the higher sell price
    /// </summary>
    /// <param name="command">The CompareCommand instance</param>
    /// <param name="priceFrom">Price quote for the first fiat currency</param>
    /// <param name="priceTo">Price quote for the second fiat currency</param>
    /// <returns>The fiat code with the higher sell price; the second currency on a tie</returns>
    /// <exception cref="ArgumentNullException">Thrown if any argument is null</exception>
    public static string GetBestSellFiat(this CompareCommand command, Price priceFrom, Price priceTo)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(priceFrom);
        ArgumentNullException.ThrowIfNull(priceTo);

        return priceFrom.SellPrice > priceTo.SellPrice ? priceFrom.Fiat : priceTo.Fiat;
    }
}
