# BinanceP2PMonitorOptions

`BinanceP2PMonitorOptions` is the central configuration class for the Binance P2P Monitor application. It holds all runtime settings required to connect to databases, authenticate with external APIs (Binance and Telegram), control monitoring and alerting behavior, manage data retention, and specify logging parameters. An instance of this class is typically populated from application configuration files and injected into services that orchestrate P2P market surveillance.

## API

### `DatabaseConnectionString`
`public string DatabaseConnectionString`

The connection string used to establish a session with the application's backing database. This value must be a valid, non-empty connection string compatible with the configured database provider. No default is applied; the application will fail to start if this is null, empty, or malformed.

### `BinanceApiKey`
`public string? BinanceApiKey`

The API key credential for authenticating requests to the Binance API. May be `null` if the application is configured to operate without authenticated endpoints, but certain features (e.g., account-specific queries) will be unavailable. When provided, it must be paired with `BinanceApiSecret`.

### `BinanceApiSecret`
`public string? BinanceApiSecret`

The API secret credential corresponding to `BinanceApiKey`. Must be `null` if and only if `BinanceApiKey` is `null`. If both are supplied, they are used to sign Binance API requests. Leaking this value compromises the associated Binance account.

### `TelegramBotToken`
`public string? TelegramBotToken`

The token for the Telegram Bot API used to send notifications. If `null` or empty, Telegram notifications are disabled regardless of `EnableTelegramNotifications`. The token must be valid and obtained from BotFather for the bot to function.

### `TelegramAdminChatId`
`public string? TelegramAdminChatId`

The chat identifier (numeric or username-based) where administrative alerts and summaries are delivered. May be `null` if Telegram notifications are disabled or if admin alerts are not required. When `EnableTelegramNotifications` is `true` and this is `null`, admin-specific messages are silently dropped.

### `MonitoringIntervalSeconds`
`public int MonitoringIntervalSeconds`

The interval, in seconds, between consecutive P2P market data fetch cycles. Must be a positive integer. Values below 5 seconds are allowed but may trigger rate-limiting from upstream sources. Defaults to a value defined at application startup.

### `AlertCooldownMinutes`
`public int AlertCooldownMinutes`

The minimum number of minutes that must elapse before a subsequent alert of the same type is issued for the same monitored asset and user. Prevents notification flooding. Must be non-negative; a value of zero disables cooldown entirely.

### `MaxAlertsPerUser`
`public int MaxAlertsPerUser`

The maximum number of distinct alerts that can be tracked or dispatched for a single counterparty user within a monitoring session or time window. When exceeded, further alerts for that user are suppressed. Must be a positive integer.

### `DefaultPriceChangeThreshold`
`public double DefaultPriceChangeThreshold`

The default percentage (expressed as a decimal fraction, e.g., 0.05 for 5%) change in price that triggers a price-change alert when no asset-specific override exists. Must be non-negative. A value of zero effectively disables price-change alerts by default.

### `DefaultSpreadThreshold`
`public double DefaultSpreadThreshold`

The default percentage spread (difference between buy and sell prices, expressed as a decimal fraction) that triggers a spread alert when no asset-specific override exists. Must be non-negative. A value of zero disables spread alerts by default.

### `HistoryRetentionDays`
`public int HistoryRetentionDays`

The number of calendar days that historical P2P market data is retained in the database before becoming eligible for cleanup. Must be a positive integer. Records older than this threshold are removed when `EnableAutoCleanup` is `true`.

### `MaxHistoryRecords`
`public int MaxHistoryRecords`

The absolute upper bound on the number of historical records retained per monitored asset. When the count exceeds this limit, the oldest records are purged first, regardless of their age. Must be a positive integer.

### `DatabaseCommandTimeoutSeconds`
`public int DatabaseCommandTimeoutSeconds`

The timeout, in seconds, applied to individual database commands executed by the application. Must be a positive integer. Commands exceeding this duration are cancelled and an exception is raised by the data layer.

### `EnableWebSocket`
`public bool EnableWebSocket`

Controls whether the application establishes a WebSocket connection for real-time data streams. When `false`, only periodic polling via `MonitoringIntervalSeconds` is used. WebSocket connectivity may require valid API credentials.

### `EnableTelegramNotifications`
`public bool EnableTelegramNotifications`

Master switch for Telegram notification delivery. When `false`, no Telegram messages are sent even if `TelegramBotToken` and `TelegramAdminChatId` are configured. When `true`, notifications are sent only if the token is valid and the target chat is reachable.

### `EnableAutoCleanup`
`public bool EnableAutoCleanup`

Determines whether automatic database cleanup routines execute. When `true`, records exceeding `HistoryRetentionDays` or `MaxHistoryRecords` are periodically purged. When `false`, cleanup is deferred or must be performed manually.

### `DailySummaryHourUtc`
`public int DailySummaryHourUtc`

The hour of the day (0–23) in UTC at which a daily summary report is generated and dispatched (if notifications are enabled). Values outside 0–23 are clamped or cause a configuration validation error at startup.

### `LogLevel`
`public string LogLevel`

The minimum log severity level for the application. Expected values follow standard level names (e.g., `"Debug"`, `"Information"`, `"Warning"`, `"Error"`, `"Critical"`). Invalid or unrecognized values cause a configuration error at startup.

### `LogPath`
`public string LogPath`

The file system path where log output is written. May be a directory (for rolling file logs) or a full file path. If `null` or empty, logging to file is disabled and output defaults to console or another configured sink.

