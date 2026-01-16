#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Caching;

/// <summary>
/// In-memory cache implementation with expiration support
/// </summary>
public class MemoryCache : ICache, IDisposable
{
    private readonly Dictionary<string, CacheEntry> _cache = new();
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly ILogger<MemoryCache> _logger;
    private Timer? _cleanupTimer;

    public MemoryCache(ILogger<MemoryCache> logger)
    {
        _logger = logger;
        // Start cleanup timer to remove expired entries every 5 minutes
        _cleanupTimer = new Timer(RemoveExpiredEntries, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        await Task.Yield();
        _lock.EnterReadLock();
        try
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (entry.IsExpired)
                {
                    _lock.ExitReadLock();
                    await RemoveAsync(key, ct).ConfigureAwait(false);
                    return default;
                }

                _logger.LogDebug("Cache hit for key: {Key}", key);
                return (T?)entry.Value;
            }

            return default;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        await Task.Yield();
        _lock.EnterWriteLock();
        try
        {
            _cache[key] = new CacheEntry { Value = value, ExpiresAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : null };
            _logger.LogDebug("Cache set for key: {Key}, expiration: {Expiration}", key, expiration);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await Task.Yield();
        _lock.EnterWriteLock();
        try
        {
            _cache.Remove(key);
            _logger.LogDebug("Cache removed for key: {Key}", key);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        return await GetAsync<object?>(key, ct) is not null;
    }

    public async Task ClearAsync(CancellationToken ct = default)
    {
        await Task.Yield();
        _lock.EnterWriteLock();
        try
        {
            var count = _cache.Count;
            _cache.Clear();
            _logger.LogInformation("Cache cleared, removed {Count} entries", count);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        var cached = await GetAsync<T>(key, ct).ConfigureAwait(false);
        if (cached is not null)
            return cached;

        var value = await factory(ct).ConfigureAwait(false);
        await SetAsync(key, value, expiration, ct).ConfigureAwait(false);
        return value;
    }

    private void RemoveExpiredEntries(object? state)
    {
        _lock.EnterWriteLock();
        try
        {
            var expiredKeys = _cache
                .Where(kvp => kvp.Value.IsExpired)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
                _cache.Remove(key);

            if (expiredKeys.Any())
                _logger.LogDebug("Removed {Count} expired cache entries", expiredKeys.Count);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
        _lock?.Dispose();
    }

    private class CacheEntry
    {
        public object? Value { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
    }
}
