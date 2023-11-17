// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Services;

/// <summary>
/// Service interface for managing price history and trend analysis
/// </summary>
public interface IPriceHistoryService
{
    Task<int> RecordPriceAsync(Price price);
    Task<IEnumerable<PriceHistory>> GetHistoryAsync(string asset, string fiat, int hours = 24);
    Task<decimal> GetPriceTrendAsync(string asset, string fiat, int hours);
    Task<(decimal High, decimal Low, decimal Average)> GetPriceStatsAsync(string asset, string fiat, int hours);
    Task<bool> CleanupOldHistoryAsync(int daysOld);
    Task<long> GetHistoryCountAsync();
    Task<Dictionary<string, object>> GetDetailedAnalysisAsync(string asset, string fiat, int hours = 24);
}
