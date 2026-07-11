# PriceHistoryServiceTests

Unit tests for `PriceHistoryService`, verifying price trend calculation, historical data statistics, and repository interaction logic. Focuses on edge cases around empty datasets, single entries, and invalid constructor arguments.

## API

### `GetPriceTrendAsync_TwoRecordsWithRisingPrice_ReturnsPositiveTrend`
Tests that a positive price trend is correctly identified when two records show an increasing price. Validates the linear regression slope calculation and ensures the returned trend value is positive.

### `GetPriceTrendAsync_SingleRecord_ReturnsZero`
Ensures that a single historical price record cannot produce a meaningful trend. Returns zero, indicating no trend can be derived from a single data point.

### `GetPriceTrendAsync_EmptyHistory_ReturnsZero`
Confirms that an empty price history collection results in a zero trend value, preventing division-by-zero or invalid calculations when no data exists.

### `GetPriceStatsAsync_EmptyHistory_ReturnsAllZeroTuple`
Verifies that querying statistics (high, low, average) on an empty history returns a tuple of zeros, avoiding null references or exceptions when no data is present.

### `GetPriceStatsAsync_MultipleRecords_ReturnsCorrectHighLowAverage`
Validates that the correct high, low, and average prices are computed from a non-empty collection of historical prices. Ensures floating-point precision and correct aggregation logic.

### `CleanupOldHistoryAsync_DelegatesToRepository_ReturnsRepoResult`
Tests that the cleanup operation forwards the call to the underlying repository and returns the repository’s result. Ensures proper delegation without modifying behavior.

### `GetHistoryCountAsync_DelegatesToRepository_ReturnsTotalCount`
Confirms that the count request is delegated to the repository and returns the total number of stored price records. Validates proper abstraction layer usage.

### `Constructor_NullRepository_ThrowsArgumentNullException`
Ensures that passing a null repository to the constructor throws an `ArgumentNullException`, enforcing non-null dependencies and fail-fast behavior.

### `Constructor_NullSettings_ThrowsArgumentNullException`
Validates that a null settings object during construction throws an `ArgumentNullException`, enforcing configuration requirements and preventing runtime failures.

## Usage
