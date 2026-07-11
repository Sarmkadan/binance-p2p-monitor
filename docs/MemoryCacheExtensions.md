# MemoryCacheExtensions

Provides a set of asynchronous extension methods for `IMemoryCache` that simplify common caching patterns such as get-or-create, set with expiration, and batch removal. All methods are thread-safe and designed for use in high-concurrency scenarios.

## API

### `GetValueAsync<T>`
```csharp
public static async Task<T?> GetValueAsync<T>(this IMemoryCache cache, string key)
```
Retrieves the cached value associated with the specified key.  
- **Parameters**:  
  - `cache` – The `IMemoryCache` instance.  
  - `key` – A string key identifying the cached entry.  
- **Returns**: A `Task<T?>` that resolves to the cached value, or `default(T)` if the key does not exist or the entry has expired.  
- **Throws**: `ArgumentNullException` if `cache` or `key` is `null`.

### `SetAsync<T>` (overload 1)
```csharp
public static async Task SetAsync<T>(this IMemoryCache cache, string key, T value)
```
Stores a value in the cache with no explicit expiration (uses the default cache entry options).  
- **Parameters**:  
  - `cache` – The `IMemoryCache` instance.  
  - `key` – A string key.  
  - `value` – The value to cache.  
- **Returns**: A `Task` representing the asynchronous operation.  
- **Throws**: `ArgumentNullException` if `cache` or `key` is `null`.

### `SetAsync<T>` (overload 2)
```csharp
public static async Task SetAsync<T>(this IMemoryCache cache, string key, T value, TimeSpan expiration)
```
Stores a value in the cache with an absolute expiration time.  
- **Parameters**:  
  - `cache` – The `IMemoryCache` instance.  
  - `key` – A string key.  
  - `value` – The value to cache.  
  - `expiration` – The absolute expiration duration from now.  
- **Returns**: A `Task` representing the asynchronous operation.  
- **Throws**: `ArgumentNullException` if `cache` or `key` is `null`; `ArgumentOutOfRangeException` if `expiration` is less than or equal to `TimeSpan.Zero`.

### `TryGetValueAsync<T>`
```csharp
public static async Task<(bool Success, T? Value)> TryGetValueAsync<T>(this IMemoryCache cache, string key)
```
Attempts to retrieve the cached value and returns a tuple indicating success.  
- **Parameters**:  
  - `cache` – The `IMemoryCache` instance.  
  - `key` – A string key.  
- **Returns**: A `Task` resolving to a tuple `(bool Success, T? Value)`. `Success` is `true` if the key exists and the value is of type `T`; otherwise `false` and `Value` is `default`.  
- **Throws**: `ArgumentNullException` if `cache` or `key` is `null`.

### `GetOrCreateAsync<T>` (overload 1)
```csharp
public static async Task<T> GetOrCreateAsync<T>(this IMemoryCache cache, string key, Func<Task<T>> factory)
```
Gets the cached value for the key, or asynchronously creates and stores it using the provided factory if the key does not exist. No explicit expiration is set.  
- **Parameters**:  
  - `cache` – The `IMemoryCache` instance.  
  - `key` – A string key.  
  - `factory` – An asynchronous function that produces the value to cache.  
- **Returns**: A `Task<T>` that resolves to the cached or newly created value.  
- **Throws**: `ArgumentNullException` if `cache`, `key`, or `factory` is `null`.

### `GetOrCreateAsync<T>` (overload 2)
```csharp
public static async Task<T> GetOrCreateAsync<T>(this IMemoryCache cache, string key, Func<Task<T>> factory, TimeSpan expiration)
```
Gets the cached value for the key, or asynchronously creates and stores it with an absolute expiration.  
- **Parameters**:  
  - `cache` – The `IMemoryCache` instance.  
  - `key` – A string key.  
  - `factory` – An asynchronous function that produces the value to cache.  
  - `expiration` – The absolute expiration duration from now.  
- **Returns**: A `Task<T>` that resolves to the cached or newly created value.  
- **Throws**: `ArgumentNullException` if `cache`, `key`, or `factory` is `null`; `ArgumentOutOfRangeException` if `expiration` is less than or equal to `TimeSpan.Zero`.

