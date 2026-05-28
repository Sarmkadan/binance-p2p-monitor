#nullable enable
namespace BinanceP2pMonitor.Infrastructure;

/// <summary>
/// Implements retry logic with exponential backoff
/// </summary>
public class RetryPolicy
{
    private readonly int _maxRetries;
    private readonly TimeSpan _initialDelay;
    private readonly TimeSpan _maxDelay;
    private readonly double _backoffMultiplier;
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new retry policy with exponential backoff.
    /// </summary>
    /// <param name="maxRetries">Maximum number of retry attempts.</param>
    /// <param name="initialDelay">Delay before the first retry. Defaults to 1 second.</param>
    /// <param name="backoffMultiplier">Multiplier applied to the delay after each retry.</param>
    /// <param name="maxDelay">Maximum delay between retries to prevent unbounded waits. Defaults to 30 seconds.</param>
    /// <param name="logger">Optional logger instance.</param>
    public RetryPolicy(int maxRetries = 3, TimeSpan? initialDelay = null, double backoffMultiplier = 2.0, TimeSpan? maxDelay = null, ILogger? logger = null)
    {
        _maxRetries = maxRetries;
        _initialDelay = initialDelay ?? TimeSpan.FromSeconds(1);
        _maxDelay = maxDelay ?? TimeSpan.FromSeconds(30);
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
                return await operation(ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (attempt >= _maxRetries || (shouldRetry is not null && !shouldRetry(ex)))
                {
                    _logger.LogError(ex, "Operation failed after {Attempts} attempts", attempt);
                    throw;
                }

                var actualDelay = delay > _maxDelay ? _maxDelay : delay;
                _logger.LogWarning(ex, "Attempt {Attempt}/{MaxAttempts} failed, retrying in {DelayMs}ms", attempt, _maxRetries, actualDelay.TotalMilliseconds);
                await Task.Delay(actualDelay, ct).ConfigureAwait(false);
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
            async token => { await operation(token).ConfigureAwait(false); return true; },
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
