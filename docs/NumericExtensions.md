# NumericExtensions

Static helper class that provides a collection of pure‑function extensions for common numeric operations used throughout the binance‑p2p‑monitor application. The methods are stateless, side‑effect free, and safe to call from any thread.

## API

### RoundTo
**Purpose** – Rounds a decimal value to the specified number of fractional digits.  
**Parameters**  
- `value` (decimal): The number to round.  
- `digits` (int): Number of decimal places to retain; must be between 0 and 28 inclusive.  
**Return value** – A decimal rounded to `digits` places using midpoint‑away‑from‑zero rounding.  
**Exceptions** – Throws `ArgumentOutOfRangeException` if `digits` is less than 0 or greater than 28.

### IsWithinPercentage
**Purpose** – Determines whether a value lies within a given percentage tolerance of a reference value.  
**Parameters**  
- `value` (decimal): The value to test.  
- `reference` (decimal): The baseline value for comparison.  
- `percent` (decimal): Allowed tolerance expressed as a percentage (e.g., 5 for ±5%). Must be non‑negative.  
**Return value** – `true` if `|value - reference| ≤ reference * percent / 100`; otherwise `false`.  
**Exceptions** – Throws `ArgumentOutOfRangeException` if `percent` is negative.

### CalculatePercentageChange
**Purpose** – Computes the relative change from an old value to a new value as a percentage.  
**Parameters**  
- `oldValue` (decimal): The starting value. Must not be zero.  
- `newValue` (decimal): The ending value.  
**Return value** – The percentage change: `((newValue - oldValue) / oldValue) * 100`.  
**Exceptions** – Throws `DivideByZeroException` if `oldValue` is zero.

### Clamp
**Purpose** – Restricts a value to lie within a specified inclusive range.  
**Parameters**  
- `value` (decimal): The value to clamp.  
- `min` (decimal): The lower bound of the range.  
- `max` (decimal): The upper bound of the range; must be greater than or equal to `min`.  
**Return value** – `value` if it lies within `[min, max]`; otherwise `min` if `value < min` or `max` if `value > max`.  
**Exceptions** – Throws `ArgumentException` if `min` is greater than `max`.

### IsPositive
**Purpose** – Checks whether a decimal is strictly greater than zero.  
**Parameters**  
- `value` (decimal): The number to test.  
**Return value** – `true` if `value > 0`; otherwise `false`.  
**Exceptions** – None.

### IsNegative
**Purpose** – Checks whether a decimal is strictly less than zero.  
**Parameters**  
- `value` (decimal): The number to test.  
**Return value** – `true` if `value < 0`; otherwise `false`.  
**Exceptions** – None.

### IsBetween
**Purpose** – Determines whether a value falls within a given interval.  
**Parameters**  
- `value` (decimal): The number to test.  
- `low` (decimal): The lower bound of the interval.  
- `high` (decimal): The upper bound of the interval; must be greater than or equal to `low`.  
**Return value** – `true` if `low ≤ value ≤ high`; otherwise `false`.  
**Exceptions** – Throws `ArgumentException` if `low` is greater than `high`.

### AbsolutePercentageDifference
**Purpose** – Returns the absolute difference between two values expressed as a percentage of their average.  
**Parameters**  
- `first` (decimal): First value.  
- `second` (decimal): Second value.  
**Return value** – `2 * |first - second| / (|first| + |second|) * 100`. If both values are zero, returns 0.  
**Exceptions** – None.

### ToCurrencyString
**Purpose** – Formats a decimal as a currency string using a specified symbol and two decimal places.  
**Parameters**  
- `value` (decimal): The amount to format.  
- `symbol` (string, optional): Currency symbol to prefix; defaults to `"$"`.  
**Return value** – A string in the form `{symbol}{value:N2}` (e.g., `"$123.45"`).  
**Exceptions** – Throws `ArgumentNullException` if `symbol` is `null`.

### FormatPrecision
**Purpose** – Formats a decimal with a specific number of decimal places, preserving trailing zeros.  
**Parameters**  
- `value` (decimal): The number to format.  
- `precision` (int): Number of decimal places to display; must be between 0 and 28 inclusive.  
**Return value** – A string representation of `value` with exactly `precision` digits after the decimal point.  
**Exceptions** – Throws `ArgumentOutOfRangeException` if `precision` is less than 0 or greater than 28.

## Usage

```csharp
using BinanceP2pMonitor.Utils; // namespace containing NumericExtensions

// Example 1: Determine if a trade price is within 0.5% of the market price.
decimal marketPrice = 31250.75m;
decimal tradePrice   = 31300.00m;
bool ok = NumericExtensions.IsWithinPercentage(tradePrice, marketPrice, 0.5m);
// ok is true because the difference is within the tolerance.

// Example 2: Format a balance for display in the UI.
decimal balance = 0.00456m;
string formatted = NumericExtensions.FormatPrecision(balance, 6);
// formatted == "0.004560"
```

## Notes

- All methods are pure functions; they do not modify any external state and have no side effects, making them inherently thread‑safe.thread‑safe.  
- Rounding follows the midpoint‑away‑from‑zero rule (the default for `System.Math.Round`).  
- Percentage‑based methods (`IsWithinPercentage`, `CalculatePercentageChange`, `AbsolutePercentageDifference`) guard against division by zero; callers should ensure reference values are non‑zero where required.  
- The `Clamp` and `IsBetween` methods require that the lower bound not exceed the upper bound; violating this precondition results in an `ArgumentException`.  
- Currency formatting assumes a fixed two‑decimal‑place representation; for currencies with different subunit sizes, adjust the precision before calling `ToCurrencyString`.  
- When formatting with `FormatPrecision`, specifying a precision greater than the number of significant digits of the value will pad with zeros; specifying a precision of zero yields an integer string without a decimal point.
