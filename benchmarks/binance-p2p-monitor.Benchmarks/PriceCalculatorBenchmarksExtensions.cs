using BenchmarkDotNet.Attributes;

namespace BinanceP2pMonitor.Benchmarks;

public static class PriceCalculatorBenchmarksExtensions
{
    public static decimal CalculateMovingAveragePercentageChange(this PriceCalculatorBenchmarks benchmarks, int period, int sampleSize)
    {
        decimal movingAverage = period switch
        {
            20 when sampleSize == 1000 => benchmarks.MovingAverage_Period20_N1000(),
            20 when sampleSize == 50 => benchmarks.MovingAverage_Period20_N50(),
            200 when sampleSize == 1000 => benchmarks.MovingAverage_Period200_N1000(),
            _ => throw new ArgumentException("Unsupported period/sampleSize combination")
        };
        
        decimal originalSpread = benchmarks.CalculateSpread();
        return benchmarks.PercentageChange(originalSpread, movingAverage);
    }

    public static decimal CalculateSpreadWithMovingAverage(this PriceCalculatorBenchmarks benchmarks, int period, int sampleSize)
    {
        decimal buyMovingAverage = period switch
        {
            20 when sampleSize == 1000 => benchmarks.MovingAverage_Period20_N1000(),
            20 when sampleSize == 50 => benchmarks.MovingAverage_Period20_N50(),
            200 when sampleSize == 1000 => benchmarks.MovingAverage_Period200_N1000(),
            _ => throw new ArgumentException("Unsupported period/sampleSize combination")
        };
        
        decimal sellMovingAverage = buyMovingAverage * 1.02m; // Simulated sell price with 2% spread
        return benchmarks.CalculateSpread(buyMovingAverage, sellMovingAverage);
    }

    public static string FormatPriceWithMovingAverage(this PriceCalculatorBenchmarks benchmarks, int period, int sampleSize)
    {
        decimal movingAverage = period switch
        {
            20 when sampleSize == 1000 => benchmarks.MovingAverage_Period20_N1000(),
            20 when sampleSize == 50 => benchmarks.MovingAverage_Period20_N50(),
            200 when sampleSize == 1000 => benchmarks.MovingAverage_Period200_N1000(),
            _ => throw new ArgumentException("Unsupported period/sampleSize combination")
        };
        
        return $"{benchmarks.FormatPrice_WithSymbol()} | MA({period})={movingAverage:F4}";
    }

    public static (decimal mean, decimal stdDev) CalculateSpreadStatistics(this PriceCalculatorBenchmarks benchmarks, int sampleCount = 100)
    {
        List<decimal> spreads = new List<decimal>(sampleCount);
        for (int i = 0; i < sampleCount; i++)
        {
            spreads.Add(benchmarks.CalculateSpread());
        }
        
        decimal mean = spreads.Average();
        decimal stdDev = (decimal)Math.Sqrt(spreads.Select(s => (double)(s - mean) * (s - mean)).Average());
        return (mean, stdDev);
    }
}
