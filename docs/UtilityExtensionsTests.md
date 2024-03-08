# UtilityExtensionsTests

The `UtilityExtensionsTests` class serves as the comprehensive test suite for the `UtilityExtensions` helper class within the `binance-p2p-monitor` project. It validates the correctness, reliability, and edge-case handling of various string manipulation, numerical calculation, date conversion, and validation extension methods used throughout the application. By employing a data-driven or arrangement-act-assert pattern, this class ensures that utility functions behave consistently under normal conditions and handle invalid inputs gracefully, maintaining data integrity for Binance P2P monitoring operations.

## API

### `ToUnixTimestampMs_ShouldReturnCorrectTimestamp`
Verifies that the extension method correctly converts a `DateTime` object to a Unix timestamp represented in milliseconds.
*   **Purpose**: Ensures accurate time serialization for API requests and logging.
*   **Parameters**: Implicitly utilizes a specific `DateTime` instance defined within the test arrangement.
*   **Return Value**: `void` (Asserts equality between the calculated result and the expected long integer).
*   **Throws**: Fails the test assertion if the calculated timestamp deviates from the expected value.

### `GetTimeAgoString_ShouldReturnCorrectString_ForMinutes`
Validates the logic that generates human-readable relative time strings (e.g., "5 minutes ago") based on a time difference measured in minutes.
*   **Purpose**: Guarantees correct formatting for UI displays showing trade or order age.
*   **Parameters**: Implicitly uses a `TimeSpan` or `DateTime` pair representing the interval.
*   **Return Value**: `void` (Asserts the formatted string matches the expected pattern).
*   **Throws**: Fails the test assertion if the string format or unit (minutes vs. hours) is incorrect.

### `Chunk_ShouldReturnCorrectChunks`
Tests the functionality that splits a collection or string into smaller, equally sized chunks, handling remainders appropriately.
*   **Purpose**: Supports batch processing of large datasets, such as splitting ticker lists for API rate limit compliance.
*   **Parameters**: Implicitly defines a source collection and a chunk size integer.
*   **Return Value**: `void` (Asserts the count of resulting chunks and the content of each chunk).
*   **Throws**: Fails the test assertion if chunk sizes are inconsistent or data is lost during splitting.

### `FirstOrNull_ShouldReturnFirstItemOrNull`
Confirms that the method returns the first element of a sequence if it exists, or `null` (for reference types) / `null` (for nullable value types) if the sequence is empty, without throwing an exception.
*   **Purpose**: Provides a safe alternative to `First()` that avoids `InvalidOperationException` on empty collections.
*   **Parameters**: Implicitly tests against both populated and empty enumerables.
*   **Return Value**: `void` (Asserts the returned value is the expected item or null).
*   **Throws**: Fails the test assertion if an exception is thrown on empty sequences or the wrong item is returned.

### `RoundTo_ShouldRoundDecimalCorrectly`
Ensures decimal values are rounded to a specified number of decimal places using standard or specific banking rounding rules required for financial calculations.
*   **Purpose**: Maintains precision integrity for currency values and percentage calculations.
*   **Parameters**: Implicitly uses a `decimal` input and an integer representing the precision.
*   **Return Value**: `void` (Asserts the rounded result matches the expected decimal).
*   **Throws**: Fails the test assertion if rounding errors occur or precision is lost incorrectly.

### `CalculatePercentageChange_ShouldReturnCorrectChange`
Validates the mathematical formula for calculating the percentage change between an original value and a new value.
*   **Purpose**: Critical for determining price movement trends in P2P market analysis.
*   **Parameters**: Implicitly uses two decimal values (old price, new price).
*   **Return Value**: `void` (Asserts the calculated percentage matches the expected decimal result).
*   **Throws**: Fails the test assertion if the calculation is mathematically incorrect; may verify behavior when the original value is zero.

### `Truncate_ShouldTruncateStringCorrectly`
Tests the method that shortens a string to a maximum length, optionally appending an ellipsis or suffix if truncation occurs.
*   **Purpose**: Prevents database overflow and ensures UI consistency for long ticker names or descriptions.
*   **Parameters**: Implicitly uses a source string and a maximum length integer.
*   **Return Value**: `void` (Asserts the output string length and content).
*   **Throws**: Fails the test assertion if the string exceeds the limit or is truncated unnecessarily.

