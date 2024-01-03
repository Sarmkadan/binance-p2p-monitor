#nullable enable
using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Services;

/// <summary>
/// Service interface for managing price history and trend analysis
/// </summary>
public interface IPriceHistoryService
{
    /// <summary>
    /// Records a new price in the history.
    /// </summary>
    /// <param name="price">The <see cref="Price"/> object to record.</param>
    /// <returns>The number of records added.</returns>
    Task<int> RecordPriceAsync(Price price);

    /// <summary>
    /// Gets the price history for an asset and fiat pair over a specified number of hours.
    /// </summary>
    /// <param name="asset">The asset symbol.</param>
    /// <param name="fiat">The fiat currency symbol.</param>
    /// <param name="hours">The number of hours to look back (default 24).</param>
    /// <returns>A collection of <see cref="PriceHistory"/> objects.</returns>
    Task<IEnumerable<PriceHistory>> GetHistoryAsync(string asset, string fiat, int hours = 24);

    /// <summary>
    /// Gets the price trend for an asset and fiat pair over a specified number of hours.
    /// </summary>
    /// <param name="asset">The asset symbol.</param>
    /// <param name="fiat">The fiat currency symbol.</param>
    /// <param name="hours">The number of hours to calculate the trend over.</param>
    /// <returns>The price trend as a decimal.</returns>
    Task<decimal> GetPriceTrendAsync(string asset, string fiat, int hours);

    /// <summary>
    /// Gets price statistics (High, Low, Average) for an asset and fiat pair over a specified number of hours.
    /// </summary>
    /// <param name="asset">The asset symbol.</param>
    /// <param name="fiat">The fiat currency symbol.</param>
    /// <param name="hours">The number of hours to calculate statistics over.</param>
    /// <returns>A tuple containing the High, Low, and Average price values.</returns>
    Task<(decimal High, decimal Low, decimal Average)> GetPriceStatsAsync(string asset, string fiat, int hours);

    /// <summary>
    /// Cleans up old price history records.
    /// </summary>
    /// <param name="daysOld">The number of days to retain records.</param>
    /// <returns>True if the cleanup was successful; otherwise, false.</returns>
    Task<bool> CleanupOldHistoryAsync(int daysOld);

    /// <summary>
    /// Gets the total count of history records.
    /// </summary>
    /// <returns>The total number of history records.</returns>
    Task<long> GetHistoryCountAsync();

    /// <summary>
    /// Gets a detailed price analysis for an asset and fiat pair over a specified number of hours.
    /// </summary>
    /// <param name="asset">The asset symbol.</param>
    /// <param name="fiat">The fiat currency symbol.</param>
    /// <param name="hours">The number of hours to analyze (default 24).</param>
    /// <returns>A dictionary containing detailed analysis.</returns>
    Task<Dictionary<string, object>> GetDetailedAnalysisAsync(string asset, string fiat, int hours = 24);
}
