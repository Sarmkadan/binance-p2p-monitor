#nullable enable
using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Services;

/// <summary>
/// Service interface for managing price alerts and notifications
/// </summary>
public interface IAlertService
{
    /// <summary>
    /// Creates a new price alert.
    /// </summary>
    /// <param name="alert">The <see cref="PriceAlert"/> object to create.</param>
    /// <returns>The ID of the created alert.</returns>
    Task<int> CreateAlertAsync(PriceAlert alert);

    /// <summary>
    /// Updates an existing price alert.
    /// </summary>
    /// <param name="alert">The <see cref="PriceAlert"/> object to update.</param>
    /// <returns>True if the alert was updated successfully; otherwise, false.</returns>
    Task<bool> UpdateAlertAsync(PriceAlert alert);

    /// <summary>
    /// Deletes a price alert by its ID.
    /// </summary>
    /// <param name="alertId">The ID of the alert to delete.</param>
    /// <returns>True if the alert was deleted successfully; otherwise, false.</returns>
    Task<bool> DeleteAlertAsync(int alertId);

    /// <summary>
    /// Gets all alerts for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A collection of <see cref="PriceAlert"/> objects.</returns>
    Task<IEnumerable<PriceAlert>> GetUserAlertsAsync(int userId);

    /// <summary>
    /// Checks if any alerts are triggered by the current price.
    /// </summary>
    /// <param name="currentPrice">The <see cref="Price"/> object to check against.</param>
    /// <returns>A collection of triggered <see cref="PriceAlert"/> objects.</returns>
    Task<IEnumerable<PriceAlert>> CheckTriggersAsync(Price currentPrice);

    /// <summary>
    /// Sends a notification to a specific Telegram chat.
    /// </summary>
    /// <param name="telegramChatId">The Telegram chat ID.</param>
    /// <param name="message">The message to send.</param>
    /// <returns>A task representing the operation.</returns>
    Task SendNotificationAsync(long telegramChatId, string message);

    /// <summary>
    /// Sends a notification to multiple Telegram chats.
    /// </summary>
    /// <param name="chatIds">A collection of Telegram chat IDs.</param>
    /// <param name="message">The message to send.</param>
    /// <returns>A task representing the operation.</returns>
    Task SendBulkNotificationsAsync(IEnumerable<long> chatIds, string message);

    /// <summary>
    /// Tests an alert.
    /// </summary>
    /// <param name="alertId">The ID of the alert to test.</param>
    /// <returns>True if the test was successful; otherwise, false.</returns>
    Task<bool> TestAlertAsync(int alertId);

    /// <summary>
    /// Gets the count of active alerts for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>The number of active alerts.</returns>
    Task<int> GetActiveAlertCountAsync(int userId);
}