### `ToSnakeCase_ShouldConvertCorrectly`
Verifies the conversion of strings from PascalCase or camelCase to snake_case format.
*   **Purpose**: Facilitates compatibility with external APIs or database columns that utilize snake_case naming conventions.
*   **Parameters**: Implicitly uses various casing input strings.
*   **Return Value**: `void` (Asserts the converted string matches the expected snake_case format).
*   **Throws**: Fails the test assertion if capitalization or underscore placement is incorrect.

### `IsValidEmail_ShouldReturnCorrectResult`
Tests the regular expression or logic used to validate email address formats.
*   **Purpose**: Ensures user notification settings or account data contain valid contact information.
*   **Parameters**: Implicitly iterates through a set of valid and invalid email string samples.
*   **Return Value**: `void` (Asserts boolean true/false results against expected validity).
*   **Throws**: Fails the test assertion if a valid email is rejected or an invalid email is accepted.

### `IsValidTicker_ShouldReturnCorrectResult`
Validates the format of cryptocurrency ticker symbols (e.g., "BTCUSDT", "ETHBUSD") according to Binance P2P standards.
*   **Purpose**: Prevents invalid asset queries and ensures data filtering accuracy.
*   **Parameters**: Implicitly tests a range of compliant and non-compliant ticker strings.
*   **Return Value**: `void` (Asserts boolean validity results).
*   **Throws**: Fails the test assertion if ticker validation logic allows malformed symbols.

## Usage

The following examples demonstrate how the methods tested by `UtilityExtensionsTests` are typically consumed in the production codebase.

```csharp
// Example 1: Processing market data with safe collection handling and formatting
var recentTrades = GetRecentTrades(); // Returns List<Trade>

// Safely get the latest trade without throwing if the list is empty
var latestTrade = recentTrades.FirstOrNull();

if (latestTrade != null)
{
    // Calculate price movement
    var change = latestTrade.Price.CalculatePercentageChange(latestTrade.PreviousPrice);
    
    // Format the time for display
    var timeString = latestTrade.Timestamp.GetTimeAgoString();
    
    Console.WriteLine($"Ticker: {latestTrade.Ticker.ToSnakeCase()}, Change: {change:P2}, Time: {timeString}");
}
```

```csharp
// Example 2: Preparing batch requests and validating user input
string userInputEmail = "trader@example.com";
string tickerInput = "BTCUSDT";

// Validate inputs before processing
if (userInputEmail.IsValidEmail() && tickerInput.IsValidTicker())
{
    // Prepare a large list of IDs to send to an API with batch limits
    var ids = Enumerable.Range(1, 150).Select(i => i.ToString());
    
    // Chunk into batches of 50
    var batches = ids.Chunk(50);
    
    foreach (var batch in batches)
    {
        var timestamp = DateTime.UtcNow.ToUnixTimestampMs();
        SendBatchRequest(batch, timestamp);
    }
}
```

## Notes

*   **Edge Cases**: The `Chunk` method tests must verify behavior when the collection size is not evenly divisible by the chunk size, ensuring the final chunk contains the remaining elements. `CalculatePercentageChange` tests should explicitly cover scenarios where the original value is zero to confirm division-by-zero handling (either returning infinity, zero, or throwing a specific handled exception depending on implementation). `Truncate` tests must ensure that strings shorter than the limit remain unmodified.
*   **Thread Safety**: As `UtilityExtensionsTests` validates static extension methods that typically operate on immutable types (strings, decimals, structs) or pure functional logic without shared mutable state, the underlying methods are inherently thread-safe. No synchronization context is required for these utilities.
*   **Precision**: Financial calculations in `RoundTo` and `CalculatePercentageChange` rely on the `decimal` type. Tests ensure that floating-point inaccuracies associated with `double` do not propagate into monetary values.
*   **Culture Sensitivity**: String conversions like `ToSnakeCase` and number formatting should ideally be culture-invariant. The tests imply verification that results are consistent regardless of the executing thread's current culture settings.
