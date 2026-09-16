## Command Parsing

`src/CLI/CommandParser.cs` converts an already-tokenized `string[]` into a new `CommandContext`. The first token is copied verbatim to `CommandName`; when no tokens are supplied, the command name defaults to `help`. The supplied `IServiceProvider` is stored on the context.

Every token after the command name is classified as follows:

- `--key=value` adds `key` to `Options` with `value`; only the first `=` is used as the separator.
- `--key` adds `key` to `Options` with the string value `"true"`. Long options do not consume the following token.
- A two-character short option such as `-f` consumes the next token as its value when that token does not start with `-`, storing the pair in `Options`.
- A two-character short option without such a following value is stored in `Flags` with the string value `"true"`.
- Tokens that do not start with `-`, plus dash-prefixed numeric tokens such as `-5` and `-123`, become positional `Arguments`.
- Other dash-prefixed forms, such as `-ab` or `-x=value`, are ignored.

Options, flags, and positional arguments can be interspersed. Repeated keys overwrite earlier values in the same dictionary. The parser does not validate or normalize command and option names, split a raw command-line string, combine short flags, or treat `--` as an end-of-options marker. After parsing non-empty input, it writes a debug log containing the command name and the final option and flag counts.

## Compare Command

`compare` retrieves the current buy and sell prices for one asset in two fiat currencies and displays them side by side:

```text
binance-p2p-monitor compare --asset=BTC --from=USD --to=EUR
```

The `--asset`, `--from`, and `--to` options are required. Use `--format=table`, `--format=json`, or `--format=markdown` to select the comparison row's format; the default is `table`, and format names are matched case-insensitively. `-h` or `--help` displays the command help.

The formatted row contains the asset and both fiat names; buy price, sell price, spread, and buy-price change for each pair; absolute and percentage buy/sell differences; and the local time when the row was created. Prices and absolute differences are shown to eight decimal places, spreads to four decimal places, and changes and percentage differences to two decimal places.

After the formatted row, the command always writes an `Analysis` section. It reports:

- The absolute buy-price difference, calculated as `from buy price - to buy price`, and its percentage relative to the `to` buy price.
- Buy and sell price ratios, calculated as the corresponding `from` price divided by the `to` price.
- The best buy location (the fiat with the lower buy price) and best sell location (the fiat with the higher sell price). Ties select the `to` fiat.

If either price lookup returns no data, the command reports that one or both pairs could not be retrieved and exits with status `1`. Unsupported formats and unexpected errors also return `1`; a successful comparison returns `0`.

## Spread Command

`spread` displays the current buy/sell spread analysis for every available trading pair or for a filtered subset:

```text
binance-p2p-monitor spread
binance-p2p-monitor spread --asset=BTC
binance-p2p-monitor spread --fiat=USD
binance-p2p-monitor spread --pair=BTC/USD
binance-p2p-monitor spread --format=json
```

The command accepts these options:

- `--asset=ASSET` — includes only spreads whose asset matches the value, case-insensitively.
- `--fiat=FIAT` — includes only spreads whose fiat currency matches the value, case-insensitively. It can be combined with `--asset`.
- `--pair=ASSET/FIAT` — requests one pair directly. The separator may be `/` or `\`; surrounding whitespace is trimmed. When supplied, this option takes precedence over `--asset` and `--fiat`.
- `--format=FORMAT` — selects `table`, `json`, or `markdown`; the default is `table`, and format names are case-insensitive.
- `-h, --help` — shows the command help.

Each formatted result contains the asset, fiat, pair, current/average/minimum/maximum spread, standard deviation, sample count, risk level, percentage variance from the average, high- and low-spread indicators, and a human-readable last-updated time. Spread values and standard deviation are rendered to four decimal places; variance is rendered to two decimal places. Risk levels are based on the current spread (`Very Low` below 0.3%, `Low` below 0.6%, `Medium` below 1.0%, `High` below 1.5%, and `Very High` otherwise). The high indicator uses a fixed threshold above 1.5%, while the low indicator uses a fixed threshold below 0.3%.

After successful formatted output, the command prints a `Configuration` section containing `DefaultSpreadThreshold` and `SpreadAnalysisHistoryHours`. These settings describe the configured analysis, but the displayed high/low indicators use the fixed model thresholds described above.

With no filters, or with only `--asset` and/or `--fiat`, the command retrieves all spread analyses and filters them locally. With `--pair`, it performs a single-pair lookup. No matching data is informational and returns exit code `0`; an invalid pair, unsupported format, or unexpected error returns `1`. A successful result returns `0`.

## Output Formatters

The implementations in `src/Formatters/` share the `IOutputFormatter` interface. Each exposes a `FormatType` and can format a single object, a collection, or a collection with explicit headers:

- `CsvOutputFormatter` (`csv`) uses public property names as the default header row, emits one row per object, formats `IFormattable` values with the invariant culture, and applies CSV quoting and quote escaping. A null single value or an empty inferred collection produces an empty string.
- `JsonOutputFormatter` (`json`) uses indented `System.Text.Json` output. Collections become JSON arrays; the explicit-header overload produces an object containing `headers` and `data`. A null single value produces `null`, and serialization failures produce a JSON object with an `error` property.
- `MarkdownOutputFormatter` (`markdown`) renders public properties as a Markdown table. Values are converted with `ToString()`, null properties appear as `(null)`, and cells are truncated to 50 characters. A null single value produces `(empty)`, while an empty collection produces `(no data)`.
- `TableOutputFormatter` (`table`) renders the same reflected properties, null marker, and 50-character cell truncation as an ASCII table with borders. Its null and empty results are also `(empty)` and `(no data)` respectively.

Users select a formatter with `--format=FORMAT`, for example `binance-p2p-monitor history --asset=BTC --fiat=USDT --format=csv`. `monitor` and `history` accept `table`, `json`, `csv`, and `markdown` (default `table`). `spread`, `compare`, and `summary` accept `table`, `json`, and `markdown` (default `table`). `export` accepts `csv`, `json`, and `markdown` (default `csv`). The application registers all four formatters and these commands look up the requested value by `FormatType`; lookups during execution are case-insensitive, although `monitor` and `history` validate their format values case-sensitively.

## UtilityExtensionsTests

The `UtilityExtensionsTests` class provides comprehensive unit tests for various utility extension methods used throughout the application, including date/time manipulation, enumerable processing, numeric calculations, string formatting, and data validation helpers. These tests ensure the reliability and correct behavior of these foundational extension methods.

```csharp
using BinanceP2pMonitor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

// DateTime extension usage
var now = DateTime.UtcNow;
var unixTimestamp = now.ToUnixTimestampMs();
Console.WriteLine($"Unix timestamp (ms): {unixTimestamp}");

var timeAgo = now.AddMinutes(-5).GetTimeAgoString();
Console.WriteLine($"Time ago: {timeAgo}"); // "5m ago"

// Enumerable extension usage
var items = new List<int> { 1, 2, 3, 4, 5 };
var chunks = Enumerable.Chunk(items, 2).ToList();
Console.WriteLine($"Chunks count: {chunks.Count}");

var firstItem = items.FirstOrNull();
Console.WriteLine($"First item: {firstItem}");

// Numeric extension usage
var rounded = 123.456m.RoundTo(2);
Console.WriteLine($"Rounded: {rounded}"); // 123.46

var percentageChange = 110m.CalculatePercentageChange(100m);
Console.WriteLine($"Percentage change: {percentageChange}%"); // 10%

// String extension usage
var truncated = "LongStringExample".Truncate(5);
Console.WriteLine($"Truncated: {truncated}"); // "Lo..."

var snakeCase = "PascalCaseString".ToSnakeCase();
Console.WriteLine($"Snake case: {snakeCase}"); // "pascal_case_string"

// Validation helper usage
var isValidEmail = ValidationHelper.IsValidEmail("test@example.com");
Console.WriteLine($"Valid email: {isValidEmail}");

var isValidTicker = ValidationHelper.IsValidTicker("USDT");
Console.WriteLine($"Valid ticker: {isValidTicker}");
```

## LoggingExtensionsValidation

The `LoggingExtensionsValidation` class provides validation helpers for logging configuration and parameters to ensure correct usage before actual logging occurs. It contains extension methods for validating `ILoggingBuilder`, `ILogger`, and various logging parameters, returning lists of validation problems or boolean validity checks.

## ApiResponseJsonExtensions

The `ApiResponseJsonExtensions` class provides JSON serialization and deserialization extensions for `ApiResponse` and `ApiResponse<T>` types. It simplifies converting API response objects to/from JSON strings with camelCase property naming and configurable formatting. The extensions handle both generic and non-generic response types.

```csharp
using BinanceP2pMonitor.Infrastructure;
using System;

// Example API response data
var response = new ApiResponse
{
    Code = "000000",
    Message = "Success",
    Timestamp = DateTime.UtcNow,
    Data = null
};

// Serialize to JSON string
string json = response.ToJson(); // Compact JSON
string prettyJson = response.ToJson(indented: true); // Pretty-printed JSON

// Deserialize from JSON string
ApiResponse? deserialized = ApiResponseJsonExtensions.FromJson(json);

// Try to deserialize with error handling
if (ApiResponseJsonExtensions.TryFromJson(json, out var tryDeserialized))
{
    Console.WriteLine($"Successfully deserialized: {tryDeserialized?.Code}");
}

// Example with generic ApiResponse<T>
var genericResponse = new ApiResponse<string[]>
{
    Code = "000000",
    Message = "Success",
    Timestamp = DateTime.UtcNow,
    Data = new[] { "BTC", "USDT", "ETH" }
};

// Serialize generic response
string genericJson = genericResponse.ToJson();

// Deserialize generic response
ApiResponse<string[]>? genericDeserialized = ApiResponseJsonExtensions.FromJson<string[]>(genericJson);
```

## PriceUpdatedEventExtensions

The `PriceUpdatedEventExtensions` class provides extension methods for working with `PriceUpdatedEvent` objects, offering convenient ways to analyze price updates, calculate changes, and check market conditions. These extensions help monitor price movements, spread analysis, and offer availability tracking.

```csharp
using BinanceP2pMonitor.Events;
using System;

// Create a price update event
var priceEvent = new PriceUpdatedEvent
{
    Asset = "USDT",
    Fiat = "USDT",
    BuyPrice = 1.005m,
    SellPrice = 1.01m,
    PreviousBuyPrice = 1.00m,
    PreviousSellPrice = 1.005m,
    BuyOfferCount = 5,
    SellOfferCount = 3
};

// Example 1: Get buy price change percentage
decimal buyChange = priceEvent.GetBuyPriceChangePercentage();
Console.WriteLine($"Buy price change: {buyChange:F2}%"); // 0.50%

// Example 2: Get sell price change percentage
decimal sellChange = priceEvent.GetSellPriceChangePercentage();
Console.WriteLine($"Sell price change: {sellChange:F2}%"); // 0.50%

// Example 3: Check if buy price increased
bool buyIncreased = priceEvent.HasBuyPriceIncreased();
Console.WriteLine($"Buy price increased: {buyIncreased}"); // True

// Example 4: Check if sell price increased
bool sellIncreased = priceEvent.HasSellPriceIncreased();
Console.WriteLine($"Sell price increased: {sellIncreased}"); // True

// Example 5: Get the trading pair
string pair = priceEvent.GetPair();
Console.WriteLine($"Trading pair: {pair}"); // "USDT/USDT"

// Example 6: Calculate price spread
decimal spread = priceEvent.GetPriceSpread();
Console.WriteLine($"Price spread: {spread:F4}"); // 0.0050

// Example 7: Check if spread exceeds threshold
bool spreadTooHigh = priceEvent.HasSpreadExceededThreshold(0.01m);
Console.WriteLine($"Spread too high: {spreadTooHigh}"); // False

// Example 8: Check for active buy offers
bool hasBuyOffers = priceEvent.HasActiveBuyOffers();
Console.WriteLine($"Has buy offers: {hasBuyOffers}"); // True

// Example 9: Check for active sell offers
bool hasSellOffers = priceEvent.HasActiveSellOffers();
Console.WriteLine($"Has sell offers: {hasSellOffers}"); // True

// Example 10: Get formatted offer counts
string offerCounts = priceEvent.GetOfferCountsSummary();
Console.WriteLine($"Offer counts: {offerCounts}"); // "Buy: 5 | Sell: 3"

// Example 11: Create a deep copy
var priceEventCopy = priceEvent.DeepCopy();
Console.WriteLine($"Original and copy are equal: {priceEvent.Asset == priceEventCopy.Asset}"); // True

// Example 12: Check for significant price movement
bool significantMovement = priceEvent.HasSignificantPriceMovement(1.0m);
Console.WriteLine($"Significant movement (>1%): {significantMovement}"); // False
```

## BacktestOptionsExtensions

The `BacktestOptionsExtensions` class provides extension methods for the `BacktestOptions` class, offering convenient calculations and decision logic for backtesting scenarios. These extensions help determine position sizing, stop loss/take profit triggers, and transaction costs based on configured thresholds and percentages.

```csharp
using BinanceP2pMonitor.Backtesting;
using System;

// Example backtest options configuration
var options = new BacktestOptions
{
    InitialEquity = 10000m,
    PositionSizeFraction = 0.1m, // 10% of equity
    EntrySpreadThreshold = 0.5m, // 0.5% entry threshold
    ExitSpreadThreshold = 0.2m,  // 0.2% exit threshold
    StopLossPercent = 0.1m,      // 0.1% stop loss
    TakeProfitPercent = 0.3m,     // 0.3% take profit
    TransactionCostPercent = 0.1m // 0.1% transaction cost
};

// Calculate maximum position size based on initial equity and position size fraction
decimal maxPositionSize = options.CalculateMaxPositionSize();
Console.WriteLine($"Maximum position size: {maxPositionSize:C}"); // $1000.00

// Determine if stop loss should trigger based on current spread
bool shouldStopLoss = options.ShouldTriggerStopLoss(0.3m); // current spread = 0.3%
Console.WriteLine($"Should trigger stop loss: {shouldStopLoss}"); // False (0.3 > 0.5 - 0.1)

// Determine if take profit should trigger based on current spread
bool shouldTakeProfit = options.ShouldTriggerTakeProfit(0.6m); // current spread = 0.6%
Console.WriteLine($"Should trigger take profit: {shouldTakeProfit}"); // True (0.6 >= 0.2 + 0.3)

// Check if either stop loss or take profit should trigger
bool shouldExit = options.ShouldTriggerStopLossOrTakeProfit(0.6m);
Console.WriteLine($"Should exit position: {shouldExit}"); // True

// Calculate transaction cost for a position
decimal positionSize = 500m;
decimal transactionCost = options.CalculateTransactionCost(positionSize);
Console.WriteLine($"Transaction cost for {positionSize:C}: {transactionCost:C}"); // $0.50
```

## RetryPolicyExtensions

`RetryPolicyExtensions` adds convenient helpers for executing asynchronous operations with a `RetryPolicy`. The extensions support both value‑returning and void‑returning tasks, optional cancellation tokens, and provide a method to check whether an exception is considered transient and therefore retryable.

```csharp
using BinanceP2pMonitor.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;

// Create a retry policy (uses default settings)
var policy = new RetryPolicy();

// Example 1: Execute a function that returns a value
int result = await policy.ExecuteWithRetryAsync(async () =>
{
    // Simulate work that might fail transiently
    await Task.Delay(100);
    return 42;
});
Console.WriteLine($"Result: {result}");

// Example 2: Execute a function that returns no value
await policy.ExecuteWithRetryAsync(async () =>
{
    // Simulate fire‑and‑forget work
    await Task.Delay(50);
});

// Example 3: Execute with an explicit CancellationToken
CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
string data = await policy.ExecuteWithRetryAsync(
    async ct => 
    {
        await Task.Delay(200, ct);
        return "completed";
    },
    cancellationToken: cts.Token);
Console.WriteLine(data);

// Example 4: Check if an exception is retryable
bool shouldRetry = policy.IsRetryableException(new TimeoutException());
Console.WriteLine($"Should retry on TimeoutException: {shouldRetry}");
```

```csharp
using BinanceP2pMonitor.Models;
using System;
using System.Collections.Generic;

// Example price history records
var priceHistory = new List<PriceHistory>
{
    new PriceHistory
    {
        Asset = "USDT",
        Fiat = "USDT",
        Timestamp = DateTime.UtcNow.AddHours(-10),
        BuyPrice = 1.005m,
        SellPrice = 1.01m,
        MidPrice = 1.0075m
    },
    new PriceHistory
    {
        Asset = "USDT",
        Fiat = "USDT",
        Timestamp = DateTime.UtcNow.AddHours(-5),
        BuyPrice = 1.01m,
        SellPrice = 1.015m,
        MidPrice = 1.0125m
    },
    new PriceHistory
    {
        Asset = "USDT",
        Fiat = "USDT",
        Timestamp = DateTime.UtcNow.AddHours(-1),
        BuyPrice = 1.015m,
        SellPrice = 1.02m,
        MidPrice = 1.0175m
    }
};

// Example 1: Filter price history to a specific time range
var recentPrices = priceHistory.GetInTimeRange(DateTime.UtcNow.AddHours(-6), DateTime.UtcNow);
Console.WriteLine($"Prices in last 6 hours: {recentPrices.Count}"); // 2

// Example 2: Check if a price record is older than a specific time threshold
bool isOlder = priceHistory[0].IsOlderThan(TimeSpan.FromHours(8));
Console.WriteLine($"First price is older than 8 hours: {isOlder}"); // True

// Example 3: Get only recent price history records (within last 2 hours)
var recentOnly = priceHistory.WhereRecent(TimeSpan.FromHours(2));
Console.WriteLine($"Recent prices (last 2h): {recentOnly.Count}"); // 1

// Example 4: Calculate average price change percentage over a time period
var averageChange = priceHistory.CalculateAveragePriceChangePercentage(TimeSpan.FromHours(10));
Console.WriteLine($"Average price change over 10h: {averageChange:F2}%"); // ~1.23%

// Example 5: Calculate average spread percentage across all price records
var averageSpread = priceHistory.CalculateAverageSpreadPercentage();
Console.WriteLine($"Average spread: {averageSpread:F2}%"); // ~0.49%

// Example 6: Calculate average mid price across a time range
var avgMidPrice = priceHistory.CalculateAverageMidPrice(DateTime.UtcNow.AddHours(-10), DateTime.UtcNow);
Console.WriteLine($"Average mid price: {avgMidPrice:F4}"); // 1.0125
```

## HistoricalSpreadAnalysisExtensions

The `HistoricalSpreadAnalysisExtensions` class provides extension methods for registering historical spread analysis services with the dependency injection container and analyzing spread statistics reports. These extensions help monitor spread anomalies, volatility, trends, and critical conditions across different time windows.

```csharp
using BinanceP2pMonitor.Extensions;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

// Example 1: Register historical spread analysis services
var services = new ServiceCollection();

// Register required dependencies
services.AddScoped<IHistoryRepository, HistoryRepository>();
services.AddScoped<ISpreadAnalysisService, SpreadAnalysisService>();
services.AddScoped<IEventBus, EventBus>();

// Configure app settings
var appSettings = new AppSettings { /* your settings */ };
services.AddSingleton(appSettings);

// Register historical spread analysis
services.AddHistoricalSpreadAnalysis();

var serviceProvider = services.BuildServiceProvider();

// Example 2: Analyze spread statistics
var report = new SpreadStatisticsReport
{
    Asset = "USDT",
    Fiat = "USDT",
    TimeWindowHours = 24,
    AnalyzedAt = DateTime.UtcNow,
    SampleCount = 1000,
    CurrentSpread = 0.45m,
    Mean = 0.35m,
    StandardDeviation = 0.12m,
    Median = 0.34m,
    MinSpread = 0.10m,
    MaxSpread = 1.20m,
    Percentile5 = 0.18m,
    Percentile95 = 0.60m,
    ZScore = 2.8m,
    TrendSlope = -0.000123m
};

// Format as human-readable summary
string summary = report.ToSummaryString();
Console.WriteLine(summary);

// Check if spread is critically anomalous
bool isCritical = report.IsCritical();
Console.WriteLine($"Is critical spread: {isCritical}"); // False (Z-score < 3.0)

// Check if current spread is above historical average
bool isAboveAverage = report.IsAboveAverage();
Console.WriteLine($"Is above average: {isAboveAverage}"); // True (0.45 > 0.35)

// Get volatility range (IQR width)
decimal volatilityRange = report.GetVolatilityRange();
Console.WriteLine($"Volatility range (IQR): {volatilityRange:F4}%"); // 0.42%
```

```csharp
using BinanceP2pMonitor.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;

// Create a retry policy (uses default settings)
var policy = new RetryPolicy();

// Example 1: Execute a function that returns a value
int result = await policy.ExecuteWithRetryAsync(async () =>
{
    // Simulate work that might fail transiently
    await Task.Delay(100);
    return 42;
});
Console.WriteLine($"Result: {result}");

// Example 2: Execute a function that returns no value
await policy.ExecuteWithRetryAsync(async () =>
{
    // Simulate fire‑and‑forget work
    await Task.Delay(50);
});

// Example 3: Execute with an explicit CancellationToken
CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
string data = await policy.ExecuteWithRetryAsync(
    async ct =>
    {
        await Task.Delay(200, ct);
        return "completed";
    },
    cancellationToken: cts.Token);
Console.WriteLine(data);

// Example 4: Check if an exception is retryable
bool shouldRetry = policy.IsRetryableException(new TimeoutException());
Console.WriteLine($"Should retry on TimeoutException: {shouldRetry}");
```

```csharp
using BinanceP2pMonitor.Models;
using System;
using System.Collections.Generic;

// Example price history records
var priceHistory = new List<PriceHistory>
{
    new PriceHistory
    {
        Asset = "USDT",
        Fiat = "USDT",
        Timestamp = DateTime.UtcNow.AddHours(-10),
        BuyPrice = 1.005m,
        SellPrice = 1.01m,
        MidPrice = 1.0075m
    },
    new PriceHistory
    {
        Asset = "USDT",
        Fiat = "USDT",
        Timestamp = DateTime.UtcNow.AddHours(-5),
        BuyPrice = 1.01m,
        SellPrice = 1.015m,
        MidPrice = 1.0125m
    },
    new PriceHistory
    {
        Asset = "USDT",
        Fiat = "USDT",
        Timestamp = DateTime.UtcNow.AddHours(-1),
        BuyPrice = 1.015m,
        SellPrice = 1.02m,
        MidPrice = 1.0175m
    }
};

// Example 1: Filter price history to a specific time range
var recentPrices = priceHistory.GetInTimeRange(DateTime.UtcNow.AddHours(-6), DateTime.UtcNow);
Console.WriteLine($"Prices in last 6 hours: {recentPrices.Count}"); // 2

// Example 2: Check if a price record is older than a specific time threshold
bool isOlder = priceHistory[0].IsOlderThan(TimeSpan.FromHours(8));
Console.WriteLine($"First price is older than 8 hours: {isOlder}"); // True

// Example 3: Get only recent price history records (within last 2 hours)
var recentOnly = priceHistory.WhereRecent(TimeSpan.FromHours(2));
Console.WriteLine($"Recent prices (last 2h): {recentOnly.Count}"); // 1

// Example 4: Calculate average price change percentage over a time period
var averageChange = priceHistory.CalculateAveragePriceChangePercentage(TimeSpan.FromHours(10));
Console.WriteLine($"Average price change over 10h: {averageChange:F2}%"); // ~1.23%

// Example 5: Calculate average spread percentage across all price records
var averageSpread = priceHistory.CalculateAverageSpreadPercentage();
Console.WriteLine($"Average spread: {averageSpread:F2}%"); // ~0.49%

// Example 6: Calculate average mid price across a time range
var avgMidPrice = priceHistory.CalculateAverageMidPrice(DateTime.UtcNow.AddHours(-10), DateTime.UtcNow);
Console.WriteLine($"Average mid price: {avgMidPrice:F4}"); // 1.0125
```

## PriceHistoryExtensions

The `PriceHistoryExtensions` class provides utility methods for working with historical price data, enabling time-based filtering, spread analysis, and price change calculations across different time windows. These extensions help analyze price trends, calculate average changes, and filter price history records based on temporal criteria.

## Telegram Notification Client

The `TelegramNotificationClient` (`src/Integration/TelegramNotificationClient.cs`) implements the `ITelegramNotificationClient` interface for sending notifications via Telegram. It provides methods for sending regular messages, price alerts, and rate-limited messages to a configured Telegram chat.

### Configuration

The client requires the following settings in `AppSettings`:
- `TelegramBotToken`: The bot token obtained from @BotFather on Telegram
- `TelegramAdminChatId`: The numeric chat ID where notifications will be sent
- `EnableTelegramNotifications`: Boolean flag to enable/disable Telegram notifications

These settings are validated at startup to ensure they are properly configured when notifications are enabled.

### Usage

The client is registered as a singleton service in the dependency injection container:

```csharp
services.AddSingleton<ITelegramNotificationClient, TelegramNotificationClient>();
```

It can be injected into any service or class that requires Telegram notifications:

```csharp
public class MyService
{
    private readonly ITelegramNotificationClient _telegramClient;
    
    public MyService(ITelegramNotificationClient telegramClient)
    {
        _telegramClient = telegramClient;
    }
}
```

### Methods

#### SendMessageAsync
Sends a plain text message to a specified chat ID:
```csharp
await _telegramClient.SendMessageAsync(chatId, "Hello World!", cancellationToken);
```

#### SendPriceAlertAsync
Sends a formatted price alert to the configured admin chat:
```csharp
await _telegramClient.SendPriceAlertAsync(
    "BTC", 
    "USDT", 
    45000.00m, 
    45500.00m, 
    "Price spread exceeded threshold", 
    cancellationToken);
```

#### SendRateLimitedAsync
Sends a message with rate limiting to prevent spam:
```csharp
await _telegramClient.SendRateLimitedAsync(
    "daily_summary_key", 
    "Daily market summary", 
    TimeSpan.FromHours(24), 
    cancellationToken);
```

All methods return `true` if the message was sent successfully, `false` otherwise. Messages are formatted using HTML parse mode for rich text formatting.

## Webhook Notification Client

The `WebhookNotificationClient` (`src/Integration/WebhookNotificationClient.cs`) implements the `IWebhookNotificationClient` interface for sending HTTP POST notifications to a configured webhook endpoint. It provides methods for sending generic alerts and price alerts as JSON payloads.

### Configuration

The client requires the following settings in `AppSettings`:
- `WebhookUrl`: The HTTP endpoint URL that will receive JSON POST requests on alerts
- `EnableWebhookNotifications`: Boolean flag to enable/disable webhook notifications

These settings are validated at startup to ensure the webhook URL is configured when notifications are enabled.

### Usage

The client is registered as a singleton service in the dependency injection container:

```csharp
services.AddSingleton<IWebhookNotificationClient, WebhookNotificationClient>();
```

It can be injected into any service or class that requires webhook notifications:

```csharp
public class MyService
{
    private readonly IWebhookNotificationClient _webhookClient;
    
    public MyService(IWebhookNotificationClient webhookClient)
    {
        _webhookClient = webhookClient;
    }
}
```

### Methods

#### SendAlertAsync
Posts a generic alert payload to the configured webhook URL:
```csharp
await _webhookClient.SendAlertAsync(new WebhookPayload
{
    Event = "custom_event",
    Asset = "BTC",
    Fiat = "USDT",
    BuyPrice = 45000.00m,
    SellPrice = 45500.00m,
    AlertReason = "Price threshold breached",
    CustomData = "additional info"
}, cancellationToken);
```

#### SendPriceAlertAsync
Convenience overload for price-alert events:
```csharp
await _webhookClient.SendPriceAlertAsync(
    "BTC", 
    "USDT", 
    45000.00m, 
    45500.00m, 
    "Price spread exceeded threshold", 
    cancellationToken);
