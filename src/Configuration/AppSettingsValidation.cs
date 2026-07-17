#nullable enable

using System.Globalization;

namespace BinanceP2pMonitor.Configuration;

/// <summary>
/// Provides validation helpers for <see cref="AppSettings"/> configuration
/// </summary>
public static class AppSettingsValidation
{
    /// <summary>
    /// Validates the application settings and returns a list of validation problems.
    /// </summary>
    /// <param name="settings">The settings to validate</param>
    /// <returns>An immutable list of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null</exception>
    public static IReadOnlyList<string> Validate(this AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var problems = new List<string>();

        // String properties validation
        if (string.IsNullOrWhiteSpace(settings.DatabaseConnectionString))
            problems.Add("DatabaseConnectionString cannot be null or whitespace");

        if (string.IsNullOrWhiteSpace(settings.BinanceApiKey))
            problems.Add("BinanceApiKey cannot be null or whitespace");

        if (string.IsNullOrWhiteSpace(settings.BinanceApiSecret))
            problems.Add("BinanceApiSecret cannot be null or whitespace");

        if (string.IsNullOrWhiteSpace(settings.TelegramBotToken))
            problems.Add("TelegramBotToken cannot be null or whitespace");

        if (string.IsNullOrWhiteSpace(settings.TelegramAdminChatId))
            problems.Add("TelegramAdminChatId cannot be null or whitespace");

        if (settings.EnableWebhookNotifications && string.IsNullOrWhiteSpace(settings.WebhookUrl))
            problems.Add("WebhookUrl cannot be null or whitespace when EnableWebhookNotifications is true");

        // Numeric properties validation
        if (settings.MonitoringIntervalSeconds <= 0)
            problems.Add("MonitoringIntervalSeconds must be a positive number");

        if (settings.MonitoringIntervalSeconds > 86400) // 24 hours in seconds
            problems.Add("MonitoringIntervalSeconds cannot exceed 86400 (24 hours)");

        if (settings.AlertCooldownMinutes <= 0)
            problems.Add("AlertCooldownMinutes must be a positive number");

        if (settings.AlertCooldownMinutes > 1440) // 24 hours in minutes
            problems.Add("AlertCooldownMinutes cannot exceed 1440 (24 hours)");

        if (settings.MaxAlertsPerUser <= 0)
            problems.Add("MaxAlertsPerUser must be a positive number");

        if (settings.MaxAlertsPerUser > 1000)
            problems.Add("MaxAlertsPerUser cannot exceed 1000");

        if (settings.DefaultPriceChangeThreshold < 0)
            problems.Add("DefaultPriceChangeThreshold cannot be negative");

        if (settings.DefaultSpreadThreshold < 0)
            problems.Add("DefaultSpreadThreshold cannot be negative");

        if (settings.HistoryRetentionDays <= 0)
            problems.Add("HistoryRetentionDays must be a positive number");

        if (settings.HistoryRetentionDays > 3650) // ~10 years
            problems.Add("HistoryRetentionDays cannot exceed 3650 (10 years)");

        if (settings.MaxHistoryRecords <= 0)
            problems.Add("MaxHistoryRecords must be a positive number");

        if (settings.MaxHistoryRecords > 1000000)
            problems.Add("MaxHistoryRecords cannot exceed 1000000");

        if (settings.DatabaseCommandTimeoutSeconds <= 0)
            problems.Add("DatabaseCommandTimeoutSeconds must be a positive number");

        if (settings.DatabaseCommandTimeoutSeconds > 600) // 10 minutes
            problems.Add("DatabaseCommandTimeoutSeconds cannot exceed 600 (10 minutes)");

        if (settings.SpreadAnalysisHistoryHours <= 0)
            problems.Add("SpreadAnalysisHistoryHours must be a positive number");

        if (settings.SpreadAnalysisHistoryHours > 720) // 30 days
            problems.Add("SpreadAnalysisHistoryHours cannot exceed 720 (30 days)");

        // Date/time validation
        if (settings.DailySummaryHourUtc < -1 || settings.DailySummaryHourUtc > 23)
            problems.Add("DailySummaryHourUtc must be between -1 and 23 (inclusive)");

        // Boolean flags don't need validation

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the application settings are valid.
    /// </summary>
    /// <param name="settings">The settings to check</param>
    /// <returns>True if the settings are valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null</exception>
    public static bool IsValid(this AppSettings settings)
    {
        return Validate(settings).Count == 0;
    }

    /// <summary>
    /// Validates the application settings and throws an exception if invalid.
    /// </summary>
    /// <param name="settings">The settings to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when the settings are invalid</exception>
    public static void EnsureValid(this AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var problems = Validate(settings);

        if (problems.Count == 0)
            return;

        throw new ArgumentException(
            $"Configuration validation failed:\n{string.Join("\n", problems.Select((p, i) => $"{i + 1}. {p}"))}");
    }
}