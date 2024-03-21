using System;
using System.Collections.Generic;
using System.Linq;

namespace BinanceP2pMonitor.Benchmarks
{
    /// <summary>
    /// Provides extension methods for spread analysis benchmarks.
    /// </summary>
    public static class SpreadAnalysisBenchmarksExtensions
    {
        /// <summary>
        /// Computes moving averages of spread values over a specified window size.
        /// </summary>
        /// <param name="benchmarks">The benchmarks instance.</param>
        /// <param name="spreadValues">The spread values to analyze.</param>
        /// <param name="windowSize">The size of the moving window. Must be positive.</param>
        /// <returns>An array of moving average values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spreadValues"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="windowSize"/> is not positive.</exception>
        public static decimal[] ComputeMovingAverage(
            this SpreadAnalysisBenchmarks benchmarks,
            List<decimal> spreadValues,
            int windowSize = 5)
        {
            ArgumentNullException.ThrowIfNull(spreadValues);
            if (windowSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(windowSize), "Window size must be positive");
            }
            if (windowSize > spreadValues.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(windowSize), "Window size cannot exceed spread values count");
            }

            var result = new decimal[spreadValues.Count - windowSize + 1];
            for (int i = 0; i < result.Length; i++)
            {
                var window = spreadValues.Skip(i).Take(windowSize);
                result[i] = window.Average();
            }
            return result;
        }

        /// <summary>
        /// Calculates median spread value from a collection of spread measurements.
        /// </summary>
        /// <param name="benchmarks">The benchmarks instance.</param>
        /// <param name="spreadValues">The spread values to analyze.</param>
        /// <returns>The median spread value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spreadValues"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="spreadValues"/> contains no elements.</exception>
        public static decimal CalculateSpreadMedian(
            this SpreadAnalysisBenchmarks benchmarks,
            IEnumerable<decimal> spreadValues)
        {
            ArgumentNullException.ThrowIfNull(spreadValues);

            var sorted = spreadValues.OrderBy(x => x).ToList();
            int count = sorted.Count;
            if (count == 0)
            {
                throw new ArgumentException("No spread values provided", nameof(spreadValues));
            }

            return count % 2 == 0
                ? (sorted[count / 2 - 1] + sorted[count / 2]) / 2
                : sorted[count / 2];
        }

        /// <summary>
        /// Detects trend direction in spread values using simple linear regression.
        /// </summary>
        /// <param name="benchmarks">The benchmarks instance.</param>
        /// <param name="spreadValues">The spread values to analyze.</param>
        /// <param name="sampleSize">The number of samples to use for trend analysis.</param>
        /// <returns>The detected trend direction.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spreadValues"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="spreadValues"/> has insufficient elements for the specified sample size.</exception>
        public static TrendDirection AnalyzeSpreadTrend(
            this SpreadAnalysisBenchmarks benchmarks,
            List<decimal> spreadValues,
            int sampleSize = 20)
        {
            ArgumentNullException.ThrowIfNull(spreadValues);
            if (sampleSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleSize), "Sample size must be positive");
            }
            if (spreadValues.Count < sampleSize)
            {
                throw new ArgumentException(
                    $"Insufficient data for trend analysis. Need {sampleSize} samples but only have {spreadValues.Count}.",
                    nameof(spreadValues));
            }

            var samples = spreadValues.TakeLast(sampleSize).ToList();
            int n = samples.Count;

            double xSum = 0, ySum = 0, xySum = 0, x2Sum = 0;
            for (int i = 0; i < n; i++)
            {
                double x = i + 1;
                double y = (double)samples[i];
                xSum += x;
                ySum += y;
                xySum += x * y;
                x2Sum += x * x;
            }

            double slope = (n * xySum - xSum * ySum) / (n * x2Sum - xSum * xSum);
            return slope switch
            {
                > 0.001 => TrendDirection.Up,
                < -0.001 => TrendDirection.Down,
                _ => TrendDirection.Flat
            };
        }

        /// <summary>
        /// Represents the direction of a trend.
        /// </summary>
        public enum TrendDirection
        {
            /// <summary>Indicates an upward trend.</summary>
            Up,

            /// <summary>Indicates a downward trend.</summary>
            Down,

            /// <summary>Indicates no significant trend.</summary>
            Flat
        }
    }
}
