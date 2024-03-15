# AppSettingsExtensions

`AppSettingsExtensions` provides a centralized, static facade for reading configuration values used throughout the `binance-p2p-monitor` application. It abstracts the underlying configuration source (e.g., `IConfiguration`/`appsettings.json`) into strongly typed, purpose-specific members, allowing the rest of the application to consume settings without direct dependency on configuration keys or parsing logic.

## API

All members are `public static` and read-only properties or parameterless methods. They return configuration values that govern monitoring behavior, notification thresholds, and data retention.

### `IsTelegramConfigured`
**Type:** `bool` (property)

Returns `true` if all required Telegram Bot settings (token, chat ID, etc.) are present and non-empty in the configuration; otherwise `false`. Used as a guard before attempting to send Telegram alerts.

### `IsWebhookConfigured`
**Type:** `bool` (property)

Returns `true` if the webhook URL and related parameters are properly configured; otherwise `false`. Used to determine whether external webhook notifications can be dispatched.

### `GetMonitoringIntervalMs()`
**Returns:** `int`

The interval in milliseconds between successive P2P market data fetches. The returned value is always positive. A value that is too low may cause rate-limiting issues with the exchange API.

### `GetAlertCooldownSeconds()`
**Returns:** `int`

The minimum number of seconds that must elapse between consecutive alerts for the same asset/fiat pair. Prevents alert flooding during volatile periods. Returns a non-negative integer; zero disables the cooldown.

### `GetHistoryRetentionHours()`
**Returns:** `int`

The number of hours for which historical price and spread data is retained in the database before pruning. Returns a positive integer.

### `GetDatabaseCommandTimeoutMs()`
**Returns:** `int`

The command timeout in milliseconds applied to database operations. Returns a positive integer used to set `SqlCommand.CommandTimeout` or equivalent.

### `GetSpreadAnalysisHistoryPeriod()`
**Returns:** `int`

The lookback period in minutes used when calculating spread statistics (e.g., average spread, standard deviation) for analysis purposes. Returns a positive integer.

### `IsDailySummaryEnabled`
**Type:** `bool` (property)

Returns `true` if the daily summary report feature is enabled; otherwise `false`. When disabled, no daily summary is generated or sent, regardless of other related settings.

### `GetDailySummaryLocalHour()`
**Returns:** `int`

The hour of the day (0–23, local time) at which the daily summary report is triggered. Only meaningful when `IsDailySummaryEnabled` is `true`.

### `GetMaxAlertsPerCooldown()`
**Returns:** `int`

The maximum number of alerts that can be sent within a single cooldown window. Returns a positive integer. When the limit is reached, further alerts are suppressed until the cooldown expires.

### `GetPriceChangeThresholdPercent()`
**Returns:** `decimal`

The minimum price change (in percent) required to trigger a price-change alert. A value of `0` effectively disables price-change alerts. The returned decimal is non-negative.

### `GetSpreadThresholdPercent()`
**Returns:** `decimal`

The minimum spread (in percent) between buy and sell prices required to trigger a spread alert. A value of `0` effectively disables spread alerts. The returned decimal is non-negative.

### `GetMonitoredAssets()`
**Returns:** `List<string>`

The list of asset symbols (e.g., `"USDT"`, `"BTC"`) that the monitor actively tracks. Returns an empty list if no assets are configured, which effectively disables monitoring. The returned list is a new instance on each call, or is otherwise safe for enumeration.

### `GetMonitoredFiats()`
**Returns:** `List<string>`

The list of fiat currency symbols (e.g., `"ARS"`, `"VES"`) that the monitor tracks for each monitored asset. Returns an empty list if no fiats are configured. The returned list is a new instance on each call, or is otherwise safe for enumeration.

## Usage

### Example 1: Guarding notification dispatch

```csharp
if (AppSettingsExtensions.IsTelegramConfigured)
{
    await telegramService.SendMessageAsync(alertMessage);
}

if (AppSettingsExtensions.IsWebhookConfigured)
{
    await webhookService.PostAsync(alertPayload);
}
```

### Example 2: Building a monitoring loop with cooldown awareness

```csharp
var interval = AppSettingsExtensions.GetMonitoringIntervalMs();
var cooldownSeconds = AppSettingsExtensions.GetAlertCooldownSeconds();
var maxAlerts = AppSettingsExtensions.GetMaxAlertsPerCooldown();
var assets = AppSettingsExtensions.GetMonitoredAssets();
var fiats = AppSettingsExtensions.GetMonitoredFiats();

while (!cancellationToken.IsCancellationRequested)
{
    foreach (var asset in assets)
    {
        foreach (var fiat in fiats)
        {
            var spread = await p2pService.GetSpreadAsync(asset, fiat);
            if (spread >= AppSettingsExtensions.GetSpreadThresholdPercent())
            {
                alertManager.RaiseSpreadAlert(asset, fiat, spread, cooldownSeconds, maxAlerts);
            }
        }
    }

    await Task.Delay(interval, cancellationToken);
}
```

## Notes

- **Thread safety:** All members are static and read configuration from an underlying source that is assumed to be initialized once at startup and never mutated thereafter. They are safe to call concurrently from multiple threads without external synchronization.
- **Empty collections:** `GetMonitoredAssets()` and `GetMonitoredFiats()` return empty lists when the corresponding configuration sections are absent or empty. Callers should handle empty lists gracefully to avoid unnecessary work.
- **Zero thresholds:** `GetPriceChangeThresholdPercent()` and `GetSpreadThresholdPercent()` return `0` when the threshold key is missing or explicitly set to zero. Callers should treat zero as "alerting disabled" for that metric.
- **Cooldown interaction:** `GetMaxAlertsPerCooldown()` works in conjunction with `GetAlertCooldownSeconds()`. If the cooldown is zero, the per-cooldown alert cap is typically ignored by alert managers.
- **Configuration absence:** If the underlying configuration section is entirely missing, properties like `IsTelegramConfigured` and `IsWebhookConfigured` return `false`, and numeric getters return sensible defaults (typically `0` or a minimal positive value) as defined by the configuration binding layer. No exceptions are thrown for missing configuration.
