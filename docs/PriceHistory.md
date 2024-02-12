# PriceHistory

`PriceHistory` is a data model class used to store and manage historical price data for assets traded on Binance P2P markets. It tracks buy and sell prices, calculates derived metrics like spread and price change percentages, and provides validation and comparison utilities for monitoring price trends over time.

## API

### `public int Id`
Unique identifier for the price history record. Used as a primary key in data storage.

### `public int PriceId`
Foreign key referencing the associated `Price` entity. Links this record to a specific price point in the system.

### `public string Asset`
The cryptocurrency asset symbol (e.g., "BTC", "ETH") for which the price data was recorded.

### `public string Fiat`
The fiat currency symbol (e.g., "USDT", "EUR") in which the price is denominated.

### `public decimal BuyPrice`
The current buy price of the asset in the specified fiat currency.

### `public decimal SellPrice`
The current sell price of the asset in the specified fiat currency.

### `public DateTime RecordedAt`
The timestamp when the price data was originally recorded from the Binance P2P market.

### `public DateTime CreatedAt`
The timestamp when the `PriceHistory` record was created in the system. Typically set to the current UTC time upon instantiation.

### `public decimal SpreadPercentage`
The calculated spread between buy and sell prices as a percentage of the mid price. Computed as `(SellPrice - BuyPrice) / MidPrice * 100`.

### `public decimal PriceChangePercent`
The percentage change in price compared to a previous reference point (e.g., 24-hour change). Value is positive for increases, negative for decreases.

### `public string? Notes`
Optional descriptive notes or metadata associated with the price record. Can be `null`.

### `public Price? Price`
Reference to the associated `Price` entity, if loaded. May be `null` if not eagerly loaded or not available.

### `public decimal GetMidPrice()`
Calculates and returns the mid price between the buy and sell prices.
**Returns:** The arithmetic mean of `BuyPrice` and `SellPrice`.
**Throws:** No exceptions are thrown under normal operation.

### `public decimal CalculateSpread()`
Calculates the absolute spread between buy and sell prices.
**Returns:** The difference `SellPrice - BuyPrice`.
**Throws:** No exceptions are thrown under normal operation.

### `public bool IsValid()`
Validates the price history record for completeness and correctness.
**Returns:** `true` if the record has non-null `Asset`, `Fiat`, valid `BuyPrice` and `SellPrice` (greater than zero), and a `RecordedAt` timestamp in the past; otherwise `false`.
**Throws:** No exceptions are thrown.

### `public bool IsRecent()`
Determines if the price record was recorded within a recent time window (e.g., last 5 minutes).
**Returns:** `true` if `RecordedAt` is within the recent window; otherwise `false`.
**Throws:** No exceptions are thrown.

### `public int GetAgeInMinutes()`
Calculates the age of the price record in minutes, based on `RecordedAt`.
**Returns:** The number of whole minutes elapsed since `RecordedAt` until the current UTC time.
**Throws:** No exceptions are thrown.

### `public decimal CompareTo(PriceHistory other)`
Compares this price history record to another based on price change percentage.
**Parameters:**
- `other`: The `PriceHistory` instance to compare against.
**Returns:** A `decimal` representing the difference in `PriceChangePercent` between this instance and `other` (`this.PriceChangePercent - other.PriceChangePercent`).
**Throws:** `ArgumentNullException` if `other` is `null`.

## Usage

### Example 1: Creating and Validating a Price History Record
