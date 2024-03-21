using BenchmarkDotNet.Attributes;
using BinanceP2pMonitor.Utilities;

namespace BinanceP2pMonitor.Benchmarks;

/// <summary>
/// Extension methods for <see cref="PriceCalculatorBenchmarks"/> that provide benchmark-specific calculations
/// and formatting operations for price analysis scenarios.
/// </summary>
public static class PriceCalculatorBenchmarksExtensions
{
	/// <summary>
	/// Calculates the percentage change between the original spread and a moving average.
	/// </summary>
	/// <param name="benchmarks">The benchmark instance.</param>
	/// <param name="period">The moving average period to calculate.</param>
	/// <param name="sampleSize">The sample size to use for the moving average calculation.</param>
	/// <returns>The percentage change between the original spread and the moving average.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when the period/sampleSize combination is not supported.</exception>
	public static decimal CalculateMovingAveragePercentageChange(this PriceCalculatorBenchmarks benchmarks, int period, int sampleSize)
	{
		ArgumentNullException.ThrowIfNull(benchmarks);

		decimal movingAverage = benchmarks.GetMovingAverage(period, sampleSize);
		decimal originalSpread = benchmarks.CalculateSpread();
		return PriceCalculator.CalculatePercentageChange(originalSpread, movingAverage);
	}

	/// <summary>
	/// Calculates the spread between a buy moving average and a sell moving average.
	/// </summary>
	/// <param name="benchmarks">The benchmark instance.</param>
	/// <param name="period">The moving average period to calculate.</param>
	/// <param name="sampleSize">The sample size to use for the moving average calculation.</param>
	/// <returns>The calculated spread between buy and sell moving averages.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when the period/sampleSize combination is not supported.</exception>
	public static decimal CalculateSpreadWithMovingAverage(this PriceCalculatorBenchmarks benchmarks, int period, int sampleSize)
	{
		ArgumentNullException.ThrowIfNull(benchmarks);

		decimal buyMovingAverage = benchmarks.GetMovingAverage(period, sampleSize);
		decimal sellMovingAverage = buyMovingAverage * 1.02m; // Simulated sell price with 2% spread
		return PriceCalculator.CalculateSpread(buyMovingAverage, sellMovingAverage);
	}

	/// <summary>
	/// Formats a price with its moving average for display purposes.
	/// </summary>
	/// <param name="benchmarks">The benchmark instance.</param>
	/// <param name="period">The moving average period to calculate.</param>
	/// <param name="sampleSize">The sample size to use for the moving average calculation.</param>
	/// <returns>A formatted string containing the price and its moving average.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when the period/sampleSize combination is not supported.</exception>
	public static string FormatPriceWithMovingAverage(this PriceCalculatorBenchmarks benchmarks, int period, int sampleSize)
	{
		ArgumentNullException.ThrowIfNull(benchmarks);

		decimal movingAverage = benchmarks.GetMovingAverage(period, sampleSize);
		return $"{benchmarks.FormatPrice_WithSymbol()} | MA({period})={movingAverage:F4}";
	}

	/// <summary>
	/// Calculates statistics (mean and standard deviation) for spread values.
	/// </summary>
	/// <param name="benchmarks">The benchmark instance.</param>
	/// <param name="sampleCount">The number of spread samples to generate and analyze.</param>
	/// <returns>A tuple containing the mean spread and standard deviation.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="sampleCount"/> is less than 1.</exception>
	public static (decimal mean, decimal stdDev) CalculateSpreadStatistics(this PriceCalculatorBenchmarks benchmarks, int sampleCount = 100)
	{
		ArgumentNullException.ThrowIfNull(benchmarks);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleCount);

		List<decimal> spreads = new(sampleCount);
		for (int i = 0; i < sampleCount; i++)
		{
			spreads.Add(benchmarks.CalculateSpread());
		}

		decimal mean = spreads.Average();
		decimal variance = spreads.Select(s => (s - mean) * (s - mean)).Average();
		decimal stdDev = (decimal)Math.Sqrt((double)variance);
		return (mean, stdDev);
	}

	private static decimal GetMovingAverage(this PriceCalculatorBenchmarks benchmarks, int period, int sampleSize)
	{
		return (period, sampleSize) switch
		{
			(20, 1000) => benchmarks.MovingAverage_Period20_N1000(),
			(20, 50) => benchmarks.MovingAverage_Period20_N50(),
			(200, 1000) => benchmarks.MovingAverage_Period200_N1000(),
			_ => throw new ArgumentException($"Unsupported period/sampleSize combination: period={period}, sampleSize={sampleSize}")
		};
	}
}
