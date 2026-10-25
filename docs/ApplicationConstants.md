# ApplicationConstants

`src/Constants/ApplicationConstants.cs` (namespace `BinanceP2pMonitor.Constants`) holds application-wide constants and a few static collections. This document lists every member, its value, and what the code actually does with it.

The `BinanceP2pMonitor.Constants` namespace is imported globally (`src/GlobalUsings.cs`), but the class is referenced by name in only two places (`StatusCommand.cs`, `AlertCommand.cs`). The **Usage** column records what the repository really does with each member:

- **Used** - referenced as `ApplicationConstants.<Name>` in production code.
- **Unused** - no reference to the member exists in `src/`, `tests/` or `benchmarks/`. The value is documented here for reference; runtime behavior is driven by `AppSettings` or hard-coded values instead (see the notes).

## Application identity

| Member | Value | Usage | Purpose |
|---|---|---|---|
| `AppName` | `"Binance P2P Monitor"` | Unused | Display name of the application. |
| `Version` | `"1.0.0"` | Unused | Application version string. `VersionCommand` reads the assembly version and falls back to the literal `"1.0.0"`, not this constant. |
| `Author` | `"Vladyslav Zaiets"` | Unused | Author name. `VersionCommand` prints the same literal string instead of this constant. |

## Endpoints

| Member | Value | Usage | Purpose |
|---|---|---|---|
| `BinanceWebSocketUrl` | `wss://stream.binance.com:9443/ws` | Unused | Base URL of the Binance WebSocket market-data stream. |
| `BinanceP2pApiUrl` | `https://p2p.binance.com/api/v2/` | Unused | Base URL of the Binance P2P HTTP API. |

## Price monitoring

| Member | Value | Usage | Purpose |
|---|---|---|---|
| `DefaultMonitoringIntervalSeconds` | `30` | Unused | Intended default polling interval for price checks. The live default is `AppSettings.MonitoringIntervalSeconds = 30`. |
| `MaxPriceHistoryDays` | `90` | Unused | Intended upper bound on how many days of price history are kept. |
| `DefaultHistoryRetentionDays` | `30` | Unused | Intended default retention period for history. The live default is `AppSettings.HistoryRetentionDays = 30`. |

## Alert configuration

