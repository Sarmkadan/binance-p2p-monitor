#nullable enable

namespace BinanceP2pMonitor.Configuration;

/// <summary>
/// Extension methods for <see cref="AppSettings"/> providing convenient utility methods
/// </summary>
public static class AppSettingsExtensions
{
    /// <summary>
    /// Determines whether Telegram notifications are enabled and properly configured
    /// </summary>
    /// <param name="settings">The application settings</param>
    /// <returns>True if Telegram notifications are enabled and token/chat ID are set</returns>
    public static bool IsTelegramConfigured(this AppSettings settings)
    {
        return settings.EnableTelegramNotifications &&
               !string.IsNullOrWhiteSpace(settings.TelegramBotToken) &&
               !string.IsNullOrWhiteSpace(settings.TelegramAdminChatId);
    }

    /// <summary>
    /// Determines whether webhook notifications are enabled and properly configured
    /// </summary>
    /// <param name="settings">The application settings</param>
    /// <returns>True if webhook notifications are enabled and URL is set</returns>
    public static bool IsWebhookConfigured(this AppSettings settings)
    {
        return settings.EnableWebhookNotifications &&
               !string.IsNullOrWhiteSpace(settings.WebhookUrl);
    }

    /// <summary>
    /// Gets the effective monitoring interval in milliseconds
    /// </summary>
    /// <param name="settings">The application settings</param>
    /// <returns>Monitoring interval in milliseconds</returns>
    public static int GetMonitoringIntervalMs(this AppSettings settings)
    {
        return settings.MonitoringIntervalSeconds * 1000;
    }

    /// <summary>
    /// Gets the effective alert cooldown time in seconds
    /// </summary>
    /// <param name="settings">The application settings</param>
    /// <returns>Alert cooldown time in seconds</returns>
    public static int GetAlertCooldownSeconds(this AppSettings settings)
    {
        return settings.AlertCooldownMinutes * 60;
    }

    /// <summary>
    /// Gets the effective history retention period in hours
    /// </summary>
    /// <param name="settings">The application settings</param>
    /// <returns>History retention period in hours</returns>
    public static int GetHistoryRetentionHours(this AppSettings settings)
    {
        return settings.HistoryRetentionDays * 24;
    }

    /// <summary>
    /// Gets the effective database command timeout in milliseconds
    /// </summary>
    /// <param name="settings">The application settings</param>
    /// <returns>Database command timeout in milliseconds</returns>
    public static int GetDatabaseCommandTimeoutMs(this AppSettings settings)
    {
        return settings.DatabaseCommandTimeoutSeconds * 1000;
    }

    /// <summary>
    /// Gets the effective spread analysis history period in hours
    /// </summary>
    /// <param name="settings">The application settings</param>
    /// <returns>Spread analysis history period in hours</returns>
    public static int GetSpreadAnalysisHistoryPeriod(this AppSettings settings)
    {
        return settings.SpreadAnalysisHistoryHours;
    }

    /// <summary>
    /// Determines whether daily summary notifications are enabled
    /// </summary>
    /// <param name="settings">The application settings</param>
    /// <returns>True if daily summary is enabled (hour is between 0-23)</returns>
    public static bool IsDailySummaryEnabled(this AppSettings settings)
    {
        return settings.DailySummaryHourUtc >= 0 && settings.DailySummaryHourUtc <= 23;
    }

    /// <summary>
    /// Gets the effective daily summary hour in local time (converts from UTC)
    /// </summary>
    /// <param name="settings">The application settings</param>
    /// <returns>Daily summary hour in local time</returns>
    public static int GetDailySummaryLocalHour(this AppSettings settings)
    {
        if (!settings.IsDailySummaryEnabled())
            return -1;

        var utcHour = settings.DailySummaryHourUtc;
        var localHour = utcHour - DateTime.UtcNow.Hour + DateTime.Now.Hour;
        return (localHour + 24) % 24; // Ensure positive value
    }

    /// <summary>
    /// Gets the maximum number of alerts that can be sent within the cooldown period
    /// </summary>
    /// <param name="settings">The application settings</param>
    /// <returns>Maximum alerts per cooldown period</returns>
    public static int GetMaxAlertsPerCooldown(this AppSettings settings)
    {
        return settings.MaxAlertsPerUser;
    }

    /// <summary>
    /// Gets the price change threshold as a percentage
    /// </summary>
    /// <param name="settings">The application settings</param>
    /// <returns>Price change threshold percentage</returns>
    public static decimal GetPriceChangeThresholdPercent(this AppSettings settings)
    {
        return settings.DefaultPriceChangeThreshold;
    }

    /// <summary>
    /// Gets the spread threshold as a percentage
    /// </summary>
    /// <param name="settings">The application settings</param>
    /// <returns>Spread threshold percentage</returns>
    public static decimal GetSpreadThresholdPercent(this AppSettings settings)
    {
        return settings.DefaultSpreadThreshold;
    }

    /// <summary>
    /// Gets the list of assets to monitor, ensuring it's not null
    /// </summary>
    /// <param name="settings">The application settings</param>
    /// <returns>List of assets to monitor</returns>
    public static List<string> GetMonitoredAssets(this AppSettings settings)
    {
        return settings.MonitoredAssets ?? new List<string> { "USDT" };
    }

    /// <summary>
    /// Gets the list of fiat currencies to monitor, ensuring it's not null
    /// </summary>
    /// <param name="settings">The application settings</param>
    /// <returns>List of fiat currencies to monitor</returns>
    public static List<string> GetMonitoredFiats(this AppSettings settings)
    {
        return settings.MonitoredFiats ?? new List<string> { "RUB", "USD", "EUR" };
    }
}