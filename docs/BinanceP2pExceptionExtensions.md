# BinanceP2pExceptionExtensions

Provides a set of static extension methods for inspecting and enriching exceptions that occur in the Binance P2P monitoring context. These methods help classify the severity of an exception (fatal vs. transient), retrieve a human-readable message, and attach additional contextual information before rethrowing or logging.

## API

### `IsFatal`

```csharp
public static bool IsFatal(this Exception exception)
```

**Purpose**  
Determines whether the specified exception represents a fatal, unrecoverable error that should halt the current operation or process.

**Parameters**  
- `exception` – The exception to evaluate. Must not be `null`.

**Returns**  
`true` if the exception is considered fatal; otherwise `false`.

**Throws**  
- `ArgumentNullException` if `exception` is `null`.

---

### `IsTransient`

```csharp
public static bool IsTransient(this Exception exception)
```

**Purpose**  
Determines whether the specified exception represents a transient, retryable error that may succeed on a subsequent attempt.

**Parameters**  
- `exception` – The exception to evaluate. Must not be `null`.

**Returns**  
`true` if the exception is considered transient; otherwise `false`.

**Throws**  
- `ArgumentNullException` if `exception` is `null`.

---

### `GetFriendlyMessage`

```csharp
public static string GetFriendlyMessage(this Exception exception)
```

**Purpose**  
Returns a user-friendly, non-technical message that describes the exception in a way suitable for display to end users or operators.

**Parameters**  
- `exception` – The exception from which to extract a friendly message. Must not be `null`.

**Returns**  
A string containing a human-readable explanation of the error.

**Throws**  
- `ArgumentNullException` if `exception` is `null`.

---

### `AddContext<T>`

```csharp
public static T AddContext<T>(this T exception, string context)
    where T : Exception
```

**Purpose**  
Attaches additional contextual information to the exception (e.g., the operation being performed, relevant identifiers) and returns the same exception instance. This is useful for preserving the original exception while enriching it with details before rethrowing or logging.

**Parameters**  
- `exception` – The exception to enrich. Must not be `null`.  
- `context` – A string describing the context in which the exception occurred.

**Returns**  
The same exception instance (`T`) with the context information stored internally (typically in the `Data` dictionary or a custom property).

**Throws**  
- `ArgumentNullException` if `exception` is `null`.  
- `ArgumentException` if `context` is `null` or empty.

## Usage

### Example 1: Classifying and logging an exception

```csharp
try
{
    await FetchOrderBookAsync("BTCUSDT");
}
catch (Exception ex)
{
    if (ex.IsFatal())
    {
        Logger.Fatal(ex.GetFriendlyMessage());
        Environment.FailFast("Fatal error encountered", ex);
    }
    else if (ex.IsTransient())
    {
        Logger.Warn(ex.GetFriendlyMessage());
        // Schedule retry logic
    }
    else
    {
        Logger.Error(ex.GetFriendlyMessage());
    }
}
```

### Example 2: Adding context before rethrowing

```csharp
public async Task<Order> PlaceOrderAsync(OrderRequest request)
{
    try
    {
        return await _client.PlaceOrderAsync(request);
    }
    catch (BinanceP2pException ex)
    {
        // Enrich with the order symbol and side before rethrowing
        throw ex.AddContext($"Symbol={request.Symbol}, Side={request.Side}");
    }
}
```

## Notes

- All methods are static and operate solely on the provided exception instance; they do not maintain any internal state. Consequently, they are thread-safe and can be called concurrently from multiple threads without synchronization.
- The classification logic in `IsFatal` and `IsTransient` is based on the exception type, its inner exceptions, and any custom data attached via `AddContext`. The exact heuristics are implementation-defined and may evolve.
- `GetFriendlyMessage` may fall back to the exception’s `Message` property if no specific friendly message mapping exists.
- `AddContext` modifies the exception’s `Data` dictionary. The same exception instance is returned, allowing fluent chaining. The context string is stored under a well-known key; subsequent calls to `AddContext` append to or overwrite the stored context depending on the implementation.
- Passing a `null` exception to any method will throw `ArgumentNullException`. Passing a `null` or empty `context` to `AddContext` will throw `ArgumentException`.
