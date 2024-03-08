# PriceCalculatorEdgeCaseTests

Unit tests for the `PriceCalculator` class, focusing on edge cases and boundary conditions in price calculation methods. These tests validate behavior when inputs are zero, null, empty, or otherwise invalid, ensuring robustness against common failure modes in financial calculations.

## API

### `CalculatePercentageChange_ShouldReturnZero_WhenOriginalPriceIsZero`
Ensures that when the original price is zero, the percentage change calculation returns zero to avoid division-by-zero errors. This test verifies that the method handles the edge case where the base value is zero without throwing exceptions.

### `CalculatePercentageChange_ShouldReturnCorrectChange_ForVariousValues`
Validates that the percentage change calculation returns the correct value for a variety of input pairs, including positive, negative, and zero price changes. This test confirms the accuracy of the formula under normal and edge-case numeric conditions.

### `CalculateSpread_ShouldReturnZero_WhenBuyPriceIsZero`
Confirms that when the buy price is zero, the spread calculation returns zero, preventing invalid spread computations. This test ensures the method gracefully handles a zero buy price without producing incorrect results.

### `CalculateSpread_ShouldReturnCorrectSpread_ForVariousPrices`
Verifies that the spread calculation returns the correct value for various combinations of buy and sell prices, including edge cases where prices are equal or differ by small amounts. This test ensures the spread formula is accurate across a range of realistic inputs.

### `CalculateMovingAverage_ShouldReturnZero_WhenPricesIsEmpty`
Checks that the moving average calculation returns zero when the prices collection is empty, avoiding invalid operations on empty data. This test ensures the method handles empty input collections predictably.

### `CalculateMovingAverage_ShouldThrowArgumentNullException_WhenPricesIsNull`
Ensures that the moving average method throws an `ArgumentNullException` when the prices collection is null, enforcing input validation. This test confirms the method fails fast and clearly when given invalid input.

### `CalculateMovingAverage_ShouldThrowArgumentOutOfRangeException_WhenPeriodIsZeroOrNegative`
Validates that the moving average method throws an `ArgumentOutOfRangeException` when the period is zero or negative, preventing invalid window sizes. This test ensures the method enforces constraints on the period parameter.

### `CalculateMovingAverage_ShouldReturnCorrectAverage_WhenPeriodGreaterThanCount`
Confirms that the moving average calculation returns the correct average when the period exceeds the number of prices, effectively computing the average of all available prices. This test verifies behavior under a common edge case in time-series analysis.

### `CalculateStandardDeviation_ShouldReturnZero_WhenPricesIsEmptyOrSingleItem`
Ensures that the standard deviation calculation returns zero when the prices collection is empty or contains only one item, as no meaningful deviation can be computed. This test prevents invalid statistical operations on insufficient data.

### `CalculateStandardDeviation_ShouldThrowArgumentNullException_WhenPricesIsNull`
Validates that the standard deviation method throws an `ArgumentNullException` when the prices collection is null, enforcing input validation. This test ensures the method fails fast and clearly when given invalid input.

### `CalculateStandardDeviation_ShouldReturnCorrectStandardDeviation`
Confirms that the standard deviation calculation returns the correct value for a set of prices, validating the statistical computation under normal conditions. This test ensures the method produces accurate results for valid inputs.

## Usage
