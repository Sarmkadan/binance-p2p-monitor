// entire file content ...
// ... goes in between

## CachedPriceMonitoringService

The `CachedPriceMonitoringService` class is a decorator that adds caching to the `IPriceMonitoringService` interface. It provides a way to store and retrieve price data in memory, reducing the number of requests made to the underlying service.

### Usage

```csharp
using BinanceP2pMonitor.Infrastructure;

// Create a new instance of CachedPriceMonitoringService
var cachedPriceMonitoringService = new CachedPriceMonitoringService(
    new PriceMonitoringService(),
    new MemoryCache(),
    new Logger<CachedPriceMonitoringService>());

// Get the current price for a specific asset and fiat
var currentPrice = await cachedPriceMonitoringService.GetCurrentPriceAsync("BTC", "USD");

// Get all current prices
var allCurrentPrices = await cachedPriceMonitoringService.GetAllCurrentPricesAsync();

// Update the price for a specific asset and fiat
var updated = await cachedPriceMonitoringService.UpdatePriceAsync(new Price { Asset = "BTC", Fiat = "USD" });

// Get the average price for a specific asset and fiat over a period of time
var averagePrice = await cachedPriceMonitoringService.GetAveragePriceAsync("BTC", "USD", 1);

// Get prices with significant change
var pricesWithSignificantChange = await cachedPriceMonitoringService.GetPricesWithSignificantChangeAsync(0.1m);

// Get the spread analysis for a specific asset and fiat
var spreadAnalysis = await cachedPriceMonitoringService.GetSpreadAnalysisAsync("BTC", "USD");

// Start monitoring prices
await cachedPriceMonitoringService.StartMonitoringAsync();

// Stop monitoring prices
await cachedPriceMonitoringService.StopMonitoringAsync();
```

// ... rest of file content ...

## RetryPolicy

The `RetryPolicy` class provides a configurable retry mechanism for handling transient errors in asynchronous operations. It implements `ILogger` and can be used to wrap operations that may fail temporarily due to network issues, rate limiting, or other transient failures. The policy supports customizable retry behavior through its public members and can be easily integrated into dependency injection scenarios.

### Usage

```csharp
using BinanceP2pMonitor.Infrastructure;
using Microsoft.Extensions.Logging;

// Create a retry policy with default settings
var retryPolicy = new RetryPolicy(
    maxRetryCount: 3,
    initialDelay: TimeSpan.FromSeconds(1),
    maxDelay: TimeSpan.FromSeconds(30),
    retryableExceptionTypes: new[] { typeof(HttpRequestException), typeof(TaskCanceledException) }
);

// Execute an async operation with retry
var result = await retryPolicy.ExecuteAsync(async () => {
    var response = await httpClient.GetAsync("https://api.binance.com/api/v3/ticker/price");
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<PriceData>();
});

// Execute a void async operation with retry
await retryPolicy.ExecuteAsync(async () => {
    await priceMonitoringService.UpdatePriceAsync(new Price { Asset = "BTC", Fiat = "USD" });
});

// Check if an exception is considered transient
bool isTransient = RetryPolicy.IsTransientError(new HttpRequestException("Connection failed"));

// Use with dependency injection and logging
services.AddLogging(logging => {
    logging.AddFilter("RetryPolicy", LogLevel.Information);
    logging.AddConsole();
});

var loggerFactory = LoggerFactory.Create(builder => {
    builder.AddConsole();
});

var scopedRetryPolicy = new RetryPolicy(
    maxRetryCount: 5,
    initialDelay: TimeSpan.FromMilliseconds(500),
    maxDelay: TimeSpan.FromSeconds(10)
);

// Log retry attempts using the ILogger interface
scopedRetryPolicy.Log(
    LogLevel.Information,
    new EventId(1, "RetryAttempt"),
    "Retry attempt {RetryCount}",
    null,
    (state, exception) => string.Format(state.ToString(), 1)
);
```