### `GetExpirationAsync`
```csharp
public static async Task<DateTime?> GetExpirationAsync(this IMemoryCache cache, string key)
```
Retrieves the absolute expiration time of a cached entry, if set.  
- **Parameters**:  
  - `cache` – The `IMemoryCache` instance.  
  - `key` – A string key.  
- **Returns**: A `Task<DateTime?>` that resolves to the expiration time in UTC, or `null` if the key does not exist or has no absolute expiration.  
- **Throws**: `ArgumentNullException` if `cache` or `key` is `null`.

### `RemoveRangeAsync`
```csharp
public static async Task RemoveRangeAsync(this IMemoryCache cache, IEnumerable<string> keys)
```
Removes all cache entries whose keys are present in the provided collection.  
- **Parameters**:  
  - `cache` – The `IMemoryCache` instance.  
  - `keys` – A collection of string keys to remove.  
- **Returns**: A `Task` representing the asynchronous operation.  
- **Throws**: `ArgumentNullException` if `cache` or `keys` is `null`.

### `GetCount`
```csharp
public static int GetCount(this IMemoryCache cache)
```
Returns the approximate number of entries currently stored in the cache.  
- **Parameters**:  
  - `cache` – The `IMemoryCache` instance.  
- **Returns**: An `int` representing the count of cache entries.  
- **Throws**: `ArgumentNullException` if `cache` is `null`.

## Usage

### Example 1: Basic get-or-create with expiration
```csharp
using Microsoft.Extensions.Caching.Memory;
using BinanceP2PMonitor.Extensions;

IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());

// Retrieve or create a value that expires after 5 minutes
var data = await cache.GetOrCreateAsync("exchange_rates", async () =>
{
    // Simulate an expensive API call
    await Task.Delay(100);
    return new { Rate = 1.234m };
}, TimeSpan.FromMinutes(5));

Console.WriteLine($"Rate: {data.Rate}");
```

### Example 2: Batch removal and count monitoring
```csharp
using Microsoft.Extensions.Caching.Memory;
using BinanceP2PMonitor.Extensions;

IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());

// Populate cache
await cache.SetAsync("key1", "value1", TimeSpan.FromHours(1));
await cache.SetAsync("key2", "value2", TimeSpan.FromHours(1));
await cache.SetAsync("key3", "value3", TimeSpan.FromHours(1));

Console.WriteLine($"Count before removal: {cache.GetCount()}"); // 3

// Remove multiple keys
await cache.RemoveRangeAsync(new[] { "key1", "key3" });

Console.WriteLine($"Count after removal: {cache.GetCount()}"); // 1

// Check expiration of remaining key
DateTime? exp = await cache.GetExpirationAsync("key2");
Console.WriteLine($"Expiration UTC: {exp}");
```

## Notes

- **Thread safety**: All methods are safe for concurrent use. `IMemoryCache` itself is thread-safe, and the async wrappers do not introduce additional locking. However, `GetOrCreateAsync` may invoke the factory multiple times if concurrent requests arrive for the same missing key. To avoid duplicate work, consider using a separate synchronization mechanism (e.g., `SemaphoreSlim` per key) if the factory is expensive or non-idempotent.
- **Key validation**: `null` keys cause an `ArgumentNullException`. Empty strings are allowed but strongly discouraged for clarity.
- **Expiration behavior**: `SetAsync` without an expiration uses the cache’s default sliding expiration (if configured) or no expiration. `GetExpirationAsync` returns `null` for entries without an absolute expiration or for sliding-expiration entries.
- **Count accuracy**: `GetCount` returns an approximate count; it may not reflect entries that are in the process of being evicted or added.
- **Async disposal**: The methods do not dispose the `IMemoryCache` instance. Callers are responsible for managing the cache lifetime.
- **Type safety**: `GetValueAsync<T>` and `TryGetValueAsync<T>` cast the stored object to `T`. If the stored value is not of the expected type, `TryGetValueAsync` returns `Success = false` and `GetValueAsync` returns `default`. No exception is thrown for type mismatch.
