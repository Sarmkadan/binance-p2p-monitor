using System.ComponentModel.DataAnnotations;

namespace BinanceP2pMonitor.Configuration
{
    /// <summary>
    /// Strongly‑typed configuration options for the Binance P2P Monitor.
    /// These options are bound to the "AppSettings" section of appsettings.json
    /// and validated using DataAnnotations.
    /// </summary>
    public class BinanceP2PMonitorOptions
    {
        // Database
        [Required(ErrorMessage = "DatabaseConnectionString is required.")]
        public string DatabaseConnectionString { get; set; } = string.Empty;

        // Binance API credentials (optional – required only when WebSocket is enabled)
        public string? BinanceApiKey { get; set; }
        public string? BinanceApiSecret { get; set; }

        // Telegram bot credentials (optional – required only when Telegram notifications are enabled)
        public string? TelegramBotToken { get; set; }
        public string? TelegramAdminChatId { get; set; }

        // Timing & limits
        [Range(1, int.MaxValue, ErrorMessage = "MonitoringIntervalSeconds must be at least 1.")]
        public int MonitoringIntervalSeconds { get; set; } = 30;

        [Range(0, int.MaxValue, ErrorMessage = "AlertCooldownMinutes cannot be negative.")]
        public int AlertCooldownMinutes { get; set; } = 5;

        [Range(1, int.MaxValue, ErrorMessage = "MaxAlertsPerUser must be at least 1.")]
        public int MaxAlertsPerUser { get; set; } = 20;

        // Thresholds
        [Range(0.0, double.MaxValue, ErrorMessage = "DefaultPriceChangeThreshold must be non‑negative.")]
        public double DefaultPriceChangeThreshold { get; set; } = 2.0;

        [Range(0.0, double.MaxValue, ErrorMessage = "DefaultSpreadThreshold must be non‑negative.")]
        public double DefaultSpreadThreshold { get; set; } = 1.5;

        // History handling
        [Range(1, int.MaxValue, ErrorMessage = "HistoryRetentionDays must be at least 1.")]
        public int HistoryRetentionDays { get; set; } = 30;

        [Range(1, int.MaxValue, ErrorMessage = "MaxHistoryRecords must be at least 1.")]
        public int MaxHistoryRecords { get; set; } = 100_000;

        // Database command timeout
        [Range(1, int.MaxValue, ErrorMessage = "DatabaseCommandTimeoutSeconds must be at least 1.")]
        public int DatabaseCommandTimeoutSeconds { get; set; } = 30;

        // Feature toggles
        public bool EnableWebSocket { get; set; } = true;
        public bool EnableTelegramNotifications { get; set; } = true;
        public bool EnableAutoCleanup { get; set; } = true;

        // Daily summary
        [Range(0, 23, ErrorMessage = "DailySummaryHourUtc must be between 0 and 23.")]
        public int DailySummaryHourUtc { get; set; } = 9;

        // Merchant quality filters
        [Range(0, 100, ErrorMessage = "MinCompletionRate must be between 0 and 100.")]
        public decimal MinCompletionRate { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "MinOrderCount must be non-negative.")]
        public int MinOrderCount { get; set; } = 0;

        // Logging
        [Required(ErrorMessage = "LogLevel is required.")]
        public string LogLevel { get; set; } = "Information";

        [Required(ErrorMessage = "LogPath is required.")]
        public string LogPath { get; set; } = "./logs";

        // Monitored symbols
        public string[] MonitoredAssets { get; set; } = new[] { "BTC", "ETH", "BNB", "USDT" };
        public string[] MonitoredFiats { get; set; } = new[] { "USD", "EUR", "GBP" };
    }
}
