# CachedPriceMonitoringServiceExtensions

The `CachedPriceMonitoringServiceExtensions` class provides a set of static asynchronous extension methods designed to enhance the functionality of the `CachedPriceMonitoringService` within the `binance-p2p-monitor` project. These utilities facilitate robust data retrieval with retry logic, filtered price analysis based on buy thresholds or significant volatility, and comprehensive spread risk assessment, allowing consumers to interact with cached Binance P2P price data efficiently and safely.

## API

### GetCurrentPriceAsyncWithRetry
Retrieves the current price for a specific asset pair, implementing an internal retry mechanism to handle transient network failures or temporary service unavailability.
*   **Parameters**: Accepts the target `CachedPriceMonitoringService` instance, the asset symbol (e.g., "USDT"), and the fiat currency (e.g., "USD"). Specific retry count and delay configurations may be optional depending on the overload used.
*   **Return Value**: Returns a `Task<Price?>`. If successful, it yields a `Price` object; if all retries are exhausted or the price is unavailable, it returns `null`.
*   **Exceptions**: Throws exceptions only if a non-retryable critical error occurs (e.g., invalid arguments or permanent service failure), otherwise it swallows transient errors and returns `null`.

### GetPricesWithMinBuyPriceAsync
Filters the available cached prices to return only those entries where the buy price meets or exceeds a specified minimum threshold.
*   **Parameters**: Accepts the `CachedPriceMonitoringService` instance, the asset symbol, the fiat currency, and a decimal `minBuyPrice`.
*   **Return Value**: Returns a `Task<IEnumerable<Price>>` containing a collection of `Price` objects that satisfy the condition. Returns an empty enumerable if no matches are found.
*   **Exceptions**: May throw `ArgumentException` if the `minBuyPrice` is negative or if the asset/fiat combination is invalid.

### GetSpreadAnalysisWithRiskAsync
Calculates the bid-ask spread for a given asset and evaluates the associated risk level based on predefined volatility or spread thresholds.
*   **Parameters**: Accepts the `CachedPriceMonitoringService` instance, the asset symbol, and the fiat currency.
*   **Return Value**: Returns a `Task` containing a tuple: `(Spread Spread, bool IsHigh, bool IsLow, string RiskLevel)`. The `Spread` object contains the calculated difference, booleans indicate if the spread is statistically high or low, and `RiskLevel` provides a qualitative string assessment (e.g., "Low", "Medium", "High").
*   **Exceptions**: Throws if current market data required to calculate the spread is missing or corrupted.

### GetPricesWithSignificantChangeAsync
Identifies and returns prices that have undergone a significant percentage change compared to a previous reference point or time window.
*   **Parameters**: Accepts the `CachedPriceMonitoringService` instance, the asset symbol, the fiat currency, and a decimal `thresholdPercentage` defining what constitutes a "significant" change.
*   **Return Value**: Returns a `Task<IEnumerable<Price>>` listing prices that have fluctuated beyond the specified threshold.
*   **Exceptions**: May throw if historical data required for comparison is unavailable or if the threshold is invalid (e.g., negative).

## Usage

### Example 1: Retrieving Prices with Retry Logic and Filtering
This example demonstrates how to fetch the current USDT price with automatic retries and then filter available offers to find those with a buy price of at least 0.98.

```csharp
using BinanceP2PMonitor.Services;
using BinanceP2PMonitor.Models;

public async Task AnalyzeUsdtOffersAsync(CachedPriceMonitoringService service)
{
    // Attempt to get the current price with built-in retry logic
    var currentPrice = await service.GetCurrentPriceAsyncWithRetry("USDT", "USD");

    if (currentPrice != null)
    {
        Console.WriteLine($"Current Reference Price: {currentPrice.Value}");

        // Fetch all prices where the buy price is >= 0.98
        var viableOffers = await service.GetPricesWithMinBuyPriceAsync("USDT", "USD", 0.98m);

        foreach (var offer in viableOffers)
        {
            Console.WriteLine($"Offer available at: {offer.BuyPrice}");
        }
    }
    else
    {
        Console.WriteLine("Failed to retrieve current price after retries.");
    }
}
```

### Example 2: Spread Analysis and Risk Assessment
This example performs a risk analysis on the ETH/BUSD pair to determine if the current spread indicates a high-risk trading environment.

```csharp
using BinanceP2PMonitor.Services;
using BinanceP2PMonitor.Models;

public async Task AssessEthRiskAsync(CachedPriceMonitoringService service)
{
    try
    {
        var analysis = await service.GetSpreadAnalysisWithRiskAsync("ETH", "BUSD");

        Console.WriteLine($"Spread: {analysis.Spread.Value}");
        Console.WriteLine($"Risk Level: {analysis.RiskLevel}");

        if (analysis.IsHigh)
        {
            Console.WriteLine("Warning: Spread is significantly higher than average.");
        }
        
        if (analysis.RiskLevel == "High")
        {
            // Trigger alert or halt trading logic
            HandleHighRiskScenario();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error analyzing spread: {ex.Message}");
    }
}

private void HandleHighRiskScenario()
{
    // Implementation of risk mitigation
}
```

## Notes

*   **Null Handling**: Consumers of `GetCurrentPriceAsyncWithRetry` must explicitly check for `null` return values, as the method suppresses transient exceptions in favor of returning `null` after exhausting retry attempts.
*   **Empty Collections**: Methods returning `IEnumerable<Price>` (such as `GetPricesWithMinBuyPriceAsync` and `GetPricesWithSignificantChangeAsync`) will return an empty collection rather than `null` if no data matches the criteria. Callers should not assume `null` indicates an error.
*   **Thread Safety**: As these are static extension methods operating on a passed service instance, thread safety depends entirely on the underlying implementation of the `CachedPriceMonitoringService`. If the service instance is shared across threads, ensure the service itself handles concurrent access to its cache correctly.
*   **Data Freshness**: The accuracy of `GetSpreadAnalysisWithRiskAsync` and `GetPricesWithSignificantChangeAsync` relies on the freshness of the internal cache. If the cache staleness policy is too lenient, risk assessments may be based on outdated market conditions.
*   **Decimal Precision**: When using threshold parameters (e.g., `minBuyPrice` or `thresholdPercentage`), ensure that the decimal precision matches the expected asset configuration to avoid floating-point comparison issues.
