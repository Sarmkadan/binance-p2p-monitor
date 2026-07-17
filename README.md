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
