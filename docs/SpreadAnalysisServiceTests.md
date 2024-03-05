# SpreadAnalysisServiceTests

Unit test class for `SpreadAnalysisService` that verifies spread calculation and update logic across currency pairs in the Binance P2P monitoring system. Focuses on validating price inputs, spread computation, and exception handling for edge cases such as zero prices or missing data.

## API

### `public SpreadAnalysisServiceTests`

Constructor for the test class. Initializes the test context with mock dependencies for `SpreadAnalysisService`, including price feed and data store abstractions.

### `public async Task AnalyzeSpreadAsync_ValidPrices_ReturnsCorrectSpread`

Validates that the service correctly computes the spread percentage when provided with valid buy and sell prices. Ensures the returned spread reflects the expected formula: `(sellPrice - buyPrice) / buyPrice * 100`.

- **Parameters**: None
- **Return value**: `Task` completing when analysis finishes; asserts the computed spread matches the expected value.
- **Throws**: Only via test assertions when computed spread does not match expected.

### `public async Task AnalyzeSpreadAsync_ZeroBuyPrice_ThrowsInvalidPriceException`

Ensures the service throws `InvalidPriceException` when the buy price is zero or invalid, preventing division by zero and invalid spread calculations.

- **Parameters**: None
- **Return value**: `Task` completing when exception is thrown.
- **Throws**: `InvalidPriceException` if buy price is zero or negative.

### `public async Task UpdateSpreadAsync_ValidSpread_ReturnsTrue`

Confirms that the service successfully updates and persists the spread when given valid price data and a calculable spread.

- **Parameters**: None
- **Return value**: `Task<bool>` resolving to `true` if the spread update succeeds.
- **Throws**: None under valid inputs; test asserts successful return.

### `public async Task UpdateSpreadAsync_InvalidSpread_ThrowsInvalidPriceException`

Validates that the service rejects and throws when the computed spread is invalid (e.g., negative or exceeds maximum threshold), ensuring data integrity.

- **Parameters**: None
- **Return value**: `Task` completing when exception is thrown.
- **Throws**: `InvalidPriceException` if the spread is invalid.

### `public async Task GetCrossCurrencySpreadAsync_ValidData_ReturnsSpread`

Tests retrieval of the current spread between two currency pairs when valid data is available. Ensures the returned value matches the expected computed spread.

- **Parameters**: None
- **Return value**: `Task<decimal>` resolving to the computed spread.
- **Throws**: None under valid inputs; test asserts expected spread.

### `public async Task GetCrossCurrencySpreadAsync_MissingData_ReturnsNull`

Verifies that the service returns `null` when required price data is missing for one or both currency pairs, avoiding invalid computations.

- **Parameters**: None
- **Return value**: `Task<decimal?>` resolving to `null` if data is missing.
- **Throws**: None; returns `null` gracefully.

## Usage
