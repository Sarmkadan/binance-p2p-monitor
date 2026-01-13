#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Repositories;

/// <summary>
/// Repository interface for TradeOffer entity operations
/// </summary>
public interface ITradeOfferRepository
{
    Task<TradeOffer?> GetByIdAsync(int id);
    Task<TradeOffer?> GetByBinanceIdAsync(string binanceId);
    Task<IEnumerable<TradeOffer>> GetAllActiveAsync();
    Task<IEnumerable<TradeOffer>> GetByAssetAndFiatAsync(string asset, string fiat);
    Task<IEnumerable<TradeOffer>> GetByTradeTypeAsync(int tradeType);
    Task<IEnumerable<TradeOffer>> GetBestOffersAsync(string asset, string fiat, int limit = 10);
    Task<int> AddAsync(TradeOffer offer);
    Task<bool> UpdateAsync(TradeOffer offer);
    Task<bool> DeleteAsync(int id);
    Task<long> GetTotalOffersCountAsync(string asset, string fiat);
    Task<decimal> GetAveragePriceAsync(string asset, string fiat);
}
