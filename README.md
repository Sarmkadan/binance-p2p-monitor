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
