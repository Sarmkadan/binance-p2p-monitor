// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Services;

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
}
