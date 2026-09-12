# DatabaseCleanupService

`DatabaseCleanupService` is the service-layer implementation of `IDatabaseCleanupService`. It wraps `IHistoryRepository` to delete old history records from the database and to report the total number of history records.

- **Namespace:** `BinanceP2pMonitor.Services`
- **File:** `src/Services/DatabaseCleanupService.cs`
- **Implements:** `IDatabaseCleanupService`

## Purpose

The service provides a single place for database cleanup operations. It delegates the actual persistence work to `IHistoryRepository`, adds logging, and translates unexpected failures into `DataAccessException` so callers can handle data-access errors uniformly.

## Dependencies

| Dependency | Purpose |
| --- | --- |
| `IHistoryRepository` | Performs the underlying count and delete operations. |
| `ILogger<DatabaseCleanupService>` | Logs cleanup activity and errors. |

Both dependencies are injected via the constructor and are required (an `ArgumentNullException` is thrown if either is `null`).

## Public Methods

### `Task<int> DeleteOldRecordsAsync(int daysOld)`

Deletes history records older than the specified number of days.

- **Parameter `daysOld`:** Number of days to keep. Records older than this are deleted.
- **Returns:** The number of records deleted.

Behavior:

- Throws `ArgumentException` if `daysOld` is negative (a warning is logged first).
- Reads the total history count before and after the delete, then returns the difference as the number of deleted records.
- On any exception, logs an error and rethrows as `DataAccessException`.

### `Task<long> GetTotalHistoryCountAsync()`

Gets the total number of history records in the database.

- **Returns:** The total count of history records.

Behavior:

- Delegates directly to `IHistoryRepository.GetTotalHistoryCountAsync()`.
- On any exception, logs an error and rethrows as `DataAccessException`.

## Usage Example

```csharp
using BinanceP2pMonitor.Services;

// Resolve from DI (or construct directly with a repository and logger).
IDatabaseCleanupService cleanupService = serviceProvider.GetRequiredService<IDatabaseCleanupService>();

// Delete records older than 30 days.
int deleted = await cleanupService.DeleteOldRecordsAsync(30);

// Report the remaining total.
long total = await cleanupService.GetTotalHistoryCountAsync();

Console.WriteLine($"Deleted {deleted} records; {total} remain.");
```