#nullable enable
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BinanceP2pMonitor.Utilities;

namespace BinanceP2pMonitor.Benchmarks;

/// <summary>
/// Benchmark class for price calculator operations.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class PriceCalculatorBenchmarks
{
    private decimal[] _prices1000 = null!;
    private decimal[] _prices50 = null!;

    /// <summary>
    /// Initializes the benchmark data.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        var rng = new Random(42);
        _prices1000 = new decimal[1000];
        _prices50 = new decimal[50];
        for (int i = 0; i < 1000; i++)
            _prices1000[i] = (decimal)(rng.NextDouble() * 90_000 + 10_000);
        for (int i = 0; i < 50; i++)
            _prices50[i] = (decimal)(rng.NextDouble() * 90_000 + 10_000);
    }

    /// <summary>
    /// Calculates the spread between a buy and sell price.
    /// </summary>
    /// <returns>The calculated spread.</returns>
    [Benchmark(Description = "CalculateSpread (buy/sell pair)")]
    public decimal CalculateSpread()
        => PriceCalculator.CalculateSpread(30_000m, 30_450m);

    /// <summary>
    /// Calculates the percentage change between two prices.
    /// </summary>
    /// <returns>The calculated percentage change.</returns>
    [Benchmark(Description = "CalculatePercentageChange")]
    public decimal PercentageChange()
        => PriceCalculator.CalculatePercentageChange(30_000m, 30_450m);

    /// <summary>
    /// Calculates the moving average of a list of prices.
    /// </summary>
    /// <param name="prices">The list of prices.</param>
    /// <param name="period">The moving average period.</param>
    /// <returns>The calculated moving average.</returns>
    [Benchmark(Description = "CalculateMovingAverage (n=1000, period=20)")]
    public decimal MovingAverage_Period20_N1000()
        => PriceCalculator.CalculateMovingAverage(_prices1000, 20);

    /// <summary>
    /// Calculates the moving average of a list of prices.
    /// </summary>
    /// <param name="prices">The list of prices.</param>
    /// <param name="period">The moving average period.</param>
    /// <returns>The calculated moving average.</returns>
    [Benchmark(Description = "CalculateMovingAverage (n=1000, period=200)")]
    public decimal MovingAverage_Period200_N1000()
        => PriceCalculator.CalculateMovingAverage(_prices1000, 200);

    /// <summary>
    /// Calculates the moving average of a list of prices.
    /// </summary>
    /// <param name="prices">The list of prices.</param>
    /// <param name="period">The moving average period.</param>
    /// <returns>The calculated moving average.</returns>
    [Benchmark(Description = "CalculateMovingAverage (n=50, period=20)")]
    public decimal MovingAverage_Period20_N50()
        => PriceCalculator.CalculateMovingAverage(_prices50, 20);

    /// <summary>
    /// Calculates the standard deviation of a list of prices.
    /// </summary>
    /// <param name="prices">The list of prices.</param>
    /// <returns>The calculated standard deviation.</returns>
    [Benchmark(Description = "CalculateStandardDeviation (n=1000)")]
    public decimal StandardDeviation_N1000()
        => PriceCalculator.CalculateStandardDeviation(_prices1000);

    /// <summary>
    /// Calculates the standard deviation of a list of prices.
    /// </summary>
    /// <param name="prices">The list of prices.</param>
    /// <returns>The calculated standard deviation.</returns>
    [Benchmark(Description = "CalculateStandardDeviation (n=50)")]
    public decimal StandardDeviation_N50()
        => PriceCalculator.CalculateStandardDeviation(_prices50);

    /// <summary>
    /// Formats a price as a string.
    /// </summary>
    /// <param name="price">The price to format.</param>
    /// <returns>The formatted price string.</returns>
    [Benchmark(Description = "FormatPrice (no symbol)")]
    public string FormatPrice_NoSymbol()
        => PriceCalculator.FormatPrice(30_450.5678m);

    /// <summary>
    /// Formats a price as a string with a symbol.
    /// </summary>
    /// <param name="price">The price to format.</param>
    /// <param name="symbol">The symbol to use.</param>
    /// <param name="decimalPlaces">The number of decimal places to use.</param>
    /// <returns>The formatted price string.</returns>
    [Benchmark(Description = "FormatPrice (with symbol)")]
    public string FormatPrice_WithSymbol()
        => PriceCalculator.FormatPrice(30_450.5678m, "$", 4);
}
