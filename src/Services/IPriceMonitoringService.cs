// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Services;

/// <summary>
/// Service interface for price monitoring operations
/// </summary>
public interface IPriceMonitoringService
{
    Task<Price?> GetCurrentPriceAsync(string asset, string fiat);
    Task<IEnumerable<Price>> GetAllCurrentPricesAsync();
    Task<bool> UpdatePriceAsync(Price price);
    Task<decimal?> GetAveragePriceAsync(string asset, string fiat, int hours);
    Task<IEnumerable<Price>> GetPricesWithSignificantChangeAsync(decimal changePercentThreshold);
    Task<Dictionary<string, decimal>> GetSpreadAnalysisAsync(string asset, string fiat);
    Task StartMonitoringAsync(CancellationToken cancellationToken);
    Task StopMonitoringAsync();
}
