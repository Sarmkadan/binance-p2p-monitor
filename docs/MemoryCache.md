# MemoryCache
The `MemoryCache` type is designed to provide a simple, in-memory caching mechanism for storing and retrieving data. It allows for asynchronous operations, making it suitable for use in concurrent environments. The cache stores values of various types, with optional expiration times, enabling efficient data retrieval and management.

## API
* `public MemoryCache`: The constructor for creating a new instance of the `MemoryCache`.
* `public async Task<T?> GetAsync<T>`: Retrieves a value of type `T` from the cache. Returns the cached value if it exists, or `null` if it does not. Throws if an error occurs during retrieval.
* `public async Task SetAsync<T>`: Sets a value of type `T` in the cache. Throws if an error occurs during storage.
* `public async Task RemoveAsync`: Removes a value from the cache. Throws if an error occurs during removal.
* `public async Task<bool> ExistsAsync`: Checks if a value exists in the cache. Returns `true` if the value exists, `false` otherwise. Throws if an error occurs during the check.
* `public async Task ClearAsync`: Clears all values from the cache. Throws if an error occurs during clearing.
* `public async Task<T> GetOrCreateAsync<T>`: Retrieves a value of type `T` from the cache, or creates and stores a new value if it does not exist. Returns the cached or newly created value. Throws if an error occurs during retrieval or creation.
* `public void Dispose`: Releases any unmanaged resources held by the cache.
* `public object? Value`: Gets the cached value.
* `public DateTime? ExpiresAt`: Gets the expiration time of the cached value.

## Usage
```csharp
// Example 1: Basic caching usage
var cache = new MemoryCache();
await cache.SetAsync("Hello, World!");
var cachedValue = await cache.GetAsync<string>();
Console.WriteLine(cachedValue);  // Outputs: Hello, World!

// Example 2: Using GetOrCreateAsync to cache computed values
var cache = new MemoryCache();
var computedValue = await cache.GetOrCreateAsync<int>(async () =>
{
    // Simulate an expensive computation
    await Task.Delay(1000);
    return 42;
});
Console.WriteLine(computedValue);  // Outputs: 42
```

## Notes
The `MemoryCache` is designed for in-memory storage, which means that cached values are lost when the application restarts. It is also important to note that the cache is not thread-safe by default, and concurrent access may lead to unexpected behavior. When using `GetOrCreateAsync`, the creation function is only executed if the value does not exist in the cache, which can help prevent redundant computations. Additionally, the `ExpiresAt` property can be used to implement time-based eviction policies, where values are automatically removed from the cache after a specified period.
