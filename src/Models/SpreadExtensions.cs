namespace BinanceP2pMonitor.Models;

/// <summary>
/// Provides extension methods for <see cref="Spread"/>.
/// </summary>
public static class SpreadExtensions
{
    /// <summary>
    /// Determines whether the spread is within a normal range.
    /// A spread is considered normal if its current value is within one standard deviation of the average spread.
    /// </summary>
    /// <param name="spread">The spread to check.</param>
    /// <returns>True if the spread is normal; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="spread"/> is null.</exception>
    public static bool IsWithinNormalRange(this Spread spread)
    {
        ArgumentNullException.ThrowIfNull(spread);

        var average = spread.AverageSpreadPercent;
        var stdDev = spread.StandardDeviation;
        var lowerBound = average - stdDev;
        var upperBound = average + stdDev;

        return spread.CurrentSpreadPercent >= lowerBound && spread.CurrentSpreadPercent <= upperBound;
    }

    /// <summary>
    /// Calculates the percentage change in spread from the previous update.
    /// </summary>
    /// <param name="spread">The spread to calculate the change for.</param>
    /// <param name="previousSpread">The previous spread value.</param>
    /// <returns>The percentage change in spread.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="spread"/> or <paramref name="previousSpread"/> is null.</exception>
    public static decimal CalculatePercentageChange(this Spread spread, Spread previousSpread)
    {
        ArgumentNullException.ThrowIfNull(spread);
        ArgumentNullException.ThrowIfNull(previousSpread);

        if (previousSpread.CurrentSpreadPercent == 0)
        {
            return 0;
        }

        return ((spread.CurrentSpreadPercent - previousSpread.CurrentSpreadPercent) / previousSpread.CurrentSpreadPercent) * 100;
    }

    /// <summary>
    /// Gets a string representation of the spread's risk level, including its current value and percentile rank.
    /// </summary>
    /// <param name="spread">The spread to get the string representation for.</param>
    /// <returns>A string representation of the spread's risk level.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="spread"/> is null.</exception>
    public static string GetRiskLevelWithDetails(this Spread spread)
    {
        ArgumentNullException.ThrowIfNull(spread);

        return $"{spread.GetRiskLevel()} (Current: {spread.CurrentSpreadPercent}%, Percentile Rank: {spread.PercentileRank})";
    }
}
