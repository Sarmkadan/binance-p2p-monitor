# RetryPolicy

RetryPolicy is a helper type that combines transient‑fault handling with structured logging. It exposes an asynchronous execution wrapper that retries operations based on a pluggable transient‑error check, while also implementing the `ILogger` interface so that retry attempts and outcomes can be logged with scopes, levels, and structured state.

## API

### `public RetryPolicy()`

Initializes a new instance with default retry settings. The instance is ready to be used for executing asynchronous operations and for logging.

### `public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)`

Executes the supplied asynchronous `operation` with retry logic.

- **Parameters**
  - `operation`: A delegate that returns a `Task<T>` representing the work to be retried. The delegate receives a `CancellationToken` that can be used to observe cancellation requests.
  - `cancellationToken`: Optional token to cancel the overall retry process.

- **Return value**: A `Task<T>` that completes with the result of `operation` when it succeeds, or after the configured number of retry attempts.

- **Exceptions**
  - `OperationCanceledException` if `cancellationToken` is triggered before the operation succeeds.
  - Any exception returned by the final attempt of `operation` that is not considered transient (see `IsTransientError`).  
  - `ArgumentNullException` if `operation` is `null`.

### `public async Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)`

Executes the supplied asynchronous `operation` with retry logic, discarding any result.

- **Parameters**
  - `operation`: A delegate that returns a `Task` representing the work to be retried.
  - `cancellationToken`: Optional token to cancel the overall retry process.

- **Return value**: A `Task` that completes when `operation` succeeds, or after the configured number of retry attempts.

- **Exceptions**
  - Same as the generic overload: `OperationCanceledException` on cancellation, or the final non‑transient exception from `operation`.  
  - `ArgumentNullException` if `operation` is `null`.

### `public static bool IsTransientError(Exception exception)`

Determines whether the supplied exception should be treated as transient and therefore eligible for a retry attempt.

- **Parameters**
  - `exception`: The exception to evaluate.

- **Return value**: `true` if the exception indicates a transient fault (e.g., temporary network glitch, service throttling); otherwise `false`.

- **Exceptions**
  - `ArgumentNullException` if `exception` is `null`.

### `public void Dispose()`

Releases any resources held by the `RetryPolicy` instance. After disposal, further calls to `ExecuteAsync*` or logging methods may throw `ObjectDisposedException`.

### `public IDisposable BeginScope<TState>(TState state)`

Begins a logical operation scope for logging purposes, returning an `IDisposable` that ends the scope when disposed.

- **Parameters**
  - `state`: The state object to associate with the scope.

- **Return value**: An `IDisposable` that, when disposed, ends the scope.

- **Exceptions**
  - `ObjectDisposedException` if the `RetryPolicy` has been disposed.

### `public bool IsEnabled(LogLevel logLevel)`

Gets a value indicating whether logging is enabled for the supplied `LogLevel`.

- **Parameters**
  - `logLevel`: The level to check.

- **Return value**: `true` if writes with the given level will be processed; otherwise `false`.

### `public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)`

Writes a log entry.

- **Parameters**
  - `logLevel`: The level of the entry.
  - `eventId`: An identifier for the event.
  - `state`: The state to be formatted.
  - `exception`: The exception related to the entry, or `null`.
  - `formatter`: A function that formats the `state` and optional `exception` into a string.

- **Exceptions**
  - `ObjectDisposedException` if the `RetryPolicy` has been disposed.
  - `ArgumentNullException` if `formatter` is `null`.

## Usage

### Example 1: Retrying a network request

```csharp
using var policy = new RetryPolicy();

try
{
    var result = await policy.ExecuteAsync<string>(async ct =>
    {
        // Simulate a call that may fail transiently
        return await httpClient.GetStringAsync("https://api.example.com/data", ct);
    }, CancellationToken.None);

    Console.WriteLine($"Success: {result}");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation was cancelled.");
}
catch (Exception ex)
{
    Console.WriteLine($"Operation failed after retries: {ex.Message}");
}
```

### Example 2: Using logging scopes with the policy

```csharp
using var policy = new RetryPolicy();

using (policy.BeginScope(new { Operation = "PriceFetch" }))
{
    policy.Log(LogLevel.Information, new EventId(100), "Starting price fetch", null,
               (state, ex) => $"Operation: {state.Operation}");

    try
    {
        await policy.ExecuteAsync(async ct =>
        {
            // Some work that logs internally via ILogger
            await Task.Delay(500, ct);
        }, ct: CancellationToken.None);

        policy.Log(LogLevel.Debug, new EventId(101), "Price fetch completed", null,
                   (state, ex) => $"Operation: {state.Operation}");
    }
    catch (Exception ex)
    {
        policy.Log(LogLevel.Error, new EventId(102), "Price fetch failed", ex,
                   (state, e) => $"Operation: {state.Operation}, Error: {e?.Message}");
    }
}
```

## Notes

- The `RetryPolicy` instance is thread‑safe for concurrent calls to `ExecuteAsync*` and logging methods; however, the delegate supplied to `ExecuteAsync*` must itself be thread‑safe if it accesses shared mutable state.
- Disposing the policy while an execution is in progress will cause the ongoing operation to be cancelled; callers should await any pending tasks before disposing.
- `BeginScope` returns a disposable that must be disposed to correctly close the logging scope; failure to dispose may result in leaked scope context in log output.
- `IsTransientError` should be a pure method; throwing from it will propagate to the caller of `ExecuteAsync*` and will be treated as a non‑transient failure.
- The `IsEnabled` method reflects the current logging configuration; changes to the underlying logger configuration after construction are not observed unless the policy wraps a mutable logger instance.
