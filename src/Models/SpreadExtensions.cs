namespace BinanceP2pMonitor.Models;

/// <summary>
/// Provides extension methods for <see cref="Spread"/> analysis.
/// </summary>
public static class SpreadExtensions
{
    /// <summary>
    /// Determines whether the spread is within a normal range.
    /// A spread is considered normal if its current value is within one standard deviation of the average spread.
    /// When standard deviation is zero, the spread is normal only if it equals the average.
    /// </summary>
    /// <param name="spread">The spread to check.</param>
    /// <returns>True if the spread is within one standard deviation of the average; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="spread"/> is null.</exception>
    public static bool IsWithinNormalRange(this Spread spread)
    {
        ArgumentNullException.ThrowIfNull(spread);

        return spread.CurrentSpreadPercent >= spread.AverageSpreadPercent - spread.StandardDeviation
               && spread.CurrentSpreadPercent <= spread.AverageSpreadPercent + spread.StandardDeviation;
    }

    /// <summary>
    /// Calculates the percentage change in spread from the previous update.
    /// The percentage change is calculated as: ((current - previous) / previous) * 100.
    /// </summary>
    /// <param name="spread">The current spread to calculate the change for.</param>
    /// <param name="previousSpread">The previous spread value.</param>
    /// <returns>The percentage change in spread from the previous value. Returns 0 when previous spread is 0 to avoid division by zero.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="spread"/> or <paramref name="previousSpread"/> is null.</exception>
    public static decimal CalculatePercentageChange(this Spread spread, Spread previousSpread)
    {
        ArgumentNullException.ThrowIfNull(spread);
        ArgumentNullException.ThrowIfNull(previousSpread);

        return previousSpread.CurrentSpreadPercent == 0
            ? 0
            : ((spread.CurrentSpreadPercent - previousSpread.CurrentSpreadPercent) / previousSpread.CurrentSpreadPercent) * 100;
    }

    /// <summary>
    /// Gets a string representation of the spread's risk level, including its current value and percentile rank.
    /// The format is: "{RiskLevel} (Current: {CurrentSpreadPercent}%, Percentile Rank: {PercentileRank})".
    /// </summary>
    /// <param name="spread">The spread to get the string representation for.</param>
    /// <returns>A formatted string containing the risk level, current spread percentage, and percentile rank.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="spread"/> is null.</exception>
    public static string GetRiskLevelWithDetails(this Spread spread)
    {
        ArgumentNullException.ThrowIfNull(spread);

        return $"{spread.GetRiskLevel()} (Current: {spread.CurrentSpreadPercent}%, Percentile Rank: {spread.PercentileRank})";
    }
}
