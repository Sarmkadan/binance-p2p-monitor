# HistoryRepository

The `HistoryRepository` class provides asynchronous data access operations for managing historical price records within the `binance-p2p-monitor` system. It serves as the primary interface for retrieving, storing, and aggregating `PriceHistory` entities, supporting queries by unique identifier, asset/fiat pairs, date ranges, and recency, while also offering utilities for data maintenance and statistical analysis such as highest and lowest price retrieval.

## API

### `public HistoryRepository`
Initializes a new instance of the `HistoryRepository` class. This constructor typically injects required dependencies such as database contexts or configuration settings necessary for data persistence.

### `public async Task<PriceHistory?> GetByIdAsync`
Retrieves a single price history record by its unique identifier.
*   **Parameters**: Expects a unique identifier (typically `long` or `string`, depending on the entity definition) corresponding to the record to fetch.
*   **Return Value**: Returns a `Task` containing the `PriceHistory` object if found, or `null` if no record matches the provided ID.
*   **Exceptions**: May throw database-related exceptions if the underlying data store is unreachable or if a query timeout occurs.

### `public async Task<IEnumerable<PriceHistory>> GetHistoryByAssetAndFiatAsync`
Fetches a collection of price history records filtered by a specific cryptocurrency asset and fiat currency pair.
*   **Parameters**: Requires the asset symbol (e.g., "USDT") and the fiat currency code (e.g., "USD").
*   **Return Value**: Returns a `Task` containing an enumerable collection of `PriceHistory` objects matching the criteria. Returns an empty collection if no matches are found.
*   **Exceptions**: Throws if the database connection fails or if invalid currency codes cause a query error.

### `public async Task<IEnumerable<PriceHistory>> GetRecentHistoryAsync`
Retrieves the most recently added price history records across all assets.
*   **Parameters**: May accept an optional count parameter to limit the number of records returned; if omitted, a default limit applies.
*   **Return Value**: Returns a `Task` containing an enumerable collection of `PriceHistory` objects ordered by timestamp in descending order.
*   **Exceptions**: Throws on data access failures or if the sorting mechanism encounters invalid data states.

### `public async Task<IEnumerable<PriceHistory>> GetHistoryByDateRangeAsync`
Queries price history records falling within a specific start and end time.
*   **Parameters**: Requires `DateTime` objects defining the start and end of the range.
*   **Return Value**: Returns a `Task` containing an enumerable collection of `PriceHistory` objects where the timestamp falls within the specified range (inclusive).
*   **Exceptions**: Throws if the start date is later than the end date or if database errors occur during execution.

### `public async Task<int> AddAsync`
Persists a new `PriceHistory` record to the data store.
*   **Parameters**: Accepts a `PriceHistory` object containing the data to be inserted.
*   **Return Value**: Returns a `Task` containing an `int` representing the number of state changes committed to the database (typically `1` on success).
*   **Exceptions**: Throws if the entity violates unique constraints, contains invalid data, or if the database transaction fails.

### `public async Task<bool> DeleteOldRecordsAsync`
Removes historical records older than a specified threshold to manage storage growth.
*   **Parameters**: Requires a `DateTime` cutoff point; records prior to this date are targeted for deletion.
*   **Return Value**: Returns a `Task` containing a `bool` indicating whether the operation completed successfully (`true`) or failed (`false`).
*   **Exceptions**: Generally catches internal errors to return `false`, but may throw critical exceptions if the database schema is locked or inaccessible.

### `public async Task<long> GetTotalHistoryCountAsync`
Calculates the total number of price history records currently stored.
*   **Parameters**: No parameters required.
*   **Return Value**: Returns a `Task` containing a `long` representing the total row count.
*   **Exceptions**: Throws if the count query cannot be executed due to database connectivity issues.

### `public async Task<decimal> GetHighestPriceAsync`
Retrieves the maximum price value recorded for a specific context (globally or filtered by asset/fiat if overloads exist).
*   **Parameters**: May require asset and fiat identifiers depending on implementation scope.
*   **Return Value**: Returns a `Task` containing a `decimal` representing the highest price found. Returns `0` or throws if no data exists, depending on specific implementation details of the underlying provider.
*   **Exceptions**: Throws if the aggregation query fails.

### `public async Task<decimal> GetLowestPriceAsync`
Retrieves the minimum price value recorded for a specific context.
*   **Parameters**: May require asset and fiat identifiers depending on implementation scope.
*   **Return Value**: Returns a `Task` containing a `decimal` representing the lowest price found.
*   **Exceptions**: Throws if the aggregation query fails.

## Usage

### Example 1: Storing and Retrieving Recent Data
This example demonstrates adding a new price record and immediately fetching the most recent entries to verify ingestion.

```csharp
var repository = new HistoryRepository(dbContext);

// Create a new history entry
var newRecord = new PriceHistory
{
    Asset = "BTC",
    Fiat = "USD",
    Price = 45000.50m,
    Timestamp = DateTime.UtcNow
};

// Add the record
int rowsAffected = await repository.AddAsync(newRecord);

if (rowsAffected > 0)
{
    // Fetch the last 10 records across all pairs
    var recentHistory = await repository.GetRecentHistoryAsync(10);
    
    foreach (var record in recentHistory)
    {
        Console.WriteLine($"{record.Asset}/{record.Fiat}: {record.Price} at {record.Timestamp}");
    }
}
```

### Example 2: Analyzing Historical Range and Cleanup
This example retrieves data for a specific pair within a date range, calculates extremes, and performs maintenance by deleting old records.

```csharp
var repository = new HistoryRepository(dbContext);
var startDate = DateTime.UtcNow.AddDays(-30);
var endDate = DateTime.UtcNow;

// Get history for ETH/EUR over the last 30 days
var history = await repository.GetHistoryByDateRangeAsync("ETH", "EUR", startDate, endDate);

if (history.Any())
{
    var highest = await repository.GetHighestPriceAsync("ETH", "EUR");
    var lowest = await repository.GetLowestPriceAsync("ETH", "EUR");
    
    Console.WriteLine($"30-Day High: {highest}, Low: {lowest}");
}

// Cleanup records older than 90 days
bool cleanupSuccess = await repository.DeleteOldRecordsAsync(DateTime.UtcNow.AddDays(-90));
if (!cleanupSuccess)
{
    Console.Error.WriteLine("Failed to delete old records.");
}
```

## Notes

*   **Null Handling**: Methods returning single entities (`GetByIdAsync`) explicitly return `null` when no match is found rather than throwing an exception. Callers must handle null coalescing or checking.
*   **Empty Collections**: Query methods returning lists (`GetHistoryByAssetAndFiatAsync`, `GetRecentHistoryAsync`, etc.) return an empty `IEnumerable` rather than `null` if no data matches the criteria.
*   **Thread Safety**: As an asynchronous repository pattern implementation, this class is designed to be stateless regarding request data. However, the underlying database context injected into the constructor may not be thread-safe. It is recommended to instantiate a new `HistoryRepository` (and its corresponding context) per logical unit of work or ensure the context is scoped correctly within the dependency injection container.
*   **Decimal Precision**: Price-related methods (`GetHighestPriceAsync`, `GetLowestPriceAsync`) return `decimal` types to maintain financial precision. Be aware that if no records exist for the aggregation, the behavior may default to `0` or throw; explicit existence checks (e.g., via `GetTotalHistoryCountAsync` or specific filters) are advised before calling aggregation methods on potentially empty datasets.
*   **Deletion Logic**: `DeleteOldRecordsAsync` returns a boolean status rather than the count of deleted rows. This abstraction hides the specific volume of data modification, focusing on the success or failure of the maintenance task.
