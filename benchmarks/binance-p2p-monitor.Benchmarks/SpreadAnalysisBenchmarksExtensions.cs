using System;
using System.Collections.Generic;
using System.Linq;

namespace BinanceP2pMonitor.Benchmarks
{
    public static class SpreadAnalysisBenchmarksExtensions
    {
        /// <summary>
        /// Computes a moving average of spread values over a specified window size.
        /// </summary>
        public static decimal ComputeMovingAverage(
            this SpreadAnalysisBenchmarks benchmarks,
            List<decimal> spreadValues,
            int windowSize = 5)
        {
            if (spreadValues == null || spreadValues.Count < windowSize)
                throw new ArgumentException("Insufficient data for moving average calculation");

            var result = new List<decimal>();
            for (int i = windowSize; i <= spreadValues.Count; i++)
            {
                var window = spreadValues.Skip(i - windowSize).Take(windowSize);
                result.Add(window.Average());
            }
            return result.Average();
        }

        /// <summary>
        /// Calculates median spread value from a collection of spread measurements.
        /// </summary>
        public static decimal CalculateSpreadMedian(
            this SpreadAnalysisBenchmarks benchmarks,
            IEnumerable<decimal> spreadValues)
        {
            if (spreadValues == null || !spreadValues.Any())
                throw new ArgumentException("No spread values provided");

            var sorted = spreadValues.OrderBy(x => x).ToList();
            int count = sorted.Count;
            if (count % 2 == 0)
            {
                return (sorted[count / 2 - 1] + sorted[count / 2]) / 2;
            }
            return sorted[count / 2];
        }

        /// <summary>
        /// Detects trend direction in spread values using simple linear regression.
        /// </summary>
        public static TrendDirection AnalyzeSpreadTrend(
            this SpreadAnalysisBenchmarks benchmarks,
            List<decimal> spreadValues,
            int sampleSize = 20)
        {
            if (spreadValues == null || spreadValues.Count < sampleSize)
                throw new ArgumentException("Insufficient data for trend analysis");

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
                > 0.0001m => TrendDirection.Up,
                < -0.0001m => TrendDirection.Down,
                _ => TrendDirection.Flat
            };
        }

        public enum TrendDirection
        {
            Up,
            Down,
            Flat
        }
    }
}
