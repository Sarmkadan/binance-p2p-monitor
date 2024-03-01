# AppSettings

The `AppSettings` class serves as the central configuration container for the `binance-p2p-monitor` application, encapsulating all necessary parameters for database connectivity, external service integration (Binance API, Telegram, Webhooks), and operational behavior such as monitoring intervals, alert thresholds, and data retention policies. Instances of this class are typically populated from configuration sources at application startup and injected into services to govern runtime behavior without requiring direct access to configuration files.

## API

### DatabaseConnectionString
Gets or sets the connection string used to establish a connection with the underlying database.
*   **Type**: `string`
*   **Purpose**: Defines the target database server, authentication credentials, and initial catalog.
*   **Remarks**: Must be a valid ADO.NET connection string format compatible with the configured database provider.

### BinanceApiKey
Gets or sets the public API key for authenticating requests to the Binance API.
*   **Type**: `string`
*   **Purpose**: Identifies the application to the Binance exchange for data retrieval.
*   **Remarks**: Should be kept confidential; ensure appropriate IP restrictions are set on the Binance dashboard.

### BinanceApiSecret
Gets or sets the secret key associated with the `BinanceApiKey` for signing requests.
*   **Type**: `string`
*   **Purpose**: Used to generate HMAC SHA256 signatures for authenticated API calls.
*   **Remarks**: Critical security credential; never log or expose this value.

### TelegramBotToken
Gets or sets the authentication token for the Telegram Bot API.
*   **Type**: `string`
*   **Purpose**: Allows the application to send messages via the configured Telegram bot.
*   **Remarks**: Obtained from @BotFather on Telegram.

### TelegramAdminChatId
Gets or sets the unique identifier of the chat (user or group) where administrative alerts and notifications are sent.
*   **Type**: `string`
*   **Purpose**: Targets specific recipients for critical system messages.
*   **Remarks**: Usually a numeric string; negative values indicate group chats.

### MonitoringIntervalSeconds
Gets or sets the frequency, in seconds, at which the system polls or analyzes market data.
*   **Type**: `int`
*   **Purpose**: Controls the heartbeat of the monitoring loop.
*   **Remarks**: Values too low may trigger API rate limits; values too high reduce data granularity.

### AlertCooldownMinutes
Gets or sets the minimum time interval, in minutes, that must pass before sending a subsequent alert of the same type for the same entity.
*   **Type**: `int`
*   **Purpose**: Prevents alert flooding during periods of high volatility.
*   **Remarks**: Must be greater than zero to be effective.

### MaxAlertsPerUser
Gets or sets the maximum number of alerts allowed to be sent to a specific user within a defined rolling window (implementation dependent).
*   **Type**: `int`
*   **Purpose**: Enforces rate limiting on a per-user basis to prevent abuse or notification fatigue.
*   **Remarks**: A value of zero may disable alerts for users depending on implementation logic.

### DefaultPriceChangeThreshold
Gets or sets the default percentage value used to trigger an alert when an asset's price changes by this amount.
*   **Type**: `decimal`
*   **Purpose**: Defines the sensitivity of price change detection.
*   **Remarks**: Represented as a percentage (e.g., `5.0m` for 5%).

### DefaultSpreadThreshold
Gets or sets the default minimum spread value (percentage or absolute, context-dependent) required to trigger a spread opportunity alert.
*   **Type**: `decimal`
*   **Purpose**: Filters out negligible arbitrage opportunities.
*   **Remarks**: Must be non-negative.

### HistoryRetentionDays
Gets or sets the number of days historical market data and alert logs are retained in the database.
*   **Type**: `int`
*   **Purpose**: Governs the automatic cleanup policy for older records.
*   **Remarks**: Used in conjunction with `EnableAutoCleanup`.

### MaxHistoryRecords
Gets or sets the absolute maximum number of records to keep in specific history tables, regardless of age.
*   **Type**: `int`
*   **Purpose**: Provides a hard cap on database growth for high-frequency data.
*   **Remarks**: Acts as a secondary limit to `HistoryRetentionDays`.

### DatabaseCommandTimeoutSeconds
Gets or sets the wait time in seconds before terminating a database command execution attempt.
*   **Type**: `int`
*   **Purpose**: Prevents indefinite hanging of the application during database latency or deadlocks.
*   **Remarks**: Should be set higher than the expected duration of complex analytical queries.

### SpreadAnalysisHistoryHours
Gets or sets the duration, in hours, of historical spread data considered during trend analysis.
*   **Type**: `int`
*   **Purpose**: Defines the lookback window for calculating moving averages or trends in spread data.
*   **Remarks**: Larger values provide smoother trends but may lag behind sudden market shifts.

