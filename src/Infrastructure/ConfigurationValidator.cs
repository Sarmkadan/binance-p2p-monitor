// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Infrastructure;

/// <summary>
/// Validates application configuration before startup
/// </summary>
public class ConfigurationValidator
{
    private readonly AppSettings _appSettings;
    private readonly ILogger<ConfigurationValidator> _logger;

    public ConfigurationValidator(AppSettings appSettings, ILogger<ConfigurationValidator> logger)
    {
        _appSettings = appSettings;
        _logger = logger;
    }

    /// <summary>
    /// Validates all configuration settings
    /// </summary>
    /// <returns>List of validation errors, empty if valid</returns>
    public List<string> Validate()
    {
        var errors = new List<string>();

        _logger.LogInformation("Validating configuration...");

        errors.AddRange(ValidateDatabase());
        errors.AddRange(ValidateMonitoring());
        errors.AddRange(ValidateAlerting());
        errors.AddRange(ValidateTelegram());
        errors.AddRange(ValidateAssets());

        if (errors.Any())
        {
            _logger.LogError("Configuration validation failed with {ErrorCount} errors", errors.Count);
            foreach (var error in errors)
                _logger.LogError("  - {Error}", error);
        }
        else
        {
            _logger.LogInformation("Configuration validation passed");
        }

        return errors;
    }

    private List<string> ValidateDatabase()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(_appSettings.DatabaseConnectionString))
            errors.Add("DatabaseConnectionString is required");

        if (_appSettings.HistoryRetentionDays < 1)
            errors.Add("HistoryRetentionDays must be at least 1");

        if (_appSettings.MaxHistoryRecords < 100)
            errors.Add("MaxHistoryRecords must be at least 100");

        if (_appSettings.DatabaseCommandTimeoutSeconds < 5)
            errors.Add("DatabaseCommandTimeoutSeconds must be at least 5");

        return errors;
    }

    private List<string> ValidateMonitoring()
    {
        var errors = new List<string>();

        if (_appSettings.MonitoringIntervalSeconds < 5)
            errors.Add("MonitoringIntervalSeconds must be at least 5 seconds");

        if (!_appSettings.EnableWebSocket && !_appSettings.EnableTelegramNotifications)
            errors.Add("At least one notification method must be enabled");

        return errors;
    }

    private List<string> ValidateAlerting()
    {
        var errors = new List<string>();

        if (_appSettings.AlertCooldownMinutes < 1)
            errors.Add("AlertCooldownMinutes must be at least 1");

        if (_appSettings.MaxAlertsPerUser < 1)
            errors.Add("MaxAlertsPerUser must be at least 1");

        if (_appSettings.DefaultPriceChangeThreshold < 0)
            errors.Add("DefaultPriceChangeThreshold cannot be negative");

        if (_appSettings.DefaultSpreadThreshold < 0)
            errors.Add("DefaultSpreadThreshold cannot be negative");

        return errors;
    }

    private List<string> ValidateTelegram()
    {
        var errors = new List<string>();

        if (_appSettings.EnableTelegramNotifications)
        {
            if (string.IsNullOrWhiteSpace(_appSettings.TelegramBotToken))
                errors.Add("TelegramBotToken is required when EnableTelegramNotifications is true");

            if (string.IsNullOrWhiteSpace(_appSettings.TelegramAdminChatId))
                errors.Add("TelegramAdminChatId is required when EnableTelegramNotifications is true");

            if (!long.TryParse(_appSettings.TelegramAdminChatId, out _))
                errors.Add("TelegramAdminChatId must be a valid numeric chat ID");
        }

        return errors;
    }

    private List<string> ValidateAssets()
    {
        var errors = new List<string>();

        if (!_appSettings.MonitoredAssets.Any())
            _logger.LogWarning("No monitored assets configured");

        if (!_appSettings.MonitoredFiats.Any())
            _logger.LogWarning("No monitored fiats configured");

        return errors;
    }
}