| Member | Value | Usage | Purpose |
|---|---|---|---|
| `DefaultCliUserId` | `1` | Used | User ID that all console-created alerts belong to. The CLI runs single-user. Used by `StatusCommand` (listing the user's alerts) and `AlertCommand` (listing alerts and setting `UserId` on new alerts). |
| `AlertCooldownMinutes` | `5` | Unused | Intended default cooldown between repeated alerts. The live default is `AppSettings.AlertCooldownMinutes = 5`. |
| `MaxAlertsPerUser` | `20` | Unused | Intended per-user alert limit. The live default is `AppSettings.MaxAlertsPerUser = 20`. |
| `DefaultPriceChangeThreshold` | `2` (percent) | Unused | Intended default price-change threshold. Note that `AppSettings.DefaultPriceChangeThreshold` is a `decimal` with value `2.0m`. |

## Spread analysis

| Member | Value | Usage | Purpose |
|---|---|---|---|
| `PercentageMultiplier` | `100m` | Unused | Converts a decimal ratio to a percentage. |
| `DefaultSpreadDecimalPlaces` | `4` | Unused | Decimal places kept in spread calculations. |
| `DefaultSpreadAnomalyZScoreThreshold` | `2.0m` | Unused | Default Z-score above which a spread is treated as anomalous. |
| `DefaultMaxSpreadThresholdPercent` | `5.0m` | Unused | Default maximum spread percentage accepted for spread alerts. |
| `DefaultSpreadThresholdPercent` | `1.5m` | Unused | Default spread threshold for alerts. The live default is `AppSettings.DefaultSpreadThreshold = 1.5m`. |
| `MinSpreadAlertPercent` | `0.1m` | Unused | Lower bound for a configurable spread-alert percentage. |
| `MaxSpreadAlertPercent` | `5.0m` | Unused | Upper bound for a configurable spread-alert percentage. |

## Display formatting

| Member | Value | Usage | Purpose |
|---|---|---|---|
| `DefaultDecimalDisplayPlaces` | `2` | Unused | Decimal places used when displaying monetary values. |
| `DefaultPriceDecimalPlaces` | `8` | Unused | Decimal places used when rounding stored price values. |

## Database

| Member | Value | Usage | Purpose |
|---|---|---|---|
| `DatabaseCommandTimeoutSeconds` | `30` | Unused | Intended SQL command timeout. The live default is `AppSettings.DatabaseCommandTimeoutSeconds = 30`. |
| `MaxDatabaseConnections` | `10` | Unused | Intended upper bound on database connections. |

## Telegram

| Member | Value | Usage | Purpose |
|---|---|---|---|
| `TelegramMaxMessageLength` | `4096` | Unused | Telegram Bot API limit on message length, in characters. |
| `TelegramReconnectIntervalSeconds` | `60` | Unused | Intended delay before reconnecting the Telegram client. |

## Performance

| Member | Value | Usage | Purpose |
|---|---|---|---|
| `BatchSizeForBulkOperations` | `1000` | Unused | Intended batch size for bulk database operations. |
| `MaxConcurrentRequests` | `5` | Unused | Intended cap on concurrent outbound requests. |

## Validation

| Member | Value | Usage | Purpose |
|---|---|---|---|
| `MinPriceValue` | `1` | Unused | Intended lower bound for a valid price. |
| `MaxPriceValue` | `1_000_000_000` | Unused | Intended upper bound for a valid price. |
| `MinPasswordLength` | `8` | Unused | Intended minimum password length. The CLI has no password feature. |
| `MaxUsernameLength` | `100` | Unused | Intended maximum username length. |

## Cache

| Member | Value | Usage | Purpose |
|---|---|---|---|
| `DefaultCacheTtlSeconds` | `30` | Unused | Default time-to-live, in seconds, for short-lived cached price data. |
| `CacheExpirationMinutes` | `15` | Unused | Intended expiration for cache entries. |
| `MaxCacheSize` | `1000` | Unused | Intended maximum number of cache entries. |

## Error handling and retries

| Member | Value | Usage | Purpose |
|---|---|---|---|
| `DefaultRetryCount` | `3` | Unused | Default number of retry attempts for transient operations. |
| `DefaultRetryDelayMilliseconds` | `1000` | Unused | Default initial delay, in milliseconds, between retry attempts. |
| `DefaultMaxRetryDelaySeconds` | `30` | Unused | Default upper bound, in seconds, on the delay between retries. |
| `DefaultRetryBackoffMultiplier` | `2.0` | Unused | Default factor applied to successive retry delays (exponential backoff). |
| `MaxRetryAttempts` | `3` | Unused | Intended maximum number of retry attempts. |
| `RetryDelayMilliseconds` | `1000` | Unused | Intended fixed delay, in milliseconds, between retries. |
| `MaxExceptionLogLength` | `2000` | Unused | Intended maximum length of an exception message written to logs. |

## Static collections

These are mutable `HashSet<string>` / `List<string>` instances declared `static readonly`. The reference is fixed, but the collection contents can still be changed at runtime. None of them are referenced by name in the repository.

| Member | Contents | Usage | Purpose |
|---|---|---|---|
| `SupportedAssets` | `BTC, ETH, BNB, USDT, USDC, XRP, SOL, ADA, DOGE, MATIC` | Unused | Crypto assets the application recognizes. Documented as extensible. |
| `SupportedFiats` | `USD, EUR, GBP, JPY, CNY, INR, AUD, CAD, SGD, HKD` | Unused | Fiat currencies the application recognizes. Documented as extensible. |
| `PopularPairs` | `BTC/USD, ETH/USD, BNB/USD, USDT/USD, BTC/EUR, ETH/EUR, BTC/GBP, ETH/GBP` | Unused | Commonly monitored trading pairs. |

## Notes

- **Mostly unreferenced.** Of the 43 constants and static collections, only `DefaultCliUserId` is referenced by name. The rest are either duplicated as literals or mirrored by `AppSettings` defaults, or are not used at all. Before relying on one of these values, check that code actually reads it.
- **Possible drift.** Several constants have a counterpart default in `src/Configuration/AppSettings.cs` (`MonitoringIntervalSeconds`, `AlertCooldownMinutes`, `MaxAlertsPerUser`, `DefaultPriceChangeThreshold`, `DefaultSpreadThreshold`, `HistoryRetentionDays`, `DatabaseCommandTimeoutSeconds`). The two sets of defaults can diverge, and the `AppSettings` values are the ones in effect.
- **Type differences.** `DefaultPriceChangeThreshold` is an `int` (`2`) here but a `decimal` (`2.0m`) in `AppSettings`. `DefaultRetryBackoffMultiplier` is a `double`, not a `decimal`.
