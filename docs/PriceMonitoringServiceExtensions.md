# PriceMonitoringServiceExtensions

Provides extension‑style utilities for retrieving and analyzing Binance P2P price data. The type groups asynchronous query methods that return current, filtered, aggregated, and statistical price information, together with a set of properties that describe the monitored asset/fiat pair and summary statistics of the observed prices.

## API

### GetCurrentPriceAsync
```csharp
public static async Task<Price?> GetCurrentPriceAsync()
```
Asynchronously obtains the most recent price for the configured asset/fiat pair.  
- **Parameters:** none.  
- **Return value:** A `Price` instance representing the latest quote, or `null` if no price data is available.  
- **Exceptions:** May throw `IOException`, `HttpRequestException`, or `OperationCanceledException` when the underlying network request fails or is cancelled.

### GetFilteredCurrentPricesAsync
```csharp
public static async Task<IEnumerable<Price>> GetFilteredCurrentPricesAsync()
```
Asynchronously retrieves a collection of current prices that satisfy the filter criteria implicit in the service configuration.  
- **Parameters:** none.  
- **Return value:** An enumerable of `Price` objects; empty sequence if no prices match the filter.  
- **Exceptions:** Same as `GetCurrentPriceAsync`.

### GetBestPricesAsync
```csharp
public static async Task<(decimal BestBuyPrice, decimal BestSellPrice, int PriceCount)> GetBestPricesAsync()
```
Asynchronously calculates the best (minimum) buy price, the best (maximum) sell price, and the total number of prices considered.  
- **Parameters:** none.  
- **Return value:** A tuple where `BestBuyPrice` is the lowest observed buy price, `BestSellPrice` is the highest observed sell price, and `PriceCount` is the number of price samples used.  
- **Exceptions:** May throw the same network‑related exceptions as the other query methods; additionally throws `InvalidOperationException` if no price data is available to compute the extrema.

### GetPriceStatisticsAsync
```csharp
public static async Task<PriceStatistics?> GetPriceStatisticsAsync()
```
Asynchronously computes statistical measures (average, volatility, min/max) for the monitored price series.  
- **Parameters:** none.  
- **Return value:** A `PriceStatistics` instance containing the derived metrics, or `null` when insufficient data exists.  
- **Exceptions:** Propagates any exceptions thrown by the data‑access layer; may also throw `InvalidOperationException` if the calculation cannot be performed due to missing values.

### WouldTriggerAlertAsync
```csharp
public static async Task<bool> WouldTriggerAlertAsync()
```
Asynchronously evaluates whether the current price conditions satisfy the alert rules associated with the service.  
- **Parameters:** none.  
- **Return value:** `true` if an alert should be raised, otherwise `false`.  
- **Exceptions:** May throw exceptions from the underlying price‑retrieval methods.

### Asset
```csharp
public string Asset { get; }
```
Gets the symbol of the digital asset being monitored (e.g., `"BTC"`).

### Fiat
```csharp
public string Fiat { get; }
```
Gets the fiat currency used for pricing (e.g., `"USD"`).

### Hours
```csharp
public int Hours { get; }
```
Gets the look‑back window, in hours, over which price samples are collected for statistical calculations.

### AverageBuyPrice
```csharp
public decimal AverageBuyPrice { get; }
```
Gets the arithmetic mean of observed buy prices within the look‑back window.

### AverageSellPrice
```csharp
public decimal AverageSellPrice { get; }
```
Gets the arithmetic mean of observed sell prices within the look‑back window.

### MinBuyPrice
```csharp
public decimal MinBuyPrice { get; }
```
Gets the smallest buy price observed in the look‑back window.

### MaxBuyPrice
```csharp
public decimal MaxBuyPrice { get; }
```
Gets the largest buy price observed in the look‑back window.

### MinSellPrice
```csharp
public decimal MinSellPrice { get; }
```
Gets the smallest sell price observed in the look‑back window.

### MaxSellPrice
```csharp
public decimal MaxSellPrice { get; }
```
Gets the largest sell price observed in the look‑back window.

### BuyPriceVolatilityPercent
```csharp
public decimal BuyPriceVolatilityPercent { get; }
```
Gets the percentage volatility of buy prices (standard deviation divided by mean) over the look‑back window.

### SellPriceVolatilityPercent
```csharp
public decimal SellPriceVolatilityPercent { get; }
```
Gets the percentage volatility of sell prices over the look‑back window.

### PriceCount
```csharp
public int PriceCount { get; }
```
Gets the total number of price samples collected during the look‑back window.

### LastUpdated
```csharp
public DateTime LastUpdated { get; }
```
Gets the timestamp of the most recent price update.

## Usage

### Example 1: Retrieving the current price and checking alert conditions
```csharp
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Assume the service has been configured elsewhere with Asset = "BTC", Fiat = "USD", Hours = 24
        Price? current = await PriceMonitoringServiceExtensions.GetCurrentPriceAsync();
        if (current.HasValue)
        {
            Console.WriteLine($"Current {PriceMonitoringServiceExtensions.Asset}/{PriceMonitoringServiceExtensions.Fiat} price: {current.Value.Amount}");
        }

        bool shouldAlert = await PriceMonitoringServiceExtensions.WouldTriggerAlertAsync();
        if (shouldAlert)
        {
            Console.WriteLine("Alert condition met!");
        }
    }
}
```

### Example 2: Obtaining best prices and statistical summary
```csharp
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var (bestBuy, bestSell, count) = await PriceMonitoringServiceExtensions.GetBestPricesAsync();
        Console.WriteLine($"Best buy: {bestBuy}, Best sell: {bestSell}, Samples: {count}");

        var stats = await PriceMonitoringServiceExtensions.GetPriceStatisticsAsync();
        if (stats != null)
        {
            Console.WriteLine($"Average buy: {stats.AverageBuy}, Average sell: {stats.AverageSell}");
            Console.WriteLine($"Buy volatility: {stats.BuyVolatilityPercent}%");
            Console.WriteLine($"Last updated: {PriceMonitoringServiceExtensions.LastUpdated:u}");
        }
    }
}
```

## Notes
- The static query methods do not accept explicit parameters; they operate on the configuration encapsulated by the instance properties (`Asset`, `Fiat`, `Hours`, etc.). Consequently, calling these methods on different threads with differing property values may produce inconsistent results if the properties are mutated concurrently.  
- Instance properties are intended to be set once (e.g., via constructor or dependency injection) and then treated as immutable for the lifetime of the object. Reading these properties from multiple threads is safe; however, concurrent writes could lead to race conditions and should be avoided.  
- All asynchronous methods perform I/O against the Binance P2P API and are therefore subject to network latency, transient failures, and cancellation. Consumers should handle `OperationCanceledException` when using cancellation tokens and implement retry logic as appropriate for their reliability requirements.  
- If no price data is available for the requested window, `GetCurrentPriceAsync` returns `null`, `GetFilteredCurrentPricesAsync` returns an empty sequence, `GetBestPricesAsync` throws `InvalidOperationException`, and `GetPriceStatisticsAsync` returns `null`. Callers should check for these conditions before using the returned values.  
- The volatility percentages are calculated as the sample standard deviation divided by the mean, multiplied by 100. When the mean is zero, the property returns `decimal.MinValue` to indicate an undefined value.  
- The type does not inherit from any other class or implement interfaces beyond those implicitly required by the listed members. No additional base‑class behavior should be assumed.
