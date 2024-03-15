# PriceCalculatorTestsExtensions

Utility class providing extension and helper methods for generating and validating price sequences and spreads in unit tests for `PriceCalculator`.

## API

### `GenerateLinearPriceSequence`

Generates a sequence of prices with a linear progression between a start and end value.

- **Parameters**
  - `start` (decimal): The starting price of the sequence.
  - `end` (decimal): The ending price of the sequence.
  - `count` (int): The number of prices to generate.
- **Returns**
  - `decimal[]`: An array of `count` prices linearly interpolated between `start` and `end`.
- **Throws**
  - `ArgumentOutOfRangeException`: Thrown if `count` is less than 1.

---

### `GenerateExponentialPriceSequence`

Generates a sequence of prices with an exponential progression between a start and end value.

- **Parameters**
  - `start` (decimal): The starting price of the sequence.
  - `end` (decimal): The ending price of the sequence.
  - `count` (int): The number of prices to generate.
- **Returns**
  - `decimal[]`: An array of `count` prices exponentially interpolated between `start` and `end`.
- **Throws**
  - `ArgumentOutOfRangeException`: Thrown if `count` is less than 1.

---

### `CalculateCumulativePercentageChange`

Calculates the cumulative percentage change from the first to the last price in a sequence.

- **Parameters**
  - `prices` (decimal[]): The sequence of prices.
- **Returns**
  - `decimal`: The cumulative percentage change as a decimal value (e.g., 0.05 for 5%).
- **Throws**
  - `ArgumentNullException`: Thrown if `prices` is `null`.
  - `ArgumentException`: Thrown if `prices` is empty or contains only one element.

---
### `CalculateAverageSpread`

Calculates the average spread between consecutive prices in a sequence.

- **Parameters**
  - `this decimal[] prices`: The sequence of prices.
- **Returns**
  - `decimal`: The average spread between consecutive prices.
- **Throws**
  - `ArgumentNullException`: Thrown if `prices` is `null`.
  - `ArgumentException`: Thrown if `prices` has fewer than 2 elements.

---
### `GenerateVolatilePriceSequence`

Generates a sequence of prices with random volatility around a base value.

- **Parameters**
  - `basePrice` (decimal): The central price around which volatility is applied.
  - `volatilityPercent` (decimal): The maximum percentage deviation (e.g., 0.1 for ±10%).
  - `count` (int): The number of prices to generate.
- **Returns**
  - `decimal[]`: An array of `count` prices with random fluctuations within the specified volatility.
- **Throws**
  - `ArgumentOutOfRangeException`: Thrown if `volatilityPercent` is negative or `count` is less than 1.

---
### `FormatPriceArray`

Formats an array of prices into a human-readable string representation.

- **Parameters**
  - `prices` (decimal[]): The sequence of prices to format.
- **Returns**
  - `string`: A formatted string showing the prices, e.g., `[100.00, 101.50, 102.25]`.
- **Throws**
  - `ArgumentNullException`: Thrown if `prices` is `null`.

---
### `ShouldBeWithinTolerance`

Determines whether a calculated value is within an acceptable tolerance of an expected value.

- **Parameters**
  - `actual` (decimal): The value to check.
  - `expected` (decimal): The expected value.
  - `tolerance` (decimal): The maximum allowed deviation (absolute value).
- **Returns**
  - `bool`: `true` if `actual` is within `±tolerance` of `expected`; otherwise, `false`.
- **Throws**
  - `ArgumentOutOfRangeException`: Thrown if `tolerance` is negative.

---
### `GenerateSpreadTestCases`

Generates a set of test cases with varying buy and sell prices for spread validation.

- **Parameters**
  - `basePrice` (decimal): The base price used to derive test cases.
  - `spreadVariations` (decimal[]): An array of spread percentages to apply (e.g., `[0.01, 0.02]`).
- **Returns**
  - `(decimal BuyPrice, decimal SellPrice)[]`: An array of tuples representing buy and sell prices for each spread variation.
- **Throws**
  - `ArgumentNullException`: Thrown if `spreadVariations` is `null`.
  - `ArgumentException`: Thrown if `spreadVariations` is empty.

## Usage

### Example 1: Testing linear price interpolation
