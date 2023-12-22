#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Services;

/// <summary>
/// Service interface for managing price alerts and notifications
/// </summary>
public interface IAlertService
{
    Task<int> CreateAlertAsync(PriceAlert alert);
    Task<bool> UpdateAlertAsync(PriceAlert alert);
    Task<bool> DeleteAlertAsync(int alertId);
    Task<IEnumerable<PriceAlert>> GetUserAlertsAsync(int userId);
    Task<IEnumerable<PriceAlert>> CheckTriggersAsync(Price currentPrice);
    Task SendNotificationAsync(long telegramChatId, string message);
    Task SendBulkNotificationsAsync(IEnumerable<long> chatIds, string message);
    Task<bool> TestAlertAsync(int alertId);
    Task<int> GetActiveAlertCountAsync(int userId);
}
