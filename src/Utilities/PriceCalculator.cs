// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Utilities;

/// <summary>
/// Utility class for price calculations and conversions
/// </summary>
public static class PriceCalculator
{
    // Pre-computed format strings avoid per-call string allocation for common decimal places.
    private static readonly string[] _fixedFormats =
        Enumerable.Range(0, 10).Select(i => $"F{i}").ToArray();

    /// <summary>
    /// Calculates percentage change between two prices
    /// </summary>
    public static decimal CalculatePercentageChange(decimal originalPrice, decimal newPrice)
    {
        if (originalPrice == 0)
            return 0;

        return ((newPrice - originalPrice) / originalPrice) * 100;
    }

    /// <summary>
    /// Calculates the spread percentage between buy and sell prices
    /// </summary>
    public static decimal CalculateSpread(decimal buyPrice, decimal sellPrice)
    {
        if (buyPrice == 0)
            return 0;

        return ((sellPrice - buyPrice) / buyPrice) * 100;
    }

    /// <summary>
    /// Calculates the mid-price (average of buy and sell)
    /// </summary>
    public static decimal CalculateMidPrice(decimal buyPrice, decimal sellPrice)
    {
        return (buyPrice + sellPrice) / 2;
    }

    /// <summary>
    /// Determines if a price is above threshold
    /// </summary>
    public static bool IsAboveThreshold(decimal price, decimal threshold)
    {
        return price > threshold;
    }

    /// <summary>
    /// Determines if a price is below threshold
    /// </summary>
    public static bool IsBelowThreshold(decimal price, decimal threshold)
    {
        return price < threshold;
    }

    /// <summary>
    /// Rounds price to a specified number of decimal places
    /// </summary>
    public static decimal RoundPrice(decimal price, int decimalPlaces = 8)
    {
        return Math.Round(price, decimalPlaces, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Formats price for display with currency symbol
    /// </summary>
    public static string FormatPrice(decimal price, string? currencySymbol = null, int decimalPlaces = 2)
    {
        var format = (uint)decimalPlaces < (uint)_fixedFormats.Length
            ? _fixedFormats[decimalPlaces]
            : $"F{decimalPlaces}";

        var formatted = price.ToString(format);
        return string.IsNullOrWhiteSpace(currencySymbol)
            ? formatted
            : string.Concat(currencySymbol, formatted);
    }

    /// <summary>
    /// Calculates moving average of prices over the most recent <paramref name="period"/> samples.
    /// Uses direct index arithmetic to avoid LINQ iterator overhead.
    /// </summary>
    public static decimal CalculateMovingAverage(IEnumerable<decimal> prices, int period)
    {
        if (prices == null)
            throw new ArgumentNullException(nameof(prices), "Prices collection cannot be null");

        if (period <= 0)
            throw new ArgumentOutOfRangeException(nameof(period), period, "Period must be greater than zero");

        var list = prices is IReadOnlyList<decimal> r ? r : prices.ToList();
        return MovingAverageCore(list, period);
    }

    private static decimal MovingAverageCore(IReadOnlyList<decimal> list, int period)
    {
        int count = list.Count;
        if (count == 0)
            return 0;

        int effectivePeriod = count < period ? count : period;
        int start = count - effectivePeriod;

        decimal sum = 0;
        for (int i = start; i < count; i++)
            sum += list[i];

        return sum / effectivePeriod;
    }

    /// <summary>
    /// Calculates standard deviation of prices using two-pass loop arithmetic
    /// to avoid multiple LINQ enumerations.
    /// </summary>
    public static decimal CalculateStandardDeviation(IEnumerable<decimal> prices)
    {
        if (prices == null)
            throw new ArgumentNullException(nameof(prices), "Prices collection cannot be null");

        var list = prices is IReadOnlyList<decimal> r ? r : prices.ToList();
        int count = list.Count;

        if (count < 2)
            return 0;

        decimal sum = 0;
        for (int i = 0; i < count; i++)
            sum += list[i];
        decimal mean = sum / count;

        decimal variance = 0;
        for (int i = 0; i < count; i++)
        {
            decimal diff = list[i] - mean;
            variance += diff * diff;
        }

        return (decimal)Math.Sqrt((double)(variance / count));
    }
}
