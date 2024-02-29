# SpreadAnalysisService
The `SpreadAnalysisService` class is designed to provide a set of methods for analyzing and retrieving spread data, which is the difference between the buy and sell prices of a particular asset. This class is part of the `binance-p2p-monitor` project and is intended to be used in the context of monitoring and analyzing cryptocurrency prices on the Binance platform.

## API
The `SpreadAnalysisService` class provides the following public members:
* `public SpreadAnalysisService`: The constructor for the `SpreadAnalysisService` class.
* `public async Task<Spread?> GetSpreadAnalysisAsync`: Retrieves the spread analysis for a particular asset. Returns a `Spread` object if successful, or `null` if no spread analysis is available.
* `public async Task<IEnumerable<Spread>> GetTopSpreadOpportunitiesAsync`: Retrieves a list of the top spread opportunities. Returns an `IEnumerable` of `Spread` objects.
* `public ValueTask<decimal> AnalyzeSpreadAsync`: Analyzes the spread for a particular asset and returns the result as a `decimal` value.
* `public ValueTask<bool> UpdateSpreadAsync`: Updates the spread data for a particular asset and returns a `bool` value indicating whether the update was successful.
* `public async Task<Dictionary<string, Spread>> GetAllSpreadsAsync`: Retrieves a dictionary of all available spreads, where the key is the asset name and the value is the corresponding `Spread` object.
* `public async Task<CrossCurrencySpread?> GetCrossCurrencySpreadAsync`: Retrieves the cross-currency spread for a particular asset. Returns a `CrossCurrencySpread` object if successful, or `null` if no cross-currency spread is available.
* `public async Task<IEnumerable<(string Asset, string Fiat, decimal Spread)>> FindAnomalousSpreadAsync`: Retrieves a list of anomalous spreads, where each anomalous spread is represented as a tuple of `(string Asset, string Fiat, decimal Spread)`.

## Usage
Here are two examples of using the `SpreadAnalysisService` class:
```csharp
// Example 1: Retrieving the top spread opportunities
var spreadAnalysisService = new SpreadAnalysisService();
var topSpreads = await spreadAnalysisService.GetTopSpreadOpportunitiesAsync();
foreach (var spread in topSpreads)
{
    Console.WriteLine($"Asset: {spread.Asset}, Spread: {spread.Spread}");
}

// Example 2: Analyzing the spread for a particular asset
var spreadAnalysisService = new SpreadAnalysisService();
var spread = await spreadAnalysisService.AnalyzeSpreadAsync();
Console.WriteLine($"Spread: {spread}");
```

## Notes
The `SpreadAnalysisService` class is designed to be used in a multi-threaded environment, and all of its methods are thread-safe. However, it is worth noting that the `UpdateSpreadAsync` method may throw an exception if the update fails, and the `GetCrossCurrencySpreadAsync` method may return `null` if no cross-currency spread is available. Additionally, the `FindAnomalousSpreadAsync` method may return an empty list if no anomalous spreads are found. It is also important to note that the `SpreadAnalysisService` class relies on the `binance-p2p-monitor` project's configuration and settings, and any changes to these settings may affect the behavior of the class.
