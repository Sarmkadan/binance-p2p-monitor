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
    public const decimal DefaultSpreadThresholdPercent = 1.5m;
    public const decimal MinSpreadAlertPercent = 0.1m;
    public const decimal MaxSpreadAlertPercent = 5.0m;

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
    public const int CacheExpirationMinutes = 15;
    public const int MaxCacheSize = 1000;

    // Error handling
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
