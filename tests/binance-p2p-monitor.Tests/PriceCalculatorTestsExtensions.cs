#nullable enable

using BinanceP2pMonitor.Utilities;

namespace BinanceP2pMonitor.Tests;

public static class PriceCalculatorTestsExtensions
{
    /// <summary>
    /// Creates a sequence of prices with linear progression for testing trend calculations.
    /// </summary>
    /// <param name="startPrice">The starting price.</param>
    /// <param name="step">The amount to increase each step.</param>
    /// <param name="count">Number of prices to generate.</param>
    /// <returns>Array of prices in linear progression.</returns>
    public static decimal[] GenerateLinearPriceSequence(decimal startPrice, decimal step, int count)
    {
        var result = new decimal[count];
        var current = startPrice;
        for (int i = 0; i < count; i++)
        {
            result[i] = current;
            current += step;
        }
        return result;
    }

    /// <summary>
    /// Creates a sequence of prices with exponential progression for testing percentage calculations.
    /// </summary>
    /// <param name="startPrice">The starting price.</param>
    /// <param name="growthRate">The growth rate per step (e.g., 0.05 for 5%).</param>
    /// <param name="count">Number of prices to generate.</param>
    /// <returns>Array of prices in exponential progression.</returns>
    public static decimal[] GenerateExponentialPriceSequence(decimal startPrice, decimal growthRate, int count)
    {
        var result = new decimal[count];
        var current = startPrice;
        for (int i = 0; i < count; i++)
        {
            result[i] = current;
            current *= (1m + growthRate);
        }
        return result;
    }

    /// <summary>
    /// Calculates the cumulative percentage change over a sequence of prices.
    /// </summary>
    /// <param name="prices">Array of prices in chronological order.</param>
    /// <returns>The total cumulative percentage change from first to last price.</returns>
    public static decimal CalculateCumulativePercentageChange(this decimal[] prices)
    {
        if (prices.Length < 2)
            return 0m;

        var startPrice = prices[0];
        var endPrice = prices[prices.Length - 1];
        return PriceCalculator.CalculatePercentageChange(startPrice, endPrice);
    }

    /// <summary>
    /// Calculates the average spread percentage across multiple buy/sell price pairs.
    /// </summary>
    /// <param name="pricePairs">Array of tuples containing (buyPrice, sellPrice) pairs.</param>
    /// <returns>The average spread percentage across all pairs.</returns>
    public static decimal CalculateAverageSpread(this (decimal BuyPrice, decimal SellPrice)[] pricePairs)
    {
        if (pricePairs.Length == 0)
            return 0m;

        decimal totalSpread = 0m;
        foreach (var pair in pricePairs)
        {
            totalSpread += PriceCalculator.CalculateSpread(pair.BuyPrice, pair.SellPrice);
        }
        return totalSpread / pricePairs.Length;
    }

    /// <summary>
    /// Creates a collection of price data points for testing moving average calculations.
    /// </summary>
    /// <param name="basePrice">The base price to start from.</param>
    /// <param name="volatility">The maximum price deviation from base.</param>
    /// <param name="count">Number of data points to generate.</param>
    /// <returns>Array of prices with random fluctuations around base price.</returns>
    public static decimal[] GenerateVolatilePriceSequence(decimal basePrice, decimal volatility, int count)
    {
        var random = new Random(42);
        var result = new decimal[count];

        for (int i = 0; i < count; i++)
        {
            var deviation = (decimal)(random.NextDouble() * 2 - 1) * volatility;
            result[i] = basePrice + deviation;
        }

        return result;
    }

    /// <summary>
    /// Formats a price array as a readable string for assertion failure messages.
    /// </summary>
    /// <param name="prices">The prices to format.</param>
    /// <param name="format">Optional format string (default: "F2").</param>
    /// <returns>Formatted string representation.</returns>
    public static string FormatPriceArray(this decimal[] prices, string format = "F2")
    {
        return string.Join(", ", prices.Select(p => p.ToString(format)));
    }

    /// <summary>
    /// Asserts that a calculated value is within expected bounds for floating point comparisons.
    /// </summary>
    /// <param name="actual">The actual calculated value.</param>
    /// <param name="expected">The expected value.</param>
    /// <param name="tolerance">Allowed tolerance (default: 0.001).</param>
    /// <returns>True if within tolerance; false otherwise.</returns>
    public static bool ShouldBeWithinTolerance(this decimal actual, decimal expected, decimal tolerance = 0.001m)
    {
        var difference = Math.Abs(actual - expected);
        return difference <= tolerance;
    }

    /// <summary>
    /// Creates test data for spread calculation edge cases.
    /// </summary>
    /// <param name="count">Number of test cases to generate.</param>
    /// <returns>Array of (buyPrice, sellPrice) tuples with edge cases.</returns>
    public static (decimal BuyPrice, decimal SellPrice)[] GenerateSpreadTestCases(int count)
    {
        var result = new (decimal BuyPrice, decimal SellPrice)[count];
        var random = new Random(123);

        for (int i = 0; i < count; i++)
        {
            var buyPrice = (decimal)(random.NextDouble() * 1000);
            var spreadPercent = (decimal)(random.NextDouble() * 10);
            var sellPrice = buyPrice * (1m + spreadPercent / 100m);
            result[i] = (buyPrice, sellPrice);
        }

        return result;
    }
}