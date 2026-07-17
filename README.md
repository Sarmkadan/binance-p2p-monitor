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
