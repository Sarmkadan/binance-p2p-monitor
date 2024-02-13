# Spread

Represents statistical metrics and risk indicators for price spreads in Binance P2P trading pairs, used to monitor volatility, outliers, and trading conditions.

## API

### `public int Id`
Unique identifier for the spread record. Used for tracking and database persistence.

### `public string Asset`
Cryptocurrency asset symbol (e.g., "USDT", "BTC") associated with the spread data.

### `public string Fiat`
Fiat currency symbol (e.g., "USD", "EUR") used in the trading pair.

### `public decimal CurrentSpreadPercent`
Current observed spread percentage between buy and sell prices. Calculated as `(askPrice - bidPrice) / midPrice * 100`.

### `public decimal AverageSpreadPercent`
Average spread percentage over all samples collected for this pair.

### `public decimal MinSpreadPercent`
Minimum observed spread percentage in the sample set.

### `public decimal MaxSpreadPercent`
Maximum observed spread percentage in the sample set.

### `public long SampleCount`
Total number of spread samples used to compute statistics.

### `public DateTime LastUpdatedAt`
Timestamp of the most recent spread update.

### `public DateTime CreatedAt`
Timestamp when the spread record was first created.

### `public decimal StandardDeviation`
Standard deviation of the spread percentages in the sample set. Measures volatility.

### `public decimal PercentileRank`
Percentile rank of the current spread relative to historical distribution (0–100). Indicates how extreme the current spread is.

### `public bool IsHighSpread`
Indicates whether the current spread is considered abnormally high based on internal thresholds.

### `public bool IsLowSpread`
Indicates whether the current spread is considered abnormally low based on internal thresholds.

### `public decimal GetVarianceFromAverage()`
Computes the squared difference between `CurrentSpreadPercent` and `AverageSpreadPercent`.

- **Returns**: `decimal` — variance value.
- **Throws**: May throw if `AverageSpreadPercent` is not set or invalid.

### `public bool IsNormal()`
Determines if the current spread falls within acceptable bounds (not high or low).

- **Returns**: `bool` — `true` if spread is within normal range.
- **Throws**: May throw if statistics are not initialized.

### `public bool IsValid`
Indicates whether the spread data is valid and safe for use (e.g., non-negative spreads, sufficient samples).

- **Returns**: `bool` — `true` if data is valid.

### `public string GetRiskLevel()`
Returns a qualitative risk assessment based on current spread and statistical context.

- **Returns**: `string` — one of: "Low", "Medium", "High", "Critical".
- **Throws**: May throw if data is invalid or insufficient.

### `public void UpdateStatistics()`
Recalculates all derived statistics (`AverageSpreadPercent`, `StandardDeviation`, `PercentileRank`, etc.) using the current sample set.

- **Throws**: May throw if sample data is corrupted or insufficient for calculation.
- **Thread-safety**: Safe for concurrent calls only if external synchronization is applied on shared sample data.

## Usage
