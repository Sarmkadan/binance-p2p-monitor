#nullable enable
namespace BinanceP2pMonitor.Constants;

/// <summary>
/// Application-wide constants
/// </summary>
public static class ApplicationConstants
{
    public const string AppName = "Binance P2P Monitor";
    public const string Version = "1.0.0";
    public const string Author = "Vladyslav Zaiets";

    // WebSocket URLs
    public const string BinanceWebSocketUrl = "wss://stream.binance.com:9443/ws";
    public const string BinanceP2pApiUrl = "https://p2p.binance.com/api/v2/";

    // Price monitoring
    public const int DefaultMonitoringIntervalSeconds = 30;
    public const int MaxPriceHistoryDays = 90;
    public const int DefaultHistoryRetentionDays = 30;

    // Alert configuration
    // The CLI runs single-user; all alerts created from the console belong to this profile
    public const int DefaultCliUserId = 1;
    public const int AlertCooldownMinutes = 5;
    public const int MaxAlertsPerUser = 20;
    public const int DefaultPriceChangeThreshold = 2; // percentage

    // Spread analysis
    /// <summary>Multiplier used to convert a decimal ratio to a percentage.</summary>
    public const decimal PercentageMultiplier = 100m;

    /// <summary>Default number of decimal places retained for spread calculations.</summary>
    public const int DefaultSpreadDecimalPlaces = 4;

    /// <summary>Default Z-score threshold used to identify an anomalous spread.</summary>
    public const decimal DefaultSpreadAnomalyZScoreThreshold = 2.0m;

    /// <summary>Default maximum spread percentage accepted for spread alerts.</summary>
    public const decimal DefaultMaxSpreadThresholdPercent = 5.0m;

    public const decimal DefaultSpreadThresholdPercent = 1.5m;
    public const decimal MinSpreadAlertPercent = 0.1m;
    public const decimal MaxSpreadAlertPercent = 5.0m;

    // Display formatting
    /// <summary>Default number of decimal places used when displaying monetary values.</summary>
    public const int DefaultDecimalDisplayPlaces = 2;

    /// <summary>Default number of decimal places used when rounding stored price values.</summary>
    public const int DefaultPriceDecimalPlaces = 8;

    // Database
    public const int DatabaseCommandTimeoutSeconds = 30;
    public const int MaxDatabaseConnections = 10;

    // Telegram
    public const int TelegramMaxMessageLength = 4096;
    public const int TelegramReconnectIntervalSeconds = 60;

    // Performance
    public const int BatchSizeForBulkOperations = 1000;
    public const int MaxConcurrentRequests = 5;

    // Validation
    public const int MinPriceValue = 1;
    public const int MaxPriceValue = 1_000_000_000;
    public const int MinPasswordLength = 8;
    public const int MaxUsernameLength = 100;

    // Cache
    /// <summary>Default time-to-live, in seconds, for short-lived cached price data.</summary>
    public const int DefaultCacheTtlSeconds = 30;

    public const int CacheExpirationMinutes = 15;
    public const int MaxCacheSize = 1000;

    // Error handling
    /// <summary>Default number of retry attempts for transient operations.</summary>
    public const int DefaultRetryCount = 3;

    /// <summary>Default initial delay, in milliseconds, between retry attempts.</summary>
    public const int DefaultRetryDelayMilliseconds = 1000;

    /// <summary>Default maximum delay, in seconds, allowed between retry attempts.</summary>
    public const int DefaultMaxRetryDelaySeconds = 30;

    /// <summary>Default multiplier applied to successive retry delays.</summary>
    public const double DefaultRetryBackoffMultiplier = 2.0;

    public const int MaxRetryAttempts = 3;
    public const int RetryDelayMilliseconds = 1000;
    public const int MaxExceptionLogLength = 2000;

    // Supported assets (can be extended)
    public static readonly HashSet<string> SupportedAssets = new()
    {
        "BTC", "ETH", "BNB", "USDT", "USDC", "XRP", "SOL", "ADA", "DOGE", "MATIC"
    };

    // Supported fiat currencies (can be extended)
    public static readonly HashSet<string> SupportedFiats = new()
    {
        "USD", "EUR", "GBP", "JPY", "CNY", "INR", "AUD", "CAD", "SGD", "HKD"
    };

    // Common trading pairs
    public static readonly List<string> PopularPairs = new()
    {
        "BTC/USD", "ETH/USD", "BNB/USD", "USDT/USD",
        "BTC/EUR", "ETH/EUR", "BTC/GBP", "ETH/GBP"
    };
}
