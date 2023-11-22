// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Infrastructure;

/// <summary>
/// Implements retry logic with exponential backoff
/// </summary>
public class RetryPolicy
{
    private readonly int _maxRetries;
    private readonly TimeSpan _initialDelay;
    private readonly double _backoffMultiplier;
    private readonly ILogger _logger;

    public RetryPolicy(int maxRetries = 3, TimeSpan? initialDelay = null, double backoffMultiplier = 2.0, ILogger? logger = null)
    {
        _maxRetries = maxRetries;
        _initialDelay = initialDelay ?? TimeSpan.FromSeconds(1);
        _backoffMultiplier = backoffMultiplier;
        _logger = logger ?? new NullLogger();
    }

    /// <summary>
    /// Executes operation with retry logic
    /// </summary>
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        Func<Exception, bool>? shouldRetry = null,
        CancellationToken ct = default)
    {
        var attempt = 0;
        var delay = _initialDelay;

        while (true)
        {
            try
            {
                attempt++;
                return await operation(ct);
            }
            catch (Exception ex)
            {
                if (attempt >= _maxRetries || (shouldRetry != null && !shouldRetry(ex)))
                {
                    _logger.LogError(ex, "Operation failed after {Attempts} attempts", attempt);
                    throw;
                }

                _logger.LogWarning(ex, "Attempt {Attempt} failed, retrying in {DelayMs}ms", attempt, delay.TotalMilliseconds);
                await Task.Delay(delay, ct);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * _backoffMultiplier);
            }
        }
    }

    /// <summary>
    /// Executes operation without return value
    /// </summary>
    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        Func<Exception, bool>? shouldRetry = null,
        CancellationToken ct = default)
    {
        await ExecuteAsync(
            async token => { await operation(token); return true; },
            shouldRetry,
            ct);
    }

    /// <summary>
    /// Predicate for transient errors (network, timeouts)
    /// </summary>
    public static bool IsTransientError(Exception ex)
    {
        return ex switch
        {
            TimeoutException => true,
            HttpRequestException => true,
            IOException => true,
            OperationCanceledException => false,
            _ => false
        };
    }

    private class NullLogger : ILogger
    {
        private class NullScope : IDisposable
        {
            public void Dispose() { }
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => new NullScope();
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
}