### `MonitoredAssets`
`public string[] MonitoredAssets`

An array of asset symbols (e.g., `"USDT"`, `"BTC"`, `"ETH"`) that the monitor tracks on Binance P2P. Must not be `null`; an empty array means no assets are monitored and the application effectively idles. Each element should be a valid, uppercase asset code recognized by Binance P2P.

## Usage

### Example 1: Basic Configuration for Polling-Only Monitoring

```csharp
var options = new BinanceP2PMonitorOptions
{
    DatabaseConnectionString = "Host=localhost;Database=binance_p2p;Username=monitor;Password=securepass",
    MonitoringIntervalSeconds = 30,
    AlertCooldownMinutes = 15,
    MaxAlertsPerUser = 5,
    DefaultPriceChangeThreshold = 0.03,   // 3%
    DefaultSpreadThreshold = 0.02,        // 2%
    HistoryRetentionDays = 90,
    MaxHistoryRecords = 100_000,
    DatabaseCommandTimeoutSeconds = 60,
    EnableWebSocket = false,
    EnableTelegramNotifications = false,
    EnableAutoCleanup = true,
    DailySummaryHourUtc = 8,
    LogLevel = "Information",
    LogPath = "/var/log/binance-p2p-monitor/",
    MonitoredAssets = new[] { "USDT", "BTC", "BUSD" }
};

// Validate and inject into monitoring service
var validator = new ConfigurationValidator();
validator.Validate(options);
var monitor = new P2PMonitorService(options);
await monitor.StartAsync();
```

### Example 2: Full Configuration with WebSocket, Telegram Alerts, and API Credentials

```csharp
var options = new BinanceP2PMonitorOptions
{
    DatabaseConnectionString = Environment.GetEnvironmentVariable("P2P_DB_CONNECTION"),
    BinanceApiKey = Environment.GetEnvironmentVariable("BINANCE_API_KEY"),
    BinanceApiSecret = Environment.GetEnvironmentVariable("BINANCE_API_SECRET"),
    TelegramBotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN"),
    TelegramAdminChatId = "-1001234567890",
    MonitoringIntervalSeconds = 10,
    AlertCooldownMinutes = 30,
    MaxAlertsPerUser = 3,
    DefaultPriceChangeThreshold = 0.05,
    DefaultSpreadThreshold = 0.015,
    HistoryRetentionDays = 60,
    MaxHistoryRecords = 50_000,
    DatabaseCommandTimeoutSeconds = 30,
    EnableWebSocket = true,
    EnableTelegramNotifications = true,
    EnableAutoCleanup = true,
    DailySummaryHourUtc = 18,
    LogLevel = "Debug",
    LogPath = "logs/monitor.log",
    MonitoredAssets = new[] { "USDT", "ETH", "BNB" }
};

// Options can be bound directly from IConfiguration in ASP.NET or HostBuilder contexts
var host = Host.CreateDefaultBuilder()
    .ConfigureServices((ctx, services) =>
    {
        services.Configure<BinanceP2PMonitorOptions>(ctx.Configuration.GetSection("P2PMonitor"));
        services.AddHostedService<MonitorBackgroundService>();
    })
    .Build();

await host.RunAsync();
```

## Notes

- **Validation**: No validation is performed automatically upon property assignment. Consumers must explicitly validate the object before use. Common validation failures include non-positive integers for interval/retention fields, mismatched API key/secret presence, and unrecognized `LogLevel` strings.
- **Nullability**: Reference-type properties (`BinanceApiKey`, `BinanceApiSecret`, `TelegramBotToken`, `TelegramAdminChatId`, `LogPath`) are explicitly nullable. Null values disable the corresponding feature or cause it to degrade gracefully. `MonitoredAssets` must not be null; an empty array is the correct way to express "no assets."
- **Thread Safety**: This class is a plain options container with no internal synchronization. It is safe for concurrent reads after initial configuration is complete. Concurrent writes without external locking may cause torn reads or inconsistent state. The intended usage pattern is write-once at startup, then read-only across multiple threads.
- **Sensitive Data**: `BinanceApiSecret`, `BinanceApiKey`, `TelegramBotToken`, and `DatabaseConnectionString` contain secrets. These should never be hardcoded or logged. The application's logging pipeline should be configured to redact these properties if options objects are serialized.
- **WebSocket and Credentials**: Setting `EnableWebSocket = true` without valid `BinanceApiKey` and `BinanceApiSecret` may result in connection failures or fallback to unauthenticated streams with reduced functionality, depending on the underlying Binance API requirements.
- **Telegram Dependency Chain**: For a Telegram notification to be delivered, all of the following must hold: `EnableTelegramNotifications == true`, `TelegramBotToken` is non-null and valid, and the target chat ID (user or admin) is non-null and reachable. If any link in this chain is broken, the notification is silently suppressed.
- **Cleanup Semantics**: When `EnableAutoCleanup` is `true`, both `HistoryRetentionDays` and `MaxHistoryRecords` act as upper bounds. Records that satisfy *either* condition (too old OR exceeding the per-asset cap) become eligible for removal. The cleanup routine typically runs on a schedule independent of the monitoring cycle.
- **Asset Case Sensitivity**: `MonitoredAssets` elements should match the casing expected by Binance P2P (typically uppercase). Case mismatches may result in zero matches and an effectively idle monitor without an explicit error.
