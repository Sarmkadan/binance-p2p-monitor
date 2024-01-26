#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BinanceP2pMonitor.Caching;

/// <summary>
/// Extension methods for <see cref="MemoryCache"/> providing additional cache operations
/// </summary>
public static class MemoryCacheExtensions
{
    /// <summary>
    /// Gets a value from cache or returns the default value if not found
    /// </summary>
    /// <typeparam name="T">Type of cached value</typeparam>
    /// <param name="cache">Cache instance</param>
    /// <param name="key">Cache key</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Cached value or default(T)</returns>
    public static async Task<T?> GetValueAsync<T>(this MemoryCache cache, string key, CancellationToken ct = default)
    {
        if (cache is null)
            throw new ArgumentNullException(nameof(cache));

        return await cache.GetAsync<T>(key, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets a value in cache with absolute expiration
    /// </summary>
    /// <typeparam name="T">Type of value</typeparam>
    /// <param name="cache">Cache instance</param>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="seconds">Expiration time in seconds</param>
    /// <param name="ct">Cancellation token</param>
    public static async Task SetAsync<T>(this MemoryCache cache, string key, T value, int seconds, CancellationToken ct = default)
    {
        if (cache is null)
            throw new ArgumentNullException(nameof(cache));

        if (key is null)
            throw new ArgumentNullException(nameof(key));

        await cache.SetAsync(key, value, TimeSpan.FromSeconds(seconds), ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets a value in cache with absolute expiration
    /// </summary>
    /// <typeparam name="T">Type of value</typeparam>
    /// <param name="cache">Cache instance</param>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="minutes">Expiration time in minutes</param>
    /// <param name="ct">Cancellation token</param>
    public static async Task SetAsync<T>(this MemoryCache cache, string key, T value, double minutes, CancellationToken ct = default)
    {
        if (cache is null)
            throw new ArgumentNullException(nameof(cache));

        if (key is null)
            throw new ArgumentNullException(nameof(key));

        await cache.SetAsync(key, value, TimeSpan.FromMinutes(minutes), ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Attempts to get a value from cache, returning a boolean indicating success
    /// </summary>
    /// <typeparam name="T">Type of cached value</typeparam>
    /// <param name="cache">Cache instance</param>
    /// <param name="key">Cache key</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Tuple containing success flag and value if found</returns>
    public static async Task<(bool Success, T? Value)> TryGetValueAsync<T>(this MemoryCache cache, string key, CancellationToken ct = default)
    {
        if (cache is null)
            throw new ArgumentNullException(nameof(cache));

        if (key is null)
            throw new ArgumentNullException(nameof(key));

        var value = await cache.GetAsync<T>(key, ct).ConfigureAwait(false);
        return (value is not null, value);
    }

    /// <summary>
    /// Gets a value from cache or creates it using the provided factory if not found
    /// </summary>
    /// <typeparam name="T">Type of cached value</typeparam>
    /// <param name="cache">Cache instance</param>
    /// <param name="key">Cache key</param>
    /// <param name="factory">Factory function to create value if not cached</param>
    /// <param name="seconds">Expiration time in seconds</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Cached or newly created value</returns>
    public static async Task<T> GetOrCreateAsync<T>(this MemoryCache cache, string key, Func<CancellationToken, Task<T>> factory, int seconds, CancellationToken ct = default)
    {
        if (cache is null)
            throw new ArgumentNullException(nameof(cache));

        if (key is null)
            throw new ArgumentNullException(nameof(key));

        if (factory is null)
            throw new ArgumentNullException(nameof(factory));

        return await cache.GetOrCreateAsync(key, factory, TimeSpan.FromSeconds(seconds), ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a value from cache or creates it using the provided factory if not found
    /// </summary>
    /// <typeparam name="T">Type of cached value</typeparam>
    /// <param name="cache">Cache instance</param>
    /// <param name="key">Cache key</param>
    /// <param name="factory">Factory function to create value if not cached</param>
    /// <param name="minutes">Expiration time in minutes</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Cached or newly created value</returns>
    public static async Task<T> GetOrCreateAsync<T>(this MemoryCache cache, string key, Func<CancellationToken, Task<T>> factory, double minutes, CancellationToken ct = default)
    {
        if (cache is null)
            throw new ArgumentNullException(nameof(cache));

        if (key is null)
            throw new ArgumentNullException(nameof(key));

        if (factory is null)
            throw new ArgumentNullException(nameof(factory));

        return await cache.GetOrCreateAsync(key, factory, TimeSpan.FromMinutes(minutes), ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the expiration time for a cached value
    /// </summary>
    /// <param name="cache">Cache instance</param>
    /// <param name="key">Cache key</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Expiration time if found, null otherwise</returns>
    public static async Task<DateTime?> GetExpirationAsync(this MemoryCache cache, string key, CancellationToken ct = default)
    {
        if (cache is null)
            throw new ArgumentNullException(nameof(cache));

        if (key is null)
            throw new ArgumentNullException(nameof(key));

        var entry = await cache.GetAsync<object?>(key, ct).ConfigureAwait(false);
        if (entry is null)
            return null;

        // Access the internal cache entry through reflection to get expiration time
        var cacheEntries = cache.GetType().GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (cacheEntries?.GetValue(cache) is not System.Collections.IEnumerable entriesEnumerable)
            return null;

        foreach (var entryObj in entriesEnumerable)
        {
            var keyProperty = entryObj?.GetType().GetProperty("Key");
            var valueProperty = entryObj?.GetType().GetProperty("Value");

            if (keyProperty?.GetValue(entryObj)?.ToString() == key && valueProperty?.GetValue(entryObj) is { } valueEntry)
            {
                var expiresAtProperty = valueEntry.GetType().GetProperty("ExpiresAt");
                return (DateTime?)expiresAtProperty?.GetValue(valueEntry);
            }
        }

        return null;
    }

    /// <summary>
    /// Removes multiple keys from cache in a single operation
    /// </summary>
    /// <param name="cache">Cache instance</param>
    /// <param name="keys">Keys to remove</param>
    /// <param name="ct">Cancellation token</param>
    public static async Task RemoveRangeAsync(this MemoryCache cache, IEnumerable<string> keys, CancellationToken ct = default)
    {
        if (cache is null)
            throw new ArgumentNullException(nameof(cache));

        if (keys is null)
            throw new ArgumentNullException(nameof(keys));

        foreach (var key in keys)
        {
            await cache.RemoveAsync(key, ct).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Gets the current count of items in cache
    /// </summary>
    /// <param name="cache">Cache instance</param>
    /// <returns>Number of items in cache</returns>
    public static int GetCount(this MemoryCache cache)
    {
        if (cache is null)
            throw new ArgumentNullException(nameof(cache));

        // Access the internal cache dictionary through reflection
        var cacheDict = cache.GetType().GetField("_cache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (cacheDict?.GetValue(cache) is System.Collections.IDictionary dictionary)
        {
            return dictionary.Count;
        }

        return 0;
    }
}