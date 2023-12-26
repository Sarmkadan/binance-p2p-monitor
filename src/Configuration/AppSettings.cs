#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Configuration;

/// <summary>
/// Application settings configuration
/// </summary>
public class AppSettings
{
    public string DatabaseConnectionString { get; set; } = string.Empty;
    public string BinanceApiKey { get; set; } = string.Empty;
    public string BinanceApiSecret { get; set; } = string.Empty;
    public string TelegramBotToken { get; set; } = string.Empty;
    public string TelegramAdminChatId { get; set; } = string.Empty;

    public int MonitoringIntervalSeconds { get; set; } = 30;
    public int AlertCooldownMinutes { get; set; } = 5;
    public int MaxAlertsPerUser { get; set; } = 20;
    public decimal DefaultPriceChangeThreshold { get; set; } = 2.0m;
    public decimal DefaultSpreadThreshold { get; set; } = 1.5m;

    public int HistoryRetentionDays { get; set; } = 30;
    public int MaxHistoryRecords { get; set; } = 100000;
    public int DatabaseCommandTimeoutSeconds { get; set; } = 30;
    public int SpreadAnalysisHistoryHours { get; set; } = 24;

    public bool EnableWebSocket { get; set; } = true;
    public bool EnableTelegramNotifications { get; set; } = true;
    public bool EnableAutoCleanup { get; set; } = true;

    public string LogLevel { get; set; } = "Information";
    public string LogPath { get; set; } = "./logs";

    public List<string> MonitoredAssets { get; set; } = new();
    public List<string> MonitoredFiats { get; set; } = new();

    /// <summary>
    /// Validates the settings
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(DatabaseConnectionString) &&
               MonitoringIntervalSeconds > 0 &&
               AlertCooldownMinutes > 0 &&
               MaxAlertsPerUser > 0 &&
               HistoryRetentionDays > 0;
    }

    /// <summary>
    /// Gets validated settings or throws exception
    /// </summary>
    public void Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(DatabaseConnectionString))
            errors.Add("DatabaseConnectionString is required");

        if (MonitoringIntervalSeconds < 5)
            errors.Add("MonitoringIntervalSeconds must be at least 5");

        if (AlertCooldownMinutes < 1)
            errors.Add("AlertCooldownMinutes must be at least 1");

        if (MaxAlertsPerUser < 1)
            errors.Add("MaxAlertsPerUser must be at least 1");

        if (HistoryRetentionDays < 1)
            errors.Add("HistoryRetentionDays must be at least 1");
            
        if (SpreadAnalysisHistoryHours < 1)
            errors.Add("SpreadAnalysisHistoryHours must be at least 1");

        if (DefaultPriceChangeThreshold < 0)
            errors.Add("DefaultPriceChangeThreshold cannot be negative");

        if (DefaultSpreadThreshold < 0)
            errors.Add("DefaultSpreadThreshold cannot be negative");

        if (errors.Any())
            throw new Exceptions.ConfigurationException(
                $"Configuration validation failed: {string.Join(", ", errors)}");
    }

    /// <summary>
    /// Gets the connection string with environment variables resolved
    /// </summary>
    public string GetResolvedConnectionString()
    {
        var resolved = DatabaseConnectionString;

        // Replace common environment variable patterns
        resolved = resolved.Replace("${APPDATA}", Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData));
        resolved = resolved.Replace("${USERPROFILE}", Environment.GetFolderPath(
            Environment.SpecialFolder.UserProfile));

        return resolved;
    }
}
