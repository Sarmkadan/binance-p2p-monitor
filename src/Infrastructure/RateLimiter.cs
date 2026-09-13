#nullable enable
namespace BinanceP2pMonitor.Infrastructure;

/// <summary>
/// Implements token bucket rate limiting algorithm
/// </summary>
public sealed class RateLimiter
{
	private readonly int _maxRequests;
	private readonly TimeSpan _timeWindow;
	private readonly Dictionary<string, TokenBucket> _buckets = new();
	private readonly ReaderWriterLockSlim _lock = new();
	private const int Zero = 0;
	private static readonly TimeSpan ZeroTimeSpan = TimeSpan.Zero;

	public RateLimiter(int maxRequests, TimeSpan timeWindow)
	{
		if (maxRequests <= Zero)
			throw new ArgumentOutOfRangeException(nameof(maxRequests));

		if (timeWindow <= ZeroTimeSpan)
			throw new ArgumentOutOfRangeException(nameof(timeWindow));

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
		// Write lock: reading token state triggers a refill, which mutates the bucket.
		_lock.EnterWriteLock();
		try
		{
			return _buckets.TryGetValue(key, out var bucket)
			? bucket.GetRemainingTokens()
			: _maxRequests;
		}
		finally
		{
			_lock.ExitWriteLock();
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
	/// Removes buckets that have not been accessed within the specified interval
	/// </summary>
	public int CleanupStaleBuckets(TimeSpan maxIdle)
	{
		_lock.EnterWriteLock();
		try
		{
			var cutoff = DateTime.UtcNow - maxIdle;
			var staleKeys = _buckets
				.Where(pair => pair.Value.LastAccessTime < cutoff)
				.Select(pair => pair.Key)
				.ToList();

			foreach (var key in staleKeys)
				_buckets.Remove(key);

			return staleKeys.Count;
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
		// Write lock: reading token state triggers a refill, which mutates the bucket.
		_lock.EnterWriteLock();
		try
		{
			return _buckets.TryGetValue(key, out var bucket)
			? bucket.GetTimeUntilNextToken()
			: null;
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	private class TokenBucket
	{
		private readonly int _capacity;
		private readonly TimeSpan _refillPeriod;
		private int _tokens;
		private DateTime _lastRefillTime;
		private DateTime _lastAccessTime;
		private readonly object _syncLock = new object();

		public DateTime LastAccessTime => _lastAccessTime;

		public TokenBucket(int capacity, TimeSpan refillPeriod)
		{
			_capacity = capacity;
			_refillPeriod = refillPeriod;
			_tokens = capacity;
			_lastRefillTime = DateTime.UtcNow;
			_lastAccessTime = _lastRefillTime;
		}

		public bool TryConsumeToken()
		{
			lock (_syncLock)
			{
				_lastAccessTime = DateTime.UtcNow;
				Refill();
				if (_tokens > Zero)
				{
					_tokens--;
					return true;
				}
				return false;
			}
		}

		public int GetRemainingTokens()
		{
			lock (_syncLock)
			{
				_lastAccessTime = DateTime.UtcNow;
				Refill();
				return _tokens;
			}
		}

		public TimeSpan? GetTimeUntilNextToken()
		{
			lock (_syncLock)
			{
				_lastAccessTime = DateTime.UtcNow;
				Refill();
				if (_tokens > Zero)
					return ZeroTimeSpan;

				return _lastRefillTime.Add(_refillPeriod) - DateTime.UtcNow;
			}
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
