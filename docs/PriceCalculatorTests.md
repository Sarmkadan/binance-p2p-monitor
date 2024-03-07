# PriceCalculatorTests

Unit tests for `PriceCalculator`, a utility class that provides financial calculations and validation methods for price monitoring in the Binance P2P system. The tests verify correct behavior for percentage change, spread, moving averages, standard deviation, price formatting, and input validation across various edge cases.

## API

### `CalculatePercentageChange_PriceIncreases_ReturnsPositivePercentage`
Tests that when the new price is higher than the original, the returned percentage change is positive. No parameters are required; the test uses hardcoded values.

### `CalculatePercentageChange_PriceDecreases_ReturnsNegativePercentage`
Verifies that a price decrease results in a negative percentage change. Uses fixed test values to ensure deterministic behavior.

### `CalculatePercentageChange_ZeroOriginalPrice_ReturnsZero`
Ensures that division by zero is avoided when the original price is zero, returning zero instead of throwing an exception.

### `CalculateSpread_BuyAndSellPrices_ReturnsCorrectSpreadPercent`
Validates that the spread percentage is correctly calculated as `(sellPrice - buyPrice) / buyPrice * 100`. Uses sample buy and sell prices from the test.

### `CalculateSpread_ZeroBuyPrice_ReturnsZero`
Confirms that when the buy price is zero, the spread calculation returns zero to prevent division by zero errors.

### `CalculateMidPrice_TwoPrices_ReturnsArithmeticMean`
Checks that the mid price is the arithmetic mean of two given prices. Uses two test prices to compute the expected average.

### `CalculateMovingAverage_FewerPricesThanPeriod_ReturnsOverallAverage`
Ensures that when the number of prices is less than the specified period, the moving average returns the average of all available prices.

### `CalculateMovingAverage_ExactPeriod_ReturnsLastNAverage`
Validates that when the number of prices exactly matches the period, the moving average returns the average of the last N prices.

### `CalculateStandardDeviation_SinglePrice_ReturnsZero`
Confirms that the standard deviation of a single price is zero, as there is no deviation from the mean.

### `CalculateStandardDeviation_IdenticalPrices_ReturnsZero`
Ensures that when all prices are identical, the standard deviation is zero, indicating no variation.

### `FormatPrice_WithCurrencySymbol_PrependsCurrencySymbol`
Tests that a price formatted with a currency symbol correctly prepends the symbol to the decimal value.

### `FormatPrice_NoCurrencySymbol_ReturnsPlainDecimal`
Verifies that formatting a price without a currency symbol returns the plain decimal representation.

### `IsValidEmail_VariousInputs_ReturnsExpectedResult`
Evaluates the email validation logic against a set of valid and invalid email strings, asserting expected boolean outcomes.

### `IsValidTicker_VariousInputs_ReturnsExpectedResult`
Tests the ticker validation logic using various inputs, ensuring only valid tickers are accepted.

### `IsValidFiatCode_VariousInputs_ReturnsExpectedResult`
Validates the fiat currency code validation logic with a range of inputs, confirming correct acceptance or rejection.

### `IsValidPrice_PriceWithinDefaultRange_ReturnsTrue`
Checks that a price within the default valid range (e.g., between 0.01 and 1,000,000) is accepted as valid.

### `IsValidPrice_ZeroPrice_ReturnsFalse`
Ensures that a zero price is considered invalid and rejected by the validation logic.

### `IsValidTelegramChatId_PositiveId_ReturnsTrue`
Confirms that positive Telegram chat IDs are accepted as valid.

### `IsValidTelegramChatId_ZeroOrNegative_ReturnsFalse`
Verifies that zero or negative Telegram chat IDs are rejected as invalid.

### `IsValidDateRange_StartBeforeEnd_ReturnsTrue`
Tests that a date range where the start date is before the end date is considered valid.

## Usage
