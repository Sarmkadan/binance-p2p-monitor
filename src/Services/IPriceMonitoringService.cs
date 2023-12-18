#nullable enable
using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Services;

/// <summary>
/// Service interface for price monitoring operations
/// </summary>
public interface IPriceMonitoringService
{
    /// <summary>
    /// Gets the latest price for a given asset and fiat currency.
    /// </summary>
    /// <param name="asset">The asset symbol (e.g., BTC).</param>
    /// <param name="fiat">The fiat currency symbol (e.g., USD).</param>
    /// <returns>The <see cref="Price"/> object if found; otherwise, null.</returns>
    Task<Price?> GetCurrentPriceAsync(string asset, string fiat);

    /// <summary>
    /// Gets all current active prices.
    /// </summary>
    /// <returns>A collection of <see cref="Price"/> objects.</returns>
    Task<IEnumerable<Price>> GetAllCurrentPricesAsync();

    /// <summary>
    /// Updates the price for an asset and fiat pair.
    /// </summary>
    /// <param name="price">The <see cref="Price"/> object to update.</param>
    /// <returns>True if the price was updated successfully; otherwise, false.</returns>
    Task<bool> UpdatePriceAsync(Price price);

    /// <summary>
    /// Calculates the average price for an asset and fiat pair over the specified number of hours.
    /// </summary>
    /// <param name="asset">The asset symbol.</param>
    /// <param name="fiat">The fiat currency symbol.</param>
    /// <param name="hours">The number of hours to calculate the average over.</param>
    /// <returns>The average price as a decimal, or null if no data exists.</returns>
    Task<decimal?> GetAveragePriceAsync(string asset, string fiat, int hours);

    /// <summary>
    /// Gets prices that have changed by more than the specified threshold percentage.
    /// </summary>
    /// <param name="changePercentThreshold">The percentage threshold to identify significant changes.</param>
    /// <returns>A collection of <see cref="Price"/> objects with significant changes.</returns>
    Task<IEnumerable<Price>> GetPricesWithSignificantChangeAsync(decimal changePercentThreshold);

    /// <summary>
    /// Performs a spread analysis for the given asset and fiat pair.
    /// </summary>
    /// <param name="asset">The asset symbol.</param>
    /// <param name="fiat">The fiat currency symbol.</param>
    /// <returns>A <see cref="Spread"/> object containing analysis data, or null if not available.</returns>
    Task<Spread?> GetSpreadAnalysisAsync(string asset, string fiat);

    /// <summary>
    /// Starts the monitoring process.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the operation.</returns>
    Task StartMonitoringAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Stops the monitoring process.
    /// </summary>
    /// <returns>A task representing the operation.</returns>
    Task StopMonitoringAsync();
}
