# PerformanceMetrics

`PerformanceMetrics` is a utility class designed to track and report the performance characteristics of repeated operations, such as API calls or network requests. It records success and failure counts, execution durations, and timestamps to provide insights into operational reliability and latency. This class is particularly useful for monitoring recurring tasks in applications like `binance-p2p-monitor`, where consistent performance is critical.

## API

### `void RecordOperation(bool success, TimeSpan duration)`
Records the outcome and duration of a single operation.

- **Parameters**
  - `success` – `true` if the operation succeeded; otherwise, `false`.
  - `duration` – The time taken to complete the operation.
- **Throws**
  - `ArgumentOutOfRangeException` – If `duration` is negative.

### `OperationMetrics? GetMetrics()`
Retrieves the aggregated metrics for the operation tracked by this instance.

- **Returns**
  - An `OperationMetrics` object containing aggregated data (e.g., success/failure counts, durations), or `null` if no operations have been recorded.
- **Remarks**
  - The returned object is a snapshot and will not reflect subsequent changes.

### `IReadOnlyDictionary<string, OperationMetrics> GetAllMetrics()`
Retrieves metrics for all operations tracked by this instance.

- **Returns**
  - A read-only dictionary mapping operation names to their respective `OperationMetrics` objects.
- **Remarks**
  - Returns an empty dictionary if no operations have been recorded.

### `string GenerateReport()`
Generates a human-readable report summarizing the tracked performance metrics.

- **Returns**
  - A formatted string containing key metrics such as total operations, success/failure rates, and duration statistics.
- **Remarks**
  - Includes timestamps and derived statistics (e.g., average duration) if applicable.

### `void Clear()`
Resets all recorded metrics to their initial state.

- **Remarks**
  - After calling this method, all counters and timestamps are reset to zero or default values.

### `string OperationName` (property)
Gets the name of the operation being tracked.

- **Returns**
  - The name of the operation (e.g., `"PlaceOrder"`, `"FetchBalance"`).

### `int TotalCount` (property)
Gets the total number of operations recorded.

- **Returns**
  - The cumulative count of all operations, including successes and failures.

### `int SuccessCount` (property)
Gets the number of successful operations recorded.

- **Returns**
  - The count of operations where `success` was `true`.

### `int FailureCount` (property)
Gets the number of failed operations recorded.

- **Returns**
  - The count of operations where `success` was `false`.

### `TimeSpan TotalDuration` (property)
Gets the cumulative duration of all recorded operations.

- **Returns**
  - The sum of all `duration` values passed to `RecordOperation`.

### `TimeSpan MinDuration` (property)
Gets the shortest recorded operation duration.

- **Returns**
  - The minimum `duration` observed, or `TimeSpan.Zero` if no operations have been recorded.

### `TimeSpan MaxDuration` (property)
Gets the longest recorded operation duration.

- **Returns**
  - The maximum `duration` observed, or `TimeSpan.Zero` if no operations have been recorded.

### `DateTime LastExecutionTime` (property)
Gets the timestamp of the most recent operation recorded.

- **Returns**
  - The `DateTime` of the last call to `RecordOperation`, or `DateTime.MinValue` if no operations have been recorded.

## Usage

### Example 1: Basic Monitoring
```csharp
var metrics = new PerformanceMetrics("PlaceOrder");

metrics.RecordOperation(true, TimeSpan.FromMilliseconds(120));
metrics.RecordOperation(false, TimeSpan.FromMilliseconds(340));
metrics.RecordOperation(true, TimeSpan.FromMilliseconds(95));

Console.WriteLine(metrics.GenerateReport());
/*
Output:
Operation: PlaceOrder
Total Operations: 3
Success Rate: 66.67%
Failures: 1
Total Duration: 00:00:0.555
Average Duration: 00:00:0.185
Min Duration: 00:00:0.095
Max Duration: 00:00:0.340
Last Execution: 2024-06-14 12:34:56
*/
```

### Example 2: Multi-Operation Tracking
```csharp
var tracker = new PerformanceMetrics("APIClient");

tracker.RecordOperation(true, TimeSpan.FromSeconds(1));
tracker.RecordOperation(true, TimeSpan.FromSeconds(1.5));

var allMetrics = tracker.GetAllMetrics();
foreach (var kvp in allMetrics)
{
    Console.WriteLine($"Operation: {kvp.Key}");
    Console.WriteLine($"Successes: {kvp.Value.SuccessCount}");
    Console.WriteLine($"Failures: {kvp.Value.FailureCount}");
}
```

## Notes

- **Thread Safety**: This class is **not thread-safe**. Concurrent calls to `RecordOperation`, `Clear`, or property accessors may result in inconsistent or corrupted metrics. External synchronization (e.g., `lock`) is required if used in multi-threaded contexts.
- **Edge Cases**:
  - If `RecordOperation` is called with a negative `duration`, an `ArgumentOutOfRangeException` is thrown immediately.
  - Properties like `MinDuration` and `MaxDuration` return `TimeSpan.Zero` when no operations have been recorded, which may require explicit handling in reporting logic.
  - `LastExecutionTime` reflects the last call to `RecordOperation`, even if subsequent calls to `Clear` are made. Reset logic should account for this behavior.
