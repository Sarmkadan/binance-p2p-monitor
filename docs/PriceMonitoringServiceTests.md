# PriceMonitoringServiceTests

Unit test suite for the `PriceMonitoringService` class, verifying price retrieval, updates, averaging, and alerting functionality. Tests cover both nominal and edge cases including invalid inputs, missing prices, and significant price changes.

## API

### `PriceMonitoringServiceTests`

Public constructor for the test class. Initializes the test suite with required dependencies and test data.

### `GetCurrentPriceAsync_ShouldReturnPrice_WhenPriceExists`

Verifies that `GetCurrentPriceAsync` returns a valid price when a matching price record exists in the service.

- **Parameters**: None
- **Return value**: `Task` completing when the assertion passes
- **Throws**: Does not throw under normal conditions

### `GetCurrentPriceAsync_ShouldReturnNull_WhenPriceDoesNotExist`

Ensures `GetCurrentPriceAsync` returns `null` when no price record exists for the configured asset pair.

- **Parameters**: None
- **Return value**: `Task` completing when the assertion passes
- **Throws**: Does not throw under normal conditions

### `UpdatePriceAsync_ShouldAddPriceAndRecordHistoryAndCheckAlerts_WhenPriceIsValid`

Confirms that `UpdatePriceAsync` inserts a new price record, appends to the price history, and triggers alert checks when the provided price is valid.

- **Parameters**: None
- **Return value**: `Task` completing when the assertion passes
- **Throws**: Does not throw when the price is valid

### `UpdatePriceAsync_ShouldThrowArgumentException_WhenPriceIsInvalid`

Asserts that `UpdatePriceAsync` throws an `ArgumentException` when the provided price is invalid (e.g., negative or zero).

- **Parameters**: None
- **Return value**: `Task` completing when the assertion passes
- **Throws**: `ArgumentException` when the price is invalid

### `GetAveragePriceAsync_ShouldReturnAveragePrice`

Validates that `GetAveragePriceAsync` computes and returns the correct average price over the recorded history.

- **Parameters**: None
- **Return value**: `Task` completing when the assertion passes
- **Throws**: Does not throw under normal conditions

### `GetPricesWithSignificantChangeAsync_ShouldReturnPricesMeetingThreshold`

Ensures `GetPricesWithSignificantChangeAsync` returns only those prices that exceed the configured significance threshold relative to the current average.

- **Parameters**: None
- **Return value**: `Task` completing when the assertion passes
- **Throws**: Does not throw under normal conditions

## Usage
