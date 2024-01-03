#nullable enable
using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Services;

/// <summary>
/// Result of a cross-currency spread analysis between two fiat denominations of the same asset.
/// </summary>
public sealed record CrossCurrencySpread(
    string Asset,
    string BaseFiat,
    string QuoteFiat,
    decimal ConversionRate,
    decimal BuyPriceInBaseFiat,
    decimal SellPriceInBaseFiat,
    decimal SpreadPercent,
    DateTime CalculatedAt);

/// <summary>
/// Service interface for spread analysis and trading pair analysis
/// </summary>
public interface ISpreadAnalysisService
{
    /// <summary>
    /// Gets the spread analysis for the given asset and fiat pair.
    /// </summary>
    /// <param name="asset">The asset symbol.</param>
    /// <param name="fiat">The fiat currency symbol.</param>
    /// <returns>A <see cref="Spread"/> object containing analysis data, or null if not available.</returns>
    Task<Spread?> GetSpreadAnalysisAsync(string asset, string fiat);

    /// <summary>
    /// Gets the top spread opportunities.
    /// </summary>
    /// <param name="limit">The maximum number of opportunities to return (default 10).</param>
    /// <returns>A collection of <see cref="Spread"/> objects.</returns>
    Task<IEnumerable<Spread>> GetTopSpreadOpportunitiesAsync(int limit = 10);

    /// <summary>
    /// Analyzes the spread between buy and sell prices.
    /// </summary>
    /// <param name="buyPrice">The buy price.</param>
    /// <param name="sellPrice">The sell price.</param>
    /// <returns>The calculated spread percentage.</returns>
    ValueTask<decimal> AnalyzeSpreadAsync(decimal buyPrice, decimal sellPrice);

    /// <summary>
    /// Updates the spread information.
    /// </summary>
    /// <param name="spread">The <see cref="Spread"/> object to update.</param>
    /// <returns>True if the update was successful; otherwise, false.</returns>
    ValueTask<bool> UpdateSpreadAsync(Spread spread);

    /// <summary>
    /// Gets all spreads.
    /// </summary>
    /// <returns>A dictionary of spreads keyed by a unique identifier.</returns>
    Task<Dictionary<string, Spread>> GetAllSpreadsAsync();

    /// <summary>
    /// Finds anomalous spreads based on a z-score threshold.
    /// </summary>
    /// <param name="zScoreThreshold">The z-score threshold for identifying anomalies (default 2.0).</param>
    /// <returns>A collection of tuples containing asset, fiat, and spread percentage.</returns>
    Task<IEnumerable<(string Asset, string Fiat, decimal Spread)>> FindAnomalousSpreadAsync(decimal zScoreThreshold = 2.0m);

    /// <summary>
    /// Calculates the arbitrage spread between the same asset priced in two different fiat currencies.
    /// The <paramref name="conversionRate"/> converts one unit of <paramref name="quoteFiat"/> into
    /// <paramref name="baseFiat"/> (e.g. for USD/EUR use the EUR→USD rate so prices are comparable).
    /// Returns null when price data for either side is unavailable.
    /// </summary>
    /// <param name="asset">The asset symbol.</param>
    /// <param name="baseFiat">The base fiat currency.</param>
    /// <param name="quoteFiat">The quote fiat currency.</param>
    /// <param name="conversionRate">The conversion rate from quote fiat to base fiat.</param>
    /// <returns>A <see cref="CrossCurrencySpread"/> object, or null if data is missing.</returns>
    Task<CrossCurrencySpread?> GetCrossCurrencySpreadAsync(
        string asset,
        string baseFiat,
        string quoteFiat,
        decimal conversionRate);
}

