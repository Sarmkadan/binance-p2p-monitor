#nullable enable
namespace BinanceP2pMonitor.Infrastructure;

/// <summary>
/// Implements token bucket rate limiting algorithm
/// </summary>
public class RateLimiter
{
    private readonly int _maxRequests;
    private readonly TimeSpan _timeWindow;
    private readonly Dictionary<string, TokenBucket> _buckets = new();
    private readonly ReaderWriterLockSlim _lock = new();

    public RateLimiter(int maxRequests, TimeSpan timeWindow)
    {
        _maxRequests = maxRequests;
        _timeWindow = timeWindow;
    }

    /// <summary>
    /// Checks if a request is allowed for the given key
    /// </summary>
    public bool IsAllowed(string key)
    {
        _lock.EnterWriteLock();
        try
        {
            if (!_buckets.TryGetValue(key, out var bucket))
            {
                bucket = new TokenBucket(_maxRequests, _timeWindow);
                _buckets[key] = bucket;
            }

            return bucket.TryConsumeToken();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Gets remaining tokens for a key
    /// </summary>
    public int GetRemainingTokens(string key)
    {
        _lock.EnterReadLock();
        try
        {
            return _buckets.TryGetValue(key, out var bucket)
                ? bucket.GetRemainingTokens()
                : _maxRequests;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Resets the bucket for a key
    /// </summary>
    public void Reset(string key)
    {
        _lock.EnterWriteLock();
        try
        {
            _buckets.Remove(key);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Clears all buckets
    /// </summary>
    public void ClearAll()
    {
        _lock.EnterWriteLock();
        try
        {
            _buckets.Clear();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Gets time until next token is available
    /// </summary>
    public TimeSpan? GetTimeUntilNextToken(string key)
    {
        _lock.EnterReadLock();
        try
        {
            return _buckets.TryGetValue(key, out var bucket)
                ? bucket.GetTimeUntilNextToken()
                : null;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    private class TokenBucket
    {
        private readonly int _capacity;
        private readonly TimeSpan _refillPeriod;
        private int _tokens;
        private DateTime _lastRefillTime;

        public TokenBucket(int capacity, TimeSpan refillPeriod)
        {
            _capacity = capacity;
            _refillPeriod = refillPeriod;
            _tokens = capacity;
            _lastRefillTime = DateTime.UtcNow;
        }

        public bool TryConsumeToken()
        {
            Refill();
            if (_tokens > 0)
            {
                _tokens--;
                return true;
            }
            return false;
        }

        public int GetRemainingTokens()
        {
            Refill();
            return _tokens;
        }

        public TimeSpan? GetTimeUntilNextToken()
        {
            Refill();
            if (_tokens > 0)
                return TimeSpan.Zero;

            return _lastRefillTime.Add(_refillPeriod) - DateTime.UtcNow;
        }

        private void Refill()
        {
            var now = DateTime.UtcNow;
            var timePassed = now - _lastRefillTime;

            if (timePassed >= _refillPeriod)
            {
                _tokens = _capacity;
                _lastRefillTime = now;
            }
        }
    }
}
