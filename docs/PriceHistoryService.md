# PriceHistoryService

The `PriceHistoryService` is a service responsible for recording, retrieving, and analyzing historical price data within the `binance-p2p-monitor` project. It provides methods to persist price points, query historical trends, compute statistical metrics, and maintain the integrity of the stored data by removing outdated entries. This service is designed to support monitoring and analytical features that depend on historical price context.

## API

### `Task<int> RecordPriceAsync`

**Purpose**
Records a new price entry in the history store. The specific price value and associated metadata (e.g., timestamp) are determined internally by the service implementation.

**Parameters**
None.

**Return Value**
Returns the number of price entries successfully recorded in the current operation.

**Exceptions**
- Throws `InvalidOperationException` if the underlying storage mechanism fails (e.g., database connection issues).
- Throws `ArgumentException` if the price data is determined to be invalid during validation.

---

### `Task<IEnumerable<PriceHistory>> GetHistoryAsync`

**Purpose**
Retrieves a sequence of historical price records. The returned collection may be filtered or ordered based on internal service logic (e.g., time range, currency pair).

**Parameters**
None.

**Return Value**
Returns an `IEnumerable<PriceHistory>` containing zero or more price history entries. The `PriceHistory` type includes properties such as `Price`, `Timestamp`, and other relevant metadata.

**Exceptions**
- Throws `InvalidOperationException` if the retrieval operation fails (e.g., storage access error).

---

### `Task<decimal> GetPriceTrendAsync`

**Purpose**
Calculates the current price trend based on recent historical data. The trend is typically represented as a signed decimal value indicating the direction and magnitude of price movement (e.g., positive for upward trend, negative for downward).

**Parameters**
None.

**Return Value**
Returns a `decimal` value representing the computed trend. The exact calculation method (e.g., moving average, linear regression) is determined by the service implementation.

**Exceptions**
- Throws `InvalidOperationException` if insufficient data is available to compute the trend.
- Throws `OverflowException` if the trend calculation results in a value outside the valid `decimal` range.

---

### `Task<(decimal High, decimal Low, decimal Average)> GetPriceStatsAsync`

**Purpose**
Computes key statistical metrics (highest price, lowest price, and average price) over a defined historical window. The window is determined by the service implementation (e.g., last 24 hours, last 7 days).

**Parameters**
None.

**Return Value**
Returns a tuple containing:
- `High`: The highest recorded price in the window.
- `Low`: The lowest recorded price in the window.
- `Average`: The arithmetic mean of all prices in the window.

**Exceptions**
- Throws `InvalidOperationException` if no data is available for the specified window.
- Throws `OverflowException` if any computed value exceeds the `decimal` range.

---

### `Task<bool> CleanupOldHistoryAsync`

**Purpose**
Removes outdated price history entries based on a retention policy (e.g., older than 30 days). This method is intended to prevent unbounded storage growth.

**Parameters**
None.

**Return Value**
Returns `true` if the cleanup operation completed successfully, `false` if no entries were eligible for removal.

**Exceptions**
- Throws `InvalidOperationException` if the cleanup operation fails (e.g., storage access error).

---

### `Task<long> GetHistoryCountAsync`

**Purpose**
Returns the total number of price history entries currently stored.

**Parameters**
None.

**Return Value**
Returns a `long` representing the count of entries.

**Exceptions**
- Throws `InvalidOperationException` if the count cannot be retrieved (e.g., storage access error).

---

### `Task<Dictionary<string, object>> GetDetailedAnalysisAsync`

**Purpose**
Generates a comprehensive analysis of historical price data, including metrics such as volatility, trend strength, and distribution characteristics. The returned dictionary contains keys representing metric names and values of varying types (e.g., `decimal`, `string`, `DateTime`).

**Parameters**
None.

**Return Value**
Returns a `Dictionary<string, object>` where each key-value pair represents a distinct analytical metric. Example keys might include `"Volatility"`, `"TrendSlope"`, or `"PriceDistribution"`.

**Exceptions**
- Throws `InvalidOperationException` if the analysis cannot be performed (e.g., insufficient data).
- Throws `KeyNotFoundException` if a requested metric cannot be computed.

## Usage

### Example 1: Recording and Analyzing Price Trends
