# Price

Represents a monitored price record for a trading pair on Binance P2P, including current and historical pricing data along with metadata for change tracking and validation.

## API

### `public int Id`
Unique identifier for the price record. Used as a primary key in persistence.

### `public string Asset`
Cryptocurrency asset symbol (e.g., "USDT", "BTC"). Non-null and non-empty.

### `public string Fiat`
Fiat currency symbol (e.g., "USD", "EUR"). Non-null and non-empty.

### `public decimal BuyPrice`
Current buy price in fiat currency. Must be positive.

### `public decimal SellPrice`
Current sell price in fiat currency. Must be positive.

### `public decimal BuyChangePercent`
Percentage change in buy price since the last recorded history entry. Can be negative or positive.

### `public decimal SellChangePercent`
Percentage change in sell price since the last recorded history entry. Can be negative or positive.

### `public DateTime Timestamp`
UTC timestamp when the price data was captured from the exchange. Must be in the past.

### `public DateTime CreatedAt`
UTC timestamp when the record was first persisted. Set once on creation.

### `public DateTime UpdatedAt`
UTC timestamp when the record was last updated. Updated on every modification.

### `public string? Metadata`
Optional JSON string containing additional exchange-specific or derived data. Nullable.

### `public ICollection<PriceHistory> History`
Collection of historical price snapshots associated with this record. Populated on load.

### `public decimal CalculateSpread()`
Computes the percentage spread between buy and sell prices as:
`(SellPrice - BuyPrice) / BuyPrice * 100`.
Returns a non-negative decimal representing the spread in percent.

### `public bool IsValid()`
Validates the record by checking:
- `Asset` and `Fiat` are non-null and non-empty.
- `BuyPrice` and `SellPrice` are positive.
- `Timestamp` is in the past.
- `BuyChangePercent` and `SellChangePercent` are within a reasonable range (e.g., ±1000%).
Returns `true` if all checks pass; otherwise `false`.

### `public bool IsDifferentFrom(Price other)`
Determines whether this price differs meaningfully from another.
Two prices are considered different if either:
- `BuyPrice` differs by more than 0.01%.
- `SellPrice` differs by more than 0.01%.
- `BuyChangePercent` or `SellChangePercent` differ by more than 0.01%.
Returns `true` if any difference exceeds the threshold; otherwise `false`.

### `public string ToJson()`
Serializes the current instance to a compact JSON string.
Includes all public properties except `History` (excluded to avoid circular references).
Returns the JSON representation.

### `public static Price? FromJson(string json)`
Deserializes a JSON string into a `Price` instance.
Returns `null` if deserialization fails or required fields are missing/invalid.
Throws `JsonException` if the JSON is malformed.

## Usage