```

Both methods return `true` if the webhook endpoint responded with an HTTP 2xx status code, `false` otherwise. The JSON payload is serialized with camelCase property naming and includes the following fields:
- `Event`: The type of alert (defaults to "alert")
- `Asset`: The asset symbol (e.g., "BTC")
- `Fiat`: The fiat currency (e.g., "USDT")
- `BuyPrice`: The buy price as decimal
- `SellPrice`: The sell price as decimal
- `AlertReason`: Description of why the alert was triggered
- `Timestamp`: UTC timestamp when the alert was generated
- `CustomData`: Optional additional data string

## DatabaseCleanupService

The `DatabaseCleanupService` (`src/Services/DatabaseCleanupService.cs`) is responsible for maintaining database hygiene by removing outdated records. It implements the `IDatabaseCleanupService` interface and provides two primary methods:

1. **DeleteOldRecordsAsync(int daysOld)** - Deletes history records older than the specified number of days and returns the count of deleted records
2. **GetTotalHistoryCountAsync()** - Returns the total number of history records currently in the database

The service works by:
- Recording the initial count of history records
- Calling the repository to delete records older than the specified threshold
- Recording the remaining count after deletion
- Calculating and returning the difference as the number of deleted records
- Logging all operations for monitoring and debugging

This service is typically used in scheduled maintenance tasks to prevent the database from growing indefinitely with historical price data that is no longer needed for analysis or reporting.

## PriceAlertTestsValidation

The `PriceAlertTestsValidation` class provides validation utilities for price alert tests, ensuring test data and configurations are valid before execution. It offers methods to validate alert conditions, thresholds, and test scenarios, returning lists of validation problems or boolean validity checks. The class helps maintain test reliability by catching invalid configurations early.

```csharp
using BinanceP2pMonitor.Tests; // or your test project namespace
using System;
using System.Collections.Generic;

// Example 1: Validate a price alert configuration
var alertConfig = new PriceAlertConfiguration
{
    Asset = "USDT",
    Fiat = "USDT",
    PriceThreshold = 1.02m,
    Direction = PriceAlertDirection.Above,
    ComparisonTolerance = 0.001m
};

