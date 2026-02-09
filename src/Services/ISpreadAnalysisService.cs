#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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
    Task<Spread?> GetSpreadAnalysisAsync(string asset, string fiat);
    Task<IEnumerable<Spread>> GetTopSpreadOpportunitiesAsync(int limit = 10);
    ValueTask<decimal> AnalyzeSpreadAsync(decimal buyPrice, decimal sellPrice);
    ValueTask<bool> UpdateSpreadAsync(Spread spread);
    Task<Dictionary<string, Spread>> GetAllSpreadsAsync();
    Task<IEnumerable<(string Asset, string Fiat, decimal Spread)>> FindAnomalousSpreadAsync(decimal zScoreThreshold = 2.0m);

    /// <summary>
    /// Calculates the arbitrage spread between the same asset priced in two different fiat currencies.
    /// The <paramref name="conversionRate"/> converts one unit of <paramref name="quoteFiat"/> into
    /// <paramref name="baseFiat"/> (e.g. for USD/EUR use the EUR→USD rate so prices are comparable).
    /// Returns null when price data for either side is unavailable.
    /// </summary>
    Task<CrossCurrencySpread?> GetCrossCurrencySpreadAsync(
        string asset,
        string baseFiat,
        string quoteFiat,
        decimal conversionRate);
}
