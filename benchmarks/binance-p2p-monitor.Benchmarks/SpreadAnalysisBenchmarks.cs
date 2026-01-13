#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System.Buffers;

namespace BinanceP2pMonitor.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class SpreadAnalysisBenchmarks
{
    private decimal[] _spreadValues = null!;

    [GlobalSetup]
    public void Setup()
    {
        var rng = new Random(42);
        _spreadValues = new decimal[500];
        for (int i = 0; i < 500; i++)
            _spreadValues[i] = (decimal)(rng.NextDouble() * 5.0);
    }

    [Benchmark(Description = "AnalyzeSpread (inline arithmetic)")]
    public decimal AnalyzeSpread_Direct()
    {
        const decimal buyPrice = 30_000m;
        const decimal sellPrice = 30_450m;
        return Math.Round(((sellPrice - buyPrice) / buyPrice) * 100m, 4);
    }

    [Benchmark(Description = "ComputeSpreadStatistics (n=500, loop)")]
    public (decimal mean, decimal stdDev) ComputeStatistics_Loop()
    {
        var values = _spreadValues;
        int count = values.Length;

        decimal sum = 0;
        for (int i = 0; i < count; i++)
            sum += values[i];
        decimal mean = sum / count;

        decimal varianceSum = 0;
        for (int i = 0; i < count; i++)
        {
            decimal diff = values[i] - mean;
            varianceSum += diff * diff;
        }

        return (mean, (decimal)Math.Sqrt((double)(varianceSum / count)));
    }

    [Benchmark(Description = "FindAnomalies_ZScore (n=500, ArrayPool)")]
    public int FindAnomalies_ZScore()
    {
        var values = _spreadValues;
        int count = values.Length;

        decimal sum = 0;
        for (int i = 0; i < count; i++)
            sum += values[i];
        decimal mean = sum / count;

        decimal varianceSum = 0;
        for (int i = 0; i < count; i++)
        {
            decimal diff = values[i] - mean;
            varianceSum += diff * diff;
        }
        decimal stdDev = (decimal)Math.Sqrt((double)(varianceSum / count));

        int anomalyCount = 0;
        if (stdDev > 0)
        {
            for (int i = 0; i < count; i++)
            {
                double zScore = Math.Abs((double)((values[i] - mean) / stdDev));
                if (zScore > 2.0) anomalyCount++;
            }
        }

        return anomalyCount;
    }

    [Benchmark(Description = "FindAnomalies_ZScore (n=500, rented buffer)")]
    public int FindAnomalies_ZScore_ArrayPool()
    {
        var source = _spreadValues;
        int count = source.Length;

        var pool = ArrayPool<decimal>.Shared;
        decimal[] buffer = pool.Rent(count);
        try
        {
            source.AsSpan().CopyTo(buffer.AsSpan(0, count));
            var values = new ReadOnlySpan<decimal>(buffer, 0, count);

            decimal sum = 0;
            for (int i = 0; i < count; i++) sum += values[i];
            decimal mean = sum / count;

            decimal varianceSum = 0;
            for (int i = 0; i < count; i++)
            {
                decimal diff = values[i] - mean;
                varianceSum += diff * diff;
            }
            decimal stdDev = (decimal)Math.Sqrt((double)(varianceSum / count));

            int anomalyCount = 0;
            if (stdDev > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    double zScore = Math.Abs((double)((values[i] - mean) / stdDev));
                    if (zScore > 2.0) anomalyCount++;
                }
            }
            return anomalyCount;
        }
        finally
        {
            pool.Return(buffer);
        }
    }
}