// Validate the configuration
var validationErrors = PriceAlertTestsValidation.Validate(alertConfig);
if (validationErrors.Count == 0)
{
    Console.WriteLine("Configuration is valid!");
}
else
{
    Console.WriteLine("Validation errors:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Example 2: Check if a configuration is valid without collecting errors
bool isValid = PriceAlertTestsValidation.IsValid(alertConfig);
Console.WriteLine($"Is valid: {isValid}");

// Example 3: Ensure a configuration is valid (throws if invalid)
try
{
    PriceAlertTestsValidation.EnsureValid(alertConfig);
    Console.WriteLine("Configuration passed validation!");
}
catch (Exception ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}

// Example 4: Validate multiple configurations
var configs = new List<PriceAlertConfiguration>
{
    new PriceAlertConfiguration { Asset = "USDT", Fiat = "USDT", PriceThreshold = 1.02m },
    new PriceAlertConfiguration { Asset = "BTC", Fiat = "USDT", PriceThreshold = 50000m },
    new PriceAlertConfiguration { Asset = "ETH", Fiat = "USDT", PriceThreshold = 3000m }
};

foreach (var config in configs)
{
    var errors = PriceAlertTestsValidation.Validate(config);
    if (errors.Count == 0)
    {
        Console.WriteLine($"Config for {config.Asset}/{config.Fiat} is valid");
    }
}

// Example 5: Validate with custom tolerance
var customConfig = new PriceAlertConfiguration
{
    Asset = "USDT",
    Fiat = "USDT",
    PriceThreshold = 1.015m,
    Direction = PriceAlertDirection.Below,
    ComparisonTolerance = 0.01m // 1% tolerance
};

var customErrors = PriceAlertTestsValidation.Validate(customConfig);
Console.WriteLine($"Custom tolerance validation: {customErrors.Count} errors");
```

## ValidationResult

The `ValidationResult` record provides static validation methods for various data types used throughout the Binance P2P monitoring system. These methods validate user inputs, configuration values, and API parameters to ensure data integrity and prevent invalid operations. The validation methods return boolean values indicating whether the input meets the required format and constraints.

```csharp
using BinanceP2pMonitor.Utilities;
using System;

// Example 1: Validate email addresses
bool isValidEmail = ValidationResult.IsValidEmail("user@example.com");
Console.WriteLine($"Valid email: {isValidEmail}"); // True

bool isInvalidEmail = ValidationResult.IsValidEmail("invalid-email");
Console.WriteLine($"Invalid email: {isInvalidEmail}"); // False

// Example 2: Validate trading pair tickers
bool isValidTicker = ValidationResult.IsValidTicker("USDT");
Console.WriteLine($"Valid ticker: {isValidTicker}"); // True

bool isInvalidTicker = ValidationResult.IsValidTicker("XYZ123");
Console.WriteLine($"Invalid ticker: {isInvalidTicker}"); // False

// Example 3: Validate fiat currency codes
bool isValidFiat = ValidationResult.IsValidFiatCode("USD");
Console.WriteLine($"Valid fiat code: {isValidFiat}"); // True

bool isInvalidFiat = ValidationResult.IsValidFiatCode("XYZ");
Console.WriteLine($"Invalid fiat code: {isInvalidFiat}"); // False

// Example 4: Validate price values
bool isValidPrice = ValidationResult.IsValidPrice(123.45m);
Console.WriteLine($"Valid price: {isValidPrice}"); // True

bool isInvalidPrice = ValidationResult.IsValidPrice(-100m);
Console.WriteLine($"Invalid price: {isInvalidPrice}"); // False

// Example 5: Validate threshold values (must be positive)
bool isValidThreshold = ValidationResult.IsValidThreshold(0.5m);
Console.WriteLine($"Valid threshold: {isValidThreshold}"); // True

bool isInvalidThreshold = ValidationResult.IsValidThreshold(-0.1m);
Console.WriteLine($"Invalid threshold: {isInvalidThreshold}"); // False

// Example 6: Validate Telegram chat IDs
bool isValidChatId = ValidationResult.IsValidTelegramChatId("-1001234567890");
Console.WriteLine($"Valid Telegram chat ID: {isValidChatId}"); // True

bool isInvalidChatId = ValidationResult.IsValidTelegramChatId("invalid");
Console.WriteLine($"Invalid Telegram chat ID: {isInvalidChatId}"); // False

// Example 7: Validate date ranges
bool isValidDateRange = ValidationResult.IsValidDateRange(
    DateTime.UtcNow.AddDays(-7),
    DateTime.UtcNow
);
Console.WriteLine($"Valid date range: {isValidDateRange}"); // True

bool isInvalidDateRange = ValidationResult.IsValidDateRange(
    DateTime.UtcNow,
    DateTime.UtcNow.AddDays(-1)
);
Console.WriteLine($"Invalid date range: {isInvalidDateRange}"); // False

// Example 8: Validate collections (non-null and non-empty)
bool isValidCollection = ValidationResult.IsValidCollection(new[] { 1, 2, 3 });
Console.WriteLine($"Valid collection: {isValidCollection}"); // True

bool isInvalidCollection = ValidationResult.IsValidCollection(Array.Empty<int>());
Console.WriteLine($"Invalid collection: {isInvalidCollection}"); // False

// Example 9: Validate decimal precision (number of decimal places)
bool isValidPrecision = ValidationResult.IsValidPrecision(2);
Console.WriteLine($"Valid precision: {isValidPrecision}"); // True

bool isInvalidPrecision = ValidationResult.IsValidPrecision(-1);
Console.WriteLine($"Invalid precision: {isInvalidPrecision}"); // False

// Example 10: Validate strings against regex patterns
bool matchesPattern = ValidationResult.MatchesPattern("BTCUSDT", "^[A-Z]{3,6}$");
Console.WriteLine($"Matches pattern: {matchesPattern}"); // True

bool doesNotMatch = ValidationResult.MatchesPattern("btc_usdt", "^[A-Z]{3,6}$");
Console.WriteLine($"Does not match: {doesNotMatch}"); // False
```

## PriceCalculatorEdgeCaseTestsExtensions

`PriceCalculatorEdgeCaseTestsExtensions` supplies a collection of helper methods and test data generators for edge‑case testing of price‑related calculations. The extensions create TheoryData for percentage‑change, spread, moving‑average, and standard‑deviation tests, and provide validation helpers that assert correct exception handling and zero‑result behavior.

```csharp
using BinanceP2pMonitor.Tests;
using BinanceP2pMonitor.Utilities;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;

// Create an instance of the test class (the class itself is defined elsewhere)
var test = new PriceCalculatorEdgeCaseTests();

// Generate test data for percentage‑change calculations
var pctChangeData = test.CreatePercentageChangeTestData();
var spreadData = test.CreateSpreadTestData();
var movingAvgData = test.CreateMovingAverageTestData();
var stdDevData = test.CreateStandardDeviationTestData();

// Example: Assert that a null price collection throws
test.ShouldThrowWhenPricesIsNull(() =>
    PriceCalculator.CalculateMovingAverage(null!, 5));

// Example: Verify that an empty collection yields zero for moving average
test.ShouldReturnZeroForEmptyCollection(new List<decimal>(), 0m);

// Enumerate comprehensive edge‑case scenarios
foreach (var (original, @new, expected) in test.GetPercentageChangeEdgeCases())
{
    // Use the values in a test or calculation
    var result = PriceCalculator.CalculatePercentageChange(original, @new);
    result.Should().BeApproximately(expected, 0.0001m);
}

foreach (var (buy, sell, expected) in test.GetSpreadEdgeCases())
{
    var result = PriceCalculator.CalculateSpread(buy, sell);
    result.Should().BeApproximately(expected, 0.0001m);
}
```

## PriceMonitoringServiceTestsValidation

The `PriceMonitoringServiceTestsValidation` class provides validation helpers for PriceMonitoringServiceTests to ensure test data integrity. It contains extension methods for validating PriceMonitoringServiceTests instances, Price objects, and AppSettings objects, returning lists of validation problems or boolean validity checks.

```csharp
using BinanceP2pMonitor.Tests;
using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Models;

// Example 1: Validate a PriceMonitoringServiceTests instance
var tests = new PriceMonitoringServiceTests();
// Initialize tests properties as needed for your test scenario
var validationErrors = tests.Validate();
if (validationErrors.Count == 0)
{
    Console.WriteLine("Tests configuration is valid!");
}
else
{
    Console.WriteLine("Validation errors:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Example 2: Check if tests configuration is valid without collecting errors
bool isValid = tests.IsValid();
Console.WriteLine($"Tests configuration is valid: {isValid}");

// Example 3: Ensure tests configuration is valid (throws exception if invalid)
try
{
    tests.EnsureValid();
    Console.WriteLine("Tests configuration passed validation!");
}
catch (Exception ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}

// Example 4: Validate a Price object
var price = new Price
{
    Asset = "USDT",
    Fiat = "USD",
    BuyPrice = 1.0m,
    SellPrice = 1.01m,
    BuyChangePercent = 0.5m,
    SellChangePercent = 0.5m,
    Timestamp = DateTime.UtcNow,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

var priceErrors = price.Validate();
if (priceErrors.Count == 0)
{
    Console.WriteLine("Price object is valid!");
}
else
{
    Console.WriteLine("Price validation errors:");
    foreach (var error in priceErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Example 5: Validate AppSettings
var settings = new AppSettings
{
    DatabaseConnectionString = "Server=localhost;Database=test;",
    MonitoringIntervalSeconds = 30,
    AlertCooldownMinutes = 5,
    MaxAlertsPerUser = 10,
    HistoryRetentionDays = 30,
    SpreadAnalysisHistoryHours = 24,
    DefaultPriceChangeThreshold = 0.5m,
    DefaultSpreadThreshold = 0.3m
};

var settingsErrors = settings.Validate();
if (settingsErrors.Count == 0)
{
    Console.WriteLine("AppSettings is valid!");
}
else
{
    Console.WriteLine("AppSettings validation errors:");
    foreach (var error in settingsErrors)
    {
        Console.WriteLine($"- {error}");
    }
}
```

## WebSocketService

`WebSocketService` (`src/Services/WebSocketService.cs`) manages a real-time WebSocket connection to Binance's market-stream endpoint and raises price-update events as ticker messages arrive. It implements `IWebSocketService` and `IDisposable`, and is the live data source that feeds the rest of the monitoring pipeline.

### Connection lifecycle

- **`ConnectAsync()`** opens a `ClientWebSocket` to `wss://stream.binance.com:9443/ws`. It is idempotent — if already connected it returns immediately. On failure it throws an `ApiException` with code `WEBSOCKET_CONNECT_FAILED`.
- **`DisconnectAsync()`** stops the keepalive timer and closes the socket with a `NormalClosure` status.
- **`Dispose()`** cancels the receive loop, waits up to 2 seconds for it to drain, then disposes the socket, timer, and cancellation token source.

### Subscribing to pairs

- **`SubscribeToPairAsync(asset, fiat)`** builds a pair key from the lowercased asset and fiat (e.g. `btcusdt`), connects first if needed, and sends a Binance `SUBSCRIBE` request for the `<pair>@ticker` stream. Subscribed pairs are tracked in a `HashSet<string>` so duplicates are ignored.
- **`UnsubscribeFromPairAsync(asset, fiat)`** sends the matching `UNSUBSCRIBE` request and removes the pair from the set.
- After a reconnection, `ConnectAsync` re-sends `SUBSCRIBE` for every pair still in the set, so subscriptions survive a dropped connection.

### Streaming and message handling

- `ListenForMessagesAsync` runs a receive loop on a 4096-byte buffer, accumulating text fragments into a `MemoryStream` until `EndOfMessage`. Binary frames are ignored.
- Each complete text message is deserialized into a `BinanceTickerMessage` (`s` symbol, `b` best bid, `a` best ask, `E` event time). The symbol is split into asset/fiat via `ParsePairKey`, which matches known quote suffixes (`USDT`, `BUSD`, `DAI`, `EUR`, `RUB`, `GBP`) longest-first, falling back to the last 3 characters as the fiat code.
- Valid updates raise the `OnPriceUpdate` event with a `PriceUpdateEventArgs` carrying `Asset`, `Fiat`, `BuyPrice`, `SellPrice`, and `UpdateTime`.

### Keepalive and reconnection

- A `Timer` sends a JSON `ping` every 20 minutes to prevent the server-side ~30-minute idle timeout.
- On an unexpected close or `WebSocketException`, the service stops the keepalive timer and attempts to reconnect with exponential backoff (5s, 10s, 20s, …) up to `MaxReconnectAttempts` (10). If the server closes the connection, reconnection is triggered the same way.

## Doctor Command

The `doctor` command (`src/Commands/DoctorCommand.cs`) validates the application configuration and checks system health. It is a read-only diagnostic command that runs a series of checks and reports a pass/fail summary, exiting with code `0` when every check passes and `1` when any check fails.

```
binance-p2p-monitor doctor
binance-p2p-monitor doctor --help
```

The command runs four checks in sequence:

1. **Configuration Validation** — runs the `ConfigurationValidator` and reports the number of configuration errors found, printing each error individually when validation fails.
2. **Database Connection** — opens a connection through `DatabaseContext` and verifies it is in the `Open` state. On success it prints the connection state and a redacted connection string (the password portion, if present, is masked as `***REDACTED***`; otherwise the string is truncated to 50 characters for security).
3. **Database Schema** — queries `sqlite_master` to confirm the `Prices` table exists, then reports the number of tables found and the current record count in `Prices`.
4. **Configuration Values** — prints the effective runtime settings (monitoring interval, alert cooldown, WebSocket/Telegram toggles, history retention, max alerts per user, price/spread thresholds, database timeout) and the first five monitored assets and fiats. It fails if no monitored assets or no monitored fiats are configured.

After the checks, the command prints a summary table of every check with a `✓ PASS` or `✗ FAIL` status. If all checks pass it prints `All N checks passed! System is healthy.` and returns `0`; otherwise it prints `N of M checks failed. Please review the errors above.` and returns `1`. Failures are also logged through the injected `ILogger<DoctorCommand>`.

## Prune Command

The `prune` command (`src/Commands/PruneCommand.cs`) deletes historical price data older than a specified number of days. It is a destructive maintenance command that permanently removes records from the database, so it requires an interactive confirmation before anything is deleted.

```
binance-p2p-monitor prune --days=30
binance-p2p-monitor prune --days=90
binance-p2p-monitor prune --help
```

### Options

- `--days=DAYS` — required. The number of days of history to keep; records older than this are deleted. Must be a positive integer.
- `-h, --help` — shows the help message.

### Behavior

The command validates its arguments up front: `--days` is required, and its value must parse as a positive integer. If validation fails it prints an error and returns exit code `1` without touching the database.

On a valid invocation it proceeds as follows:

1. Prints a `Database Prune` header and records the current total history count via `IDatabaseCleanupService.GetTotalHistoryCountAsync()`.
2. Prints a warning that the operation will permanently delete historical price data and prompts `Are you sure you want to continue? (yes/no)`. If the user does not answer `yes`, the operation is cancelled and the command returns `0`.
3. On confirmation, calls `IDatabaseCleanupService.DeleteOldRecordsAsync(days)` to delete records older than the threshold.
4. Re-queries the total history count, then prints the number of records deleted and the remaining total, returning exit code `0`.

Any exception during execution is logged through the injected `ILogger<PruneCommand>`, printed as an error, and the command returns exit code `1`. The command depends on `IDatabaseCleanupService` (see the `DatabaseCleanupService` section above), which performs the actual deletion and count reporting.
