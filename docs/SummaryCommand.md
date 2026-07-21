# SummaryCommand
The `SummaryCommand` class is designed to provide a summary of trading data, including buy and sell prices, for a specified set of assets and fiats on the Binance P2P platform. It allows users to retrieve and analyze trading data within a given time window and timezone.

## API
* `public SummaryCommand`: The constructor for the `SummaryCommand` class.
* `public string GetHelp`: Returns a help message for the command.
* `public List<string> ValidateArguments`: Validates the command's arguments and returns a list of error messages if any.
* `public async Task<int> ExecuteAsync`: Executes the command asynchronously and returns an integer indicating the result.
* `public string Date`: Gets or sets the date for the summary.
* `public string Window`: Gets or sets the time window for the summary (e.g., 1d, 1w, 1m).
* `public string Timezone`: Gets or sets the timezone for the summary.
* `public List<Asset> Assets`: Gets or sets the list of assets to include in the summary.
* `public string Symbol`: Gets or sets the symbol for the summary (e.g., BTCUSDT).
* `public List<Fiat> Fiat`: Gets or sets the list of fiats to include in the summary.
* `public decimal MinBuyPrice`: Gets the minimum buy price for the summary.
* `public decimal MaxBuyPrice`: Gets the maximum buy price for the summary.
* `public decimal AvgBuyPrice`: Gets the average buy price for the summary.
* `public decimal CurrentBuyPrice`: Gets the current buy price for the summary.
* `public decimal MinSellPrice`: Gets the minimum sell price for the summary.
* `public decimal MaxSellPrice`: Gets the maximum sell price for the summary.
* `public decimal AvgSellPrice`: Gets the average sell price for the summary.
* `public decimal CurrentSellPrice`: Gets the current sell price for the summary.

## Usage
```csharp
// Example 1: Create a new SummaryCommand and execute it
var summaryCommand = new SummaryCommand
{
    Date = "2022-01-01",
    Window = "1d",
    Timezone = "UTC",
    Assets = new List<Asset> { new Asset("BTC") },
    Symbol = "BTCUSDT",
    Fiat = new List<Fiat> { new Fiat("USDT") }
};

var result = await summaryCommand.ExecuteAsync();
Console.WriteLine($"Result: {result}");

// Example 2: Validate arguments and get help message
var summaryCommand2 = new SummaryCommand();
var errors = summaryCommand2.ValidateArguments;
if (errors.Count > 0)
{
    Console.WriteLine("Errors:");
    foreach (var error in errors)
    {
        Console.WriteLine(error);
    }
}
else
{
    Console.WriteLine(summaryCommand2.GetHelp);
}
```

## Notes
* The `ExecuteAsync` method may throw exceptions if the command fails to execute, such as network errors or invalid arguments.
* The `ValidateArguments` method may return an empty list if all arguments are valid.
* The `GetHelp` method returns a static help message and does not depend on the command's state.
* The `SummaryCommand` class is not thread-safe, and concurrent access to its members may result in unexpected behavior.
* The `Date`, `Window`, and `Timezone` properties must be set before executing the command to ensure accurate results.
* The `Assets` and `Fiat` properties must contain at least one element to ensure valid results.
