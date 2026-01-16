#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Constants;
using BinanceP2pMonitor.Exceptions;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using Microsoft.Extensions.Logging;

namespace BinanceP2pMonitor.Services;

/// <summary>
/// Service for managing price alerts and sending notifications
/// </summary>
public class AlertService : IAlertService
{
    private readonly IAlertRepository _alertRepository;
    private readonly AppSettings _settings;
    private readonly ILogger<AlertService> _logger;

    public AlertService(
        IAlertRepository alertRepository,
        AppSettings settings,
        ILogger<AlertService> logger)
    {
        _alertRepository = alertRepository ?? throw new ArgumentNullException(nameof(alertRepository));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new alert
    /// </summary>
    public async Task<int> CreateAlertAsync(PriceAlert alert)
    {
        try
        {
            if (alert is null || !alert.IsValid())
                throw new InvalidAlertException("Alert configuration is invalid");

            var userAlertCount = await GetActiveAlertCountAsync(alert.UserId).ConfigureAwait(false);
            if (userAlertCount >= _settings.MaxAlertsPerUser)
                throw new InvalidAlertException(
                    $"Maximum number of alerts ({_settings.MaxAlertsPerUser}) reached");

            alert.CreatedAt = DateTime.UtcNow;
            alert.UpdatedAt = DateTime.UtcNow;

            return await _alertRepository.AddAsync(alert).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating alert for user {UserId}", alert?.UserId);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing alert
    /// </summary>
    public async Task<bool> UpdateAlertAsync(PriceAlert alert)
    {
        try
        {
            if (alert is null || alert.Id <= 0 || !alert.IsValid())
                throw new InvalidAlertException("Alert configuration is invalid");

            alert.UpdatedAt = DateTime.UtcNow;
            return await _alertRepository.UpdateAsync(alert).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating alert {AlertId}", alert?.Id);
            throw;
        }
    }

    /// <summary>
    /// Deletes an alert
    /// </summary>
    public async Task<bool> DeleteAlertAsync(int alertId)
    {
        try
        {
            return await _alertRepository.DeleteAsync(alertId).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting alert {AlertId}", alertId);
            throw;
        }
    }

    /// <summary>
    /// Gets user's alerts
    /// </summary>
    public async Task<IEnumerable<PriceAlert>> GetUserAlertsAsync(int userId)
    {
        try
        {
            return await _alertRepository.GetUserAlertsAsync(userId).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alerts for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Checks which alerts should trigger for a price update
    /// </summary>
    public async Task<IEnumerable<PriceAlert>> CheckTriggersAsync(Price currentPrice)
    {
        try
        {
            if (currentPrice is null || !currentPrice.IsValid())
                return Enumerable.Empty<PriceAlert>();

            var alerts = await _alertRepository.GetAlertsByAssetAndFiatAsync(currentPrice.Asset, currentPrice.Fiat).ConfigureAwait(false);
            var triggeredAlerts = new List<PriceAlert>();

            foreach (var alert in alerts)
            {
                if (alert.IsInCooldownPeriod(_settings.AlertCooldownMinutes))
                    continue;

                var changePercent = alert.AlertType switch
                {
                    AlertType.PriceChange => currentPrice.BuyChangePercent,
                    AlertType.HighSpreadAlert or AlertType.LowSpreadAlert => currentPrice.CalculateSpread(),
                    _ => 0
                };

                if (alert.ShouldTrigger(changePercent))
                {
                    alert.RecordTrigger();
                    await UpdateAlertAsync(alert).ConfigureAwait(false);
                    triggeredAlerts.Add(alert);

                    _logger.LogInformation("Alert triggered: {AlertDescription}", alert.GetDescription());
                }
            }

            return triggeredAlerts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking alert triggers");
            throw;
        }
    }

    /// <summary>
    /// Sends a notification via Telegram
    /// </summary>
    public async Task SendNotificationAsync(long telegramChatId, string message)
    {
        try
        {
            if (!_settings.EnableTelegramNotifications)
                return;

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be empty");

            // TODO: Implement actual Telegram bot integration
            _logger.LogInformation("Notification sent to {ChatId}: {Message}", telegramChatId, message);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to {ChatId}", telegramChatId);
            throw;
        }
    }

    /// <summary>
    /// Sends notifications to multiple users
    /// </summary>
    public async Task SendBulkNotificationsAsync(IEnumerable<long> chatIds, string message)
    {
        try
        {
            var tasks = chatIds.Select(chatId => SendNotificationAsync(chatId, message));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk notifications");
            throw;
        }
    }

    /// <summary>
    /// Tests an alert by sending a test notification
    /// </summary>
    public async Task<bool> TestAlertAsync(int alertId)
    {
        try
        {
            var alert = await _alertRepository.GetByIdAsync(alertId).ConfigureAwait(false);
            if (alert is null)
                throw new ResourceNotFoundException($"Alert {alertId} not found");

            var testMessage = $"Test notification for {alert.GetDescription()}";
            await SendNotificationAsync(alert.User?.TelegramChatId ?? 0, testMessage).ConfigureAwait(false);

            _logger.LogInformation("Alert test sent for alert {AlertId}", alertId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing alert {AlertId}", alertId);
            throw;
        }
    }

    /// <summary>
    /// Gets count of active alerts for a user
    /// </summary>
    public async Task<int> GetActiveAlertCountAsync(int userId)
    {
        try
        {
            return await _alertRepository.GetUserAlertCountAsync(userId).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting alert count for user {UserId}", userId);
            throw;
        }
    }
}
