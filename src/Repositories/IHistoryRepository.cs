#nullable enable
using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Repositories;

/// <summary>
/// Repository interface for PriceHistory entity operations
/// </summary>
public interface IHistoryRepository
{
    Task<PriceHistory?> GetByIdAsync(int id);
    Task<IEnumerable<PriceHistory>> GetHistoryByAssetAndFiatAsync(string asset, string fiat, int hours = 24);
    Task<IEnumerable<PriceHistory>> GetRecentHistoryAsync(int minutes = 60);
    Task<IEnumerable<PriceHistory>> GetHistoryByDateRangeAsync(string asset, string fiat, DateTime from, DateTime to);
    Task<int> AddAsync(PriceHistory history);
    Task<bool> DeleteOldRecordsAsync(int daysOld);
    Task<long> GetTotalHistoryCountAsync();
    Task<decimal> GetHighestPriceAsync(string asset, string fiat, int hours);
    Task<decimal> GetLowestPriceAsync(string asset, string fiat, int hours);
}