### EnableWebSocket
Gets or sets a flag indicating whether the system should utilize WebSocket streams for real-time data instead of REST polling.
*   **Type**: `bool`
*   **Purpose**: Toggles between real-time push mechanisms and periodic polling.
*   **Remarks**: Enabling this usually requires `BinanceApiKey` configuration for private streams, though public streams may not require it.

### EnableTelegramNotifications
Gets or sets a flag to globally enable or disable the dispatch of notifications to Telegram.
*   **Type**: `bool`
*   **Purpose**: Allows quick disabling of Telegram alerts without removing configuration values.
*   **Remarks**: If false, the Telegram client service may be initialized in a no-op mode.

### EnableAutoCleanup
Gets or sets a flag to enable the background task responsible for deleting old historical records.
*   **Type**: `bool`
*   **Purpose**: Controls the execution of database maintenance routines.
*   **Remarks**: Disabling this requires manual intervention to manage database size.

### DailySummaryHourUtc
Gets or sets the hour in UTC (0-23) when the daily summary report is generated and sent.
*   **Type**: `int`
*   **Purpose**: Schedules the daily aggregation task.
*   **Remarks**: Must be within the range [0, 23].

### WebhookUrl
Gets or sets the endpoint URL to which JSON payloads are posted when webhook notifications are triggered.
*   **Type**: `string`
*   **Purpose**: Enables integration with external systems like Discord, Slack, or custom dashboards.
*   **Remarks**: Must be a valid absolute URI starting with `http://` or `https://`.

### EnableWebhookNotifications
Gets or sets a flag to globally enable or disable the dispatch of notifications to the configured `WebhookUrl`.
*   **Type**: `bool`
*   **Purpose**: Toggles webhook functionality independently of other notification channels.
*   **Remarks**: If true, `WebhookUrl` must be populated.

## Usage

### Example 1: Initializing Settings from Configuration
This example demonstrates populating the `AppSettings` instance typically performed during application startup using a configuration binder.

```csharp
using Microsoft.Extensions.Configuration;

public class Startup
{
    public AppSettings ConfigureSettings(IConfiguration configuration)
    {
        var settings = new AppSettings();
        
        // Bind configuration section "AppSettings" to the class properties
        configuration.GetSection("AppSettings").Bind(settings);

        // Validate critical dependencies manually if not using DataAnnotations
        if (string.IsNullOrWhiteSpace(settings.BinanceApiKey))
        {
            throw new InvalidOperationException("BinanceApiKey is required.");
        }

        return settings;
    }
}
```

### Example 2: Conditional Logic Based on Settings
This example shows a service method that adjusts its behavior based on the flags and thresholds defined in `AppSettings`.

```csharp
public class MarketMonitorService
{
    private readonly AppSettings _settings;

    public MarketMonitorService(AppSettings settings)
    {
        _settings = settings;
    }

    public async Task StartMonitoringAsync()
    {
        if (_settings.EnableWebSocket)
        {
            await InitializeWebSocketStreamAsync();
        }
        else
        {
            // Fallback to polling based on MonitoringIntervalSeconds
            StartPollingTimer(_settings.MonitoringIntervalSeconds * 1000);
        }

        if (_settings.EnableAutoCleanup)
        {
            ScheduleDatabaseCleanup(
                _settings.HistoryRetentionDays, 
                _settings.MaxHistoryRecords
            );
        }
    }

    private void StartPollingTimer(int intervalMs) 
    {
        // Implementation details for polling
    }
    
    private Task InitializeWebSocketStreamAsync() 
    {
        // Implementation details for WebSocket
        return Task.CompletedTask;
    }

    private void ScheduleDatabaseCleanup(int days, int maxRecords) 
    {
        // Implementation details for cleanup
    }
}
```

## Notes

*   **Thread Safety**: The `AppSettings` class consists entirely of mutable primitive types and strings. It is **not thread-safe** for write operations. It is intended to be instantiated once during application startup and treated as immutable during runtime. If properties must be updated at runtime, external synchronization (e.g., `lock` statements or `ReaderWriterLockSlim`) is required to prevent race conditions.
*   **Validation**: The class does not enforce validation constraints (such as range checks for `DailySummaryHourUtc` or URI format for `WebhookUrl`) internally. Consumers must ensure values are valid before use to prevent runtime exceptions in dependent services (e.g., `ArgumentOutOfRangeException` or `UriFormatException`).
*   **Sensitive Data**: Properties containing secrets (`BinanceApiSecret`, `TelegramBotToken`, `DatabaseConnectionString`) are stored as plain strings. Care should be taken to avoid logging instances of this class or serializing them to insecure storage.
*   **Zero Values**: Setting `MonitoringIntervalSeconds` to 0 or a negative value may cause tight loops or immediate timer failures in consuming services. Similarly, setting `MaxAlertsPerUser` to 0 might logically result in no alerts being sent, depending on the implementation of the alerting service.
