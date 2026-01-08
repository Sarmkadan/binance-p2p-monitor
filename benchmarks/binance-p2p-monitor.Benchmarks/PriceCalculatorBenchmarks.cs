// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BinanceP2pMonitor.Utilities;

namespace BinanceP2pMonitor.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class PriceCalculatorBenchmarks
{
    private decimal[] _prices1000 = null!;
    private decimal[] _prices50 = null!;

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

    [Benchmark(Description = "CalculateSpread (buy/sell pair)")]
    public decimal CalculateSpread()
        => PriceCalculator.CalculateSpread(30_000m, 30_450m);

    [Benchmark(Description = "CalculatePercentageChange")]
    public decimal PercentageChange()
        => PriceCalculator.CalculatePercentageChange(30_000m, 30_450m);

    [Benchmark(Description = "CalculateMovingAverage (n=1000, period=20)")]
    public decimal MovingAverage_Period20_N1000()
        => PriceCalculator.CalculateMovingAverage(_prices1000, 20);

    [Benchmark(Description = "CalculateMovingAverage (n=1000, period=200)")]
    public decimal MovingAverage_Period200_N1000()
        => PriceCalculator.CalculateMovingAverage(_prices1000, 200);

    [Benchmark(Description = "CalculateMovingAverage (n=50, period=20)")]
    public decimal MovingAverage_Period20_N50()
        => PriceCalculator.CalculateMovingAverage(_prices50, 20);

    [Benchmark(Description = "CalculateStandardDeviation (n=1000)")]
    public decimal StandardDeviation_N1000()
        => PriceCalculator.CalculateStandardDeviation(_prices1000);

    [Benchmark(Description = "CalculateStandardDeviation (n=50)")]
    public decimal StandardDeviation_N50()
        => PriceCalculator.CalculateStandardDeviation(_prices50);

    [Benchmark(Description = "FormatPrice (no symbol)")]
    public string FormatPrice_NoSymbol()
        => PriceCalculator.FormatPrice(30_450.5678m);

    [Benchmark(Description = "FormatPrice (with symbol)")]
    public string FormatPrice_WithSymbol()
        => PriceCalculator.FormatPrice(30_450.5678m, "$", 4);
}
