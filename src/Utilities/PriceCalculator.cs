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
        var formatted = price.ToString($"F{decimalPlaces}");
        return string.IsNullOrWhiteSpace(currencySymbol) ? formatted : $"{currencySymbol}{formatted}";
    }

    /// <summary>
    /// Calculates moving average of prices
    /// </summary>
    public static decimal CalculateMovingAverage(IEnumerable<decimal> prices, int period)
    {
        // Fix: Add null check for collection parameter
        if (prices == null)
            throw new ArgumentNullException(nameof(prices), "Prices collection cannot be null");

        if (period <= 0)
            throw new ArgumentOutOfRangeException(nameof(period), period, "Period must be greater than zero");

        var priceList = prices.ToList();

        if (priceList.Count == 0)
            return 0;

        if (priceList.Count < period)
            return priceList.Average();

        return priceList.TakeLast(period).Average();
    }

    /// <summary>
    /// Calculates standard deviation of prices
    /// </summary>
    public static decimal CalculateStandardDeviation(IEnumerable<decimal> prices)
    {
        // Fix: Add null check for collection parameter
        if (prices == null)
            throw new ArgumentNullException(nameof(prices), "Prices collection cannot be null");

        var priceList = prices.ToList();

        if (priceList.Count < 2)
            return 0;

        var mean = priceList.Average();
        var variance = priceList.Sum(p => (p - mean) * (p - mean)) / priceList.Count;

        return (decimal)Math.Sqrt((double)variance);
    }
}
