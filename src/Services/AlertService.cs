#nullable enable
using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Constants;
using BinanceP2pMonitor.Exceptions;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using BinanceP2pMonitor.Integration;
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
    private readonly ITelegramNotificationClient _telegramNotificationClient;
    private readonly IWebhookNotificationClient _webhookNotificationClient;

    public AlertService(
        IAlertRepository alertRepository,
        AppSettings settings,
        ILogger<AlertService> logger,
        ITelegramNotificationClient telegramNotificationClient,
        IWebhookNotificationClient webhookNotificationClient)
    {
        _alertRepository = alertRepository ?? throw new ArgumentNullException(nameof(alertRepository));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _telegramNotificationClient = telegramNotificationClient ?? throw new ArgumentNullException(nameof(telegramNotificationClient));
        _webhookNotificationClient = webhookNotificationClient ?? throw new ArgumentNullException(nameof(webhookNotificationClient));
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

                if (alert.IsMuted)
                    continue;

                var changePercent = alert.AlertType switch
                {
                    AlertType.PriceChange => currentPrice.BuyChangePercent,
                    AlertType.HighSpreadAlert or AlertType.LowSpreadAlert => currentPrice.CalculateSpread(),
                    _ => 0
                };

                if (ShouldTriggerWithHysteresis(alert, changePercent))
                {
                    alert.RecordTrigger(changePercent);
                    await UpdateAlertAsync(alert).ConfigureAwait(false);
                    triggeredAlerts.Add(alert);

                    _logger.LogInformation("Alert triggered: {AlertDescription}", alert.GetDescription());

                    // Deliver webhook notification when configured
                    if (_settings.EnableWebhookNotifications && !string.IsNullOrWhiteSpace(_settings.WebhookUrl))
                    {
                        await _webhookNotificationClient.SendPriceAlertAsync(
                            currentPrice.Asset,
                            currentPrice.Fiat,
                            currentPrice.BuyPrice,
                            currentPrice.SellPrice,
                            alert.GetDescription()).ConfigureAwait(false);
                    }
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
    /// Determines if an alert should trigger considering hysteresis
    /// </summary>
    /// <param name="alert">The alert to check</param>
    /// <param name="currentChange">Current price change percentage</param>
    /// <returns>True if the alert should trigger</returns>
    private bool ShouldTriggerWithHysteresis(PriceAlert alert, decimal currentChange)
    {
        // If alert has never triggered before, use normal triggering logic
        if (alert.LastTriggeredAt is null)
        {
            return alert.ShouldTrigger(currentChange);
        }

        // If alert is in cooldown period, don't trigger
        if (alert.IsInCooldownPeriod(_settings.AlertCooldownMinutes))
        {
            return false;
        }

        // Check if alert should trigger based on its condition
        if (!alert.ShouldTrigger(currentChange))
        {
            return false;
        }

        // For price change alerts, apply hysteresis
        if (alert.AlertType == AlertType.PriceChange)
        {
            // Calculate hysteresis threshold based on alert type
            decimal hysteresisMargin = _settings.PriceChangeHysteresisPercent;

            // For alerts that trigger when price goes above threshold
            if (alert.Condition == AlertCondition.GreaterThan || alert.Condition == AlertCondition.GreaterThanOrEqual)
            {
                // Price must drop below: threshold - hysteresis to re-trigger
                // Example: threshold=5%, hysteresis=0.5%, alert triggers when price > 5%
                // After triggering, price must drop below 4.5% to re-trigger
                decimal hysteresisThreshold = alert.Threshold - hysteresisMargin;

                // If we haven't triggered yet, or price moved back past hysteresis threshold
                if (alert.LastTriggerDirection == AlertDirection.Up)
                {
                    // Price went up to trigger, now must drop back below hysteresis threshold
                    return currentChange < hysteresisThreshold;
                }
            }

            // For alerts that trigger when price goes below threshold
            if (alert.Condition == AlertCondition.LessThan || alert.Condition == AlertCondition.LessThanOrEqual)
            {
                // Price must rise above: threshold + hysteresis to re-trigger
                // Example: threshold=2%, hysteresis=0.5%, alert triggers when price < 2%
                // After triggering, price must rise above 2.5% to re-trigger
                decimal hysteresisThreshold = alert.Threshold + hysteresisMargin;

                // If we haven't triggered yet, or price moved back past hysteresis threshold
                if (alert.LastTriggerDirection == AlertDirection.Down)
                {
                    // Price went down to trigger, now must rise above hysteresis threshold
                    return currentChange > hysteresisThreshold;
                }
            }
        }

        // For spread alerts, apply hysteresis
        if (alert.AlertType == AlertType.HighSpreadAlert || alert.AlertType == AlertType.LowSpreadAlert)
        {
            decimal hysteresisMargin = _settings.SpreadHysteresisPercent;

            // For high spread alerts (trigger when spread > threshold)
            if (alert.AlertType == AlertType.HighSpreadAlert)
            {
                // Spread must drop below: threshold - hysteresis to re-trigger
                decimal hysteresisThreshold = alert.Threshold - hysteresisMargin;

                if (alert.LastTriggerDirection == AlertDirection.Up)
                {
                    return currentChange < hysteresisThreshold;
                }
            }

            // For low spread alerts (trigger when spread < threshold)
            if (alert.AlertType == AlertType.LowSpreadAlert)
            {
                // Spread must rise above: threshold + hysteresis to re-trigger
                decimal hysteresisThreshold = alert.Threshold + hysteresisMargin;

                if (alert.LastTriggerDirection == AlertDirection.Down)
                {
                    return currentChange > hysteresisThreshold;
                }
            }
        }

        // For other alert types or first-time triggers, use normal logic
        return true;
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

            // For per-user alerts, ensure _telegramNotificationClient.SendMessageAsync accepts and uses the chatId.
            await _telegramNotificationClient.SendMessageAsync(telegramChatId, message).ConfigureAwait(false);
            _logger.LogInformation("Notification sent to {ChatId}: {Message}", telegramChatId, message);
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

    /// <summary>
    /// Sets the muted status of an alert.
    /// </summary>
    /// <param name="alertId">The ID of the alert.</param>
    /// <param name="isMuted">True to mute the alert; false to unmute it.</param>
    /// <returns>True if the operation was successful; otherwise, false.</returns>
    public async Task<bool> SetMutedAsync(int alertId, bool isMuted)
    {
        try
        {
            return await _alertRepository.SetMutedAsync(alertId, isMuted).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting muted status for alert {AlertId}", alertId);
            throw;
        }
    }
}
