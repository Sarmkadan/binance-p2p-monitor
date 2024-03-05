# PriceCalculator

Utility class providing static methods for common price calculations and formatting used in the Binance P2P monitoring context. These methods operate on decimal values representing prices or spreads, ensuring consistent rounding and threshold comparisons.

## API

### `public static decimal CalculatePercentageChange(decimal oldPrice, decimal newPrice)`

Calculates the percentage change between two prices using the formula:
`((newPrice - oldPrice) / oldPrice) * 100`

- **Parameters**
  - `oldPrice`: The reference price value.
  - `newPrice`: The updated price value.
- **Return Value**: The percentage change as a decimal. Returns `0` if `oldPrice` is zero to avoid division by zero.
- **Throws**: No exceptions are thrown; division by zero is handled gracefully.

---

### `public static decimal CalculateSpread(decimal askPrice, decimal bidPrice)`

Computes the absolute difference between ask and bid prices, representing the spread.

- **Parameters**
  - `askPrice`: The lowest current ask price.
  - `bidPrice`: The highest current bid price.
- **Return Value**: The spread as a positive decimal value.
- **Throws**: No exceptions are thrown.

---

### `public static decimal CalculateMidPrice(decimal askPrice, decimal bidPrice)`

Calculates the midpoint between the ask and bid prices.

- **Parameters**
  - `askPrice`: The lowest current ask price.
  - `bidPrice`: The highest current bid price.
- **Return Value**: The midpoint price as a decimal.
- **Throws**: No exceptions are thrown.

---
### `public static bool IsAboveThreshold(decimal value, decimal threshold)`

Determines whether the given value exceeds the specified threshold.

- **Parameters**
  - `value`: The value to compare.
  - `threshold`: The threshold value.
- **Return Value**: `true` if `value` is greater than `threshold`; otherwise, `false`.
- **Throws**: No exceptions are thrown.

---
### `public static bool IsBelowThreshold(decimal value, decimal threshold)`

Determines whether the given value is below the specified threshold.

- **Parameters**
  - `value`: The value to compare.
  - `threshold`: The threshold value.
- **Return Value**: `true` if `value` is less than `threshold`; otherwise, `false`.
- **Throws**: No exceptions are thrown.

---
### `public static decimal RoundPrice(decimal price, int decimals = 2)`

Rounds a price to the specified number of decimal places, defaulting to two decimal places.

- **Parameters**
  - `price`: The price value to round.
  - `decimals`: The number of decimal places to round to. Defaults to `2`.
- **Return Value**: The rounded price as a decimal.
- **Throws**: No exceptions are thrown.

---
### `public static string FormatPrice(decimal price, string format = "F2")`

Formats a price as a string using the specified format string. Defaults to fixed-point notation with two decimal places.

- **Parameters**
  - `price`: The price value to format.
  - `format`: The format string. Defaults to `"F2"`.
- **Return Value**: The formatted price string.
- **Throws**: No exceptions are thrown.

---
### `public static decimal CalculateMovingAverage(IEnumerable<decimal> prices, int period)`

Computes the simple moving average over the most recent `period` prices.

- **Parameters**
  - `prices`: The sequence of price values.
  - `period`: The number of most recent prices to include in the average.
- **Return Value**: The moving average as a decimal. Returns `0` if `prices` is empty or `period` is zero or negative.
- **Throws**: No exceptions are thrown.

---
### `public static decimal CalculateStandardDeviation(IEnumerable<decimal> prices)`

Calculates the sample standard deviation of the provided price values.

- **Parameters**
  - `prices`: The sequence of price values.
- **Return Value**: The standard deviation as a decimal. Returns `0` if fewer than two prices are provided.
- **Throws**: No exceptions are thrown.

## Usage

### Example 1: Monitoring price changes and spreads
