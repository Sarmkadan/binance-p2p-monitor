#nullable enable
using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Repositories;

/// <summary>
/// Repository interface for PriceAlert entity operations
/// </summary>
public interface IAlertRepository
{
    Task<PriceAlert?> GetByIdAsync(int id);
    Task<IEnumerable<PriceAlert>> GetEnabledAlertsAsync();
    Task<IEnumerable<PriceAlert>> GetUserAlertsAsync(int userId);
    Task<IEnumerable<PriceAlert>> GetAlertsByAssetAndFiatAsync(string asset, string fiat);
    Task<int> AddAsync(PriceAlert alert);
    Task<bool> UpdateAsync(PriceAlert alert);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteUserAlertsAsync(int userId);
    Task<bool> SetMutedAsync(int alertId, bool isMuted);
    Task<int> GetUserAlertCountAsync(int userId);
}
