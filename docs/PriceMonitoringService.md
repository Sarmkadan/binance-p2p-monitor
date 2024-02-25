# PriceMonitoringService
The `PriceMonitoringService` class is designed to monitor and analyze prices in real-time, providing methods to retrieve current prices, update prices, and perform various analyses on the price data. It is intended to be used in applications that require up-to-date and accurate price information, such as trading platforms or financial analysis tools.

## API
### Constructors
* `public PriceMonitoringService`: Initializes a new instance of the `PriceMonitoringService` class.

### Methods
* `public async Task<Price?> GetCurrentPriceAsync`: Retrieves the current price. Returns a `Price` object if successful, or `null` if the operation fails.
* `public async Task<IEnumerable<Price>> GetAllCurrentPricesAsync`: Retrieves all current prices. Returns a collection of `Price` objects.
* `public async Task<bool> UpdatePriceAsync`: Updates the price. Returns `true` if the update is successful, or `false` otherwise.
* `public async Task<decimal?> GetAveragePriceAsync`: Calculates the average price. Returns the average price as a decimal value if successful, or `null` if the operation fails.
* `public async Task<IEnumerable<Price>> GetPricesWithSignificantChangeAsync`: Retrieves prices with significant changes. Returns a collection of `Price` objects.
* `public async Task<Spread?> GetSpreadAnalysisAsync`: Performs spread analysis. Returns a `Spread` object if successful, or `null` if the operation fails.
* `public async Task StartMonitoringAsync`: Starts the price monitoring service.
* `public async Task StopMonitoringAsync`: Stops the price monitoring service.

## Usage
The following examples demonstrate how to use the `PriceMonitoringService` class:
```csharp
// Example 1: Retrieving the current price
var priceService = new PriceMonitoringService();
var currentPrice = await priceService.GetCurrentPriceAsync();
if (currentPrice != null)
{
    Console.WriteLine($"Current price: {currentPrice}");
}

// Example 2: Starting the monitoring service and retrieving prices with significant changes
var priceService = new PriceMonitoringService();
await priceService.StartMonitoringAsync();
var significantPrices = await priceService.GetPricesWithSignificantChangeAsync();
foreach (var price in significantPrices)
{
    Console.WriteLine($"Price with significant change: {price}");
}
```

## Notes
* The `PriceMonitoringService` class is designed to be used in a multi-threaded environment, but it is not thread-safe by default. Users should ensure that access to the service is properly synchronized to avoid concurrency issues.
* The `UpdatePriceAsync` method may throw an exception if the update operation fails due to external factors such as network errors or data validation issues.
* The `GetAveragePriceAsync` and `GetSpreadAnalysisAsync` methods may return `null` if the calculation fails due to insufficient data or other internal errors.
* The `StartMonitoringAsync` and `StopMonitoringAsync` methods should be used carefully, as they control the monitoring service's state and may affect the behavior of other methods.
