# Market
The `Market` type represents a single Binance P2P trading pair (asset‑fiat combination) within the monitor application. It stores identifying information, current price statistics, volume metrics, and flags that control whether the pair is actively tracked and how it should be prioritized for monitoring updates.

## API
### Fields
- **Id** (`int`)  
  Unique identifier for the market record in the data store.  
  *No parameters, no return value, does not throw.*

- **Asset** (`string`)  
  The cryptocurrency symbol (e.g., `"BTC"`).  
  *No parameters, no return value, does not throw.*

- **Fiat** (`string`)  
  The fiat currency symbol (e.g., `"USD"`).  
  *No parameters, no return value, does not throw.*

- **IsActive** (`bool`)  
  Indicates whether the market is currently enabled for trading on Binance P2P.  
  *No parameters, no return value, does not throw.*

- **IsMonitored** (`bool`)  
  Flag controlled by the monitor to determine if price updates should be fetched for this market.  
  *No parameters, no return value, does not throw.*

- **Description** (`string`)  
  Human‑readable description of the market, often formatted as `"{Asset}/{Fiat}"`.  
  *No parameters, no return value, does not throw.*

- **LastBuyPrice** (`decimal`)  
  The most recent observed buy price (the price at which users can purchase the asset).  
  *No parameters, no return value, does not throw.*

- **LastSellPrice** (`decimal`)  
  The most recent observed sell price (the price at which users can sell the asset).  
  *No parameters, no return value, does not throw.*

- **TotalOffers** (`long`)  
  Aggregate count of active buy and sell offers for the market at the last snapshot.  
  *No parameters, no return value, does not throw.*

- **DailyVolume** (`long`)  
  Estimated trading volume over the past 24 hours, expressed in the asset’s base unit.  
  *No parameters, no return value, does not throw.*

- **CreatedAt** (`DateTime`)  
  Timestamp when the market record was first inserted into the system.  
  *No parameters, no return value, does not throw.*

- **UpdatedAt** (`DateTime`)  
  Timestamp of the last modification to any field of the market record.  
  *No parameters, no return value, does not throw.*

- **LastPriceUpdateAt** (`long?`)  
  Unix epoch milliseconds of the most recent price fetch; `null` if no price data has been retrieved yet.  
  *No parameters, no return value, does not throw.*

- **MonitoringPriority** (`int`)  
  Integer used to sort markets when scheduling updates; lower numbers indicate higher priority.  
  *No parameters, no return value, does not throw.*

### Methods
- **GetPairId** (`string`)  
  Returns a stable identifier for the market, typically formatted as `"{Asset}_{Fiat}"`.  
  *Parameters: none.*  
  *Return value: a non‑null string.*  
  *Throws: none.*

- **CalculateSpread** (`decimal`)  
  Computes the percentage spread between the last sell and buy prices: `((LastSellPrice - LastBuyPrice) / LastBuyPrice) * 100`. Returns a negative value if the buy price exceeds the sell price (an arbitrage opportunity).  
  *Parameters: none.*  
  *Return value: the spread as a decimal.*  
  *Throws: `DivideByZeroException` if `LastBuyPrice` is zero.*

- **UpdatePrices** (`void`)  
  Refreshes `LastBuyPrice`, `LastSellPrice`, `TotalOffers`, `DailyVolume`, and sets `LastPriceUpdateAt` to the current Unix epoch milliseconds. Intended to be called after a successful API request to Binance P2P.  
  *Parameters: none.*  
  *Return value: none.*  
  *Throws: may propagate exceptions from the underlying data‑access layer (e.g., network or serialization errors).*

- **IsPriceStale** (`bool`)  
  Determines whether the cached price data is considered stale based on a configurable timeout (e.g., 5 minutes). Returns `true` if `LastPriceUpdateAt` is null or the elapsed time since that timestamp exceeds the threshold.  
  *Parameters: none.*  
  *Return value: `true` if stale, otherwise `false`.*  
  *Throws: none.*

- **IsValid** (`bool`)  
  Performs a basic sanity check: ensures `Asset` and `Fiat` are non‑empty, `IsActive` is true, and both `LastBuyPrice` and `LastSellPrice` are positive.  
  *Parameters: none.*  
  *Return value: `true` if the market meets all validation criteria, otherwise `false`.*  
  *Throws: none.*

- **GetActivityLevel** (`string`)  
  Returns a categorical description of market activity based on `DailyVolume` and `TotalOffers`. Example outputs: `"Low"`, `"Medium"`, `"High"`. The exact thresholds are internal to the method.  
  *Parameters: none.*  
  *Return value: one of the predefined activity level strings.*  
  *Throws: none.*

## Usage
```csharp
// Example 1: Creating a market instance and checking its validity
var market = new Market
{
    Id = 42,
    Asset = "ETH",
    Fiat = "EUR",
    IsActive = true,
    IsMonitored = true,
    Description = "ETH/EUR",
    LastBuyPrice = 1850.75m,
    LastSellPrice = 1865.20m,
    TotalOffers = 124,
    DailyVolume = 3500000L,
    CreatedAt = DateTime.UtcNow.AddDays(-10),
    UpdatedAt = DateTime.UtcNow,
    LastPriceUpdateAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    MonitoringPriority = 1
};

if (market.IsValid())
{
    Console.WriteLine($"Market {market.GetPairId()} is ready for monitoring.");
}
else
{
    Console.WriteLine("Market data is incomplete or inactive.");
}
```

```csharp
// Example 2: Updating prices and evaluating spread and staleness
market.UpdatePrices(); // fetches latest data from Binance P2P

decimal spread = market.CalculateSpread();
Console.WriteLine($"Current spread: {spend:F2}%");

if (market.IsPriceStale())
{
    Console.WriteLine("Price data is stale; consider refreshing sooner.");
}
else
{
    Console.WriteLine("Price data is fresh.");
}

string activity = market.GetActivityLevel();
Console.WriteLine($"Activity level: {activity}");
```

## Notes
- The type does **not** implement any locking mechanism; concurrent reads or writes from multiple threads can lead to inconsistent state. External synchronization (e.g., `lock`, `ReaderWriterLockSlim`, or using immutable copies) is required when the instance is accessed concurrently.
- `LastPriceUpdateAt` is nullable to distinguish between “never updated” and “updated at epoch zero”. Consumers should treat a `null` value as stale.
- `CalculateSpread` will throw a `DivideByZeroException` if `LastBuyPrice` is zero; callers should guard against this by validating prices before invocation.
- String fields (`Asset`, `Fiat`, `Description`) are not validated for null or empty values by the type itself; validation is performed by `IsValid`.
- The monitoring priority field is intended for internal sorting logic; changing it does not automatically re‑order any scheduled tasks—responsibility lies with the scheduler that consumes this value.
