#nullable enable
using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Repositories;

/// <summary>
/// Repository interface for Price entity operations
/// </summary>
public interface IPriceRepository
{
    Task<Price?> GetByIdAsync(int id);
    Task<Price?> GetLatestByAssetAndFiatAsync(string asset, string fiat);
    Task<IEnumerable<Price>> GetAllActiveAsync();
    Task<IEnumerable<Price>> GetByAssetAsync(string asset);
    Task<IEnumerable<Price>> GetByFiatAsync(string fiat);
    Task<int> AddAsync(Price price);
    Task<bool> UpdateAsync(Price price);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<Price>> GetPricesChangedSinceAsync(DateTime since);
    Task<decimal?> GetAveragePriceAsync(string asset, string fiat, int hours);
}
