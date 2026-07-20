# DailySummaryService

Represents a snapshot of daily summary data for a specific P2P trading pair on Binance. This type aggregates the date window, timezone, supported assets and fiat currencies, the trading symbol, and the best buy/sell prices observed during the window. It is typically used as a data transfer object returned by monitoring or reporting components.

## API

### `public DailySummaryService()`

Initializes a new instance of the `DailySummaryService` class. All properties are set to their default values (`null` for strings and lists, `0` for decimals).

### `public string Date`

Gets or sets the date of the summary, usually in `yyyy-MM-dd` format.  
**Value:** A string representing the calendar date. May be `null` or empty if not assigned.

### `public string Window`

Gets or sets the time window covered by the summary (e.g., `"24h"`, `"1h"`).  
**Value:** A string describing the aggregation window. May be `null` or empty.

### `public string Timezone`

Gets or sets the timezone identifier used for the summary (e.g., `"UTC"`, `"Asia/Shanghai"`).  
**Value:** A string representing the IANA timezone or UTC offset. May be `null` or empty.

### `public List<Asset> Assets`

Gets or sets the list of crypto assets included in the summary.  
**Value:** A `List<Asset>` instance. Can be `null` or empty. Each `Asset` object is expected to contain details such as the asset name and possibly its quantity or price.

### `public string Symbol`

Gets or sets the trading symbol (e.g., `"BTC"`, `"USDT"`).  
**Value:** A string representing the base asset symbol. May be `null` or empty.

### `public List<Fiat> Fiat`

Gets or sets the list of fiat currencies supported for the summary.  
**Value:** A `List<Fiat>` instance. Can be `null` or empty. Each `Fiat` object typically contains the currency code and related pricing information.

### `public decimal BuyPrice`

Gets or sets the best buy price observed during the summary window.  
**Value:** A decimal number. May be `0` if no buy orders were available.

### `public decimal SellPrice`

Gets or sets the best sell price observed during the summary window.  
**Value:** A decimal number. May be `0` if no sell orders were available.

## Usage

### Example 1: Creating and populating a summary

```csharp
var summary = new DailySummaryService
{
    Date = "2025-03-20",
    Window = "24h",
    Timezone = "UTC",
    Symbol = "BTC",
    BuyPrice = 65432.10m,
    SellPrice = 65450.00m,
    Assets = new List<Asset>
    {
        new Asset { Name = "BTC", Quantity = 0.5m }
    },
    Fiat = new List<Fiat>
    {
        new Fiat { Currency = "USD", Rate = 1.0m }
    }
};
```

### Example 2: Deserializing from JSON and accessing properties

```csharp
string json = @"
{
    ""Date"": ""2025-03-20"",
    ""Window"": ""1h"",
    ""Timezone"": ""Asia/Shanghai"",
    ""Symbol"": ""ETH"",
    ""BuyPrice"": 3450.00,
    ""SellPrice"": 3460.00,
    ""Assets"": [ { ""Name"": ""ETH"", ""Quantity"": 2.0 } ],
    ""Fiat"": [ { ""Currency"": ""CNY"", ""Rate"": 7.24 } ]
}";

var summary = JsonSerializer.Deserialize<DailySummaryService>(json);

Console.WriteLine($"Date: {summary.Date}");
Console.WriteLine($"Spread: {summary.SellPrice - summary.BuyPrice:C}");
```

## Notes

- All string properties (`Date`, `Window`, `Timezone`, `Symbol`) are nullable and can be set to `null` or empty. Consumers should check for null or whitespace before using them for display or formatting.
- The `Assets` and `Fiat` lists can be `null` or empty. Iterating over them without a null check may throw a `NullReferenceException`.
- `BuyPrice` and `SellPrice` are decimals. They may be zero if no orders were matched during the window. Negative values are not expected but are not prevented by the type.
- This class is not thread-safe. If an instance is accessed concurrently from multiple threads, external synchronization (e.g., a lock) must be used to avoid data races.
