#nullable enable
namespace BinanceP2pMonitor.Infrastructure;

/// <summary>
/// Implements retry logic with exponential backoff
/// </summary>
public class RetryPolicy
{
    private const int DefaultMaxRetries = 3;
    private const double DefaultBackoffMultiplier = 2.0;
    private const string FailureLogMessage = "Operation failed after {Attempts} attempts";
    private const string RetryLogMessage = "Attempt {Attempt}/{MaxAttempts} failed, retrying in {DelayMs}ms";
    private static readonly TimeSpan DefaultInitialDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan DefaultMaxDelay = TimeSpan.FromSeconds(30);

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
    public RetryPolicy(int maxRetries = DefaultMaxRetries, TimeSpan? initialDelay = null, double backoffMultiplier = DefaultBackoffMultiplier, TimeSpan? maxDelay = null, ILogger? logger = null)
    {
        _maxRetries = maxRetries;
        _initialDelay = initialDelay ?? DefaultInitialDelay;
        _maxDelay = maxDelay ?? DefaultMaxDelay;
        _backoffMultiplier = backoffMultiplier;
        _logger = logger ?? new NullLogger();
    }

    /// <summary>
    /// Executes operation with retry logic
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="shouldRetry">An optional predicate that determines whether an exception should trigger a retry.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the operation or retry delay to complete.</param>
    /// <returns>The result of the operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="operation"/> is null.</exception>
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        Func<Exception, bool>? shouldRetry = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
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
                    _logger.LogError(ex, FailureLogMessage, attempt);
                    throw;
                }

                var actualDelay = delay > _maxDelay ? _maxDelay : delay;
                _logger.LogWarning(ex, RetryLogMessage, attempt, _maxRetries, actualDelay.TotalMilliseconds);
                await Task.Delay(actualDelay, ct).ConfigureAwait(false);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * _backoffMultiplier);
            }
        }
    }

    /// <summary>
    /// Executes operation without return value
    /// </summary>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="shouldRetry">An optional predicate that determines whether an exception should trigger a retry.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the operation or retry delay to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="operation"/> is null.</exception>
    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        Func<Exception, bool>? shouldRetry = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        await ExecuteAsync(
            async token => { await operation(token).ConfigureAwait(false); return true; },
            shouldRetry,
            ct);
    }

    /// <summary>
    /// Predicate for transient errors (network, timeouts)
    /// </summary>
    /// <param name="ex">The exception to evaluate.</param>
    /// <returns>True if the exception is transient and should be retried; otherwise, false.</returns>
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
