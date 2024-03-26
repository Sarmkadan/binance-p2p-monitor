#nullable enable

using Microsoft.Extensions.Logging;
using System.IO;

namespace BinanceP2pMonitor.Infrastructure;

/// <summary>
/// Extension methods for logging configuration and structured logging
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Configures file logging with daily rotation
    /// </summary>
    /// <param name="builder">The logging builder instance</param>
    /// <param name="logPath">The directory path where log files should be stored</param>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="logPath"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="logPath"/> is empty or whitespace</exception>
    public static ILoggingBuilder AddFileLogging(this ILoggingBuilder builder, string logPath)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(logPath);

        if (!Directory.Exists(logPath))
            Directory.CreateDirectory(logPath);

        var timestamp = DateTime.Now.ToString("yyyy-MM-dd");
        var logFile = Path.Combine(logPath, $"app-{timestamp}.log");

        return builder.AddProvider(new FileLoggerProvider(logFile));
    }

    /// <summary>
    /// Logs performance metrics for a time-consuming operation
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="operationName">Name of the operation being measured</param>
    /// <param name="elapsed">Time elapsed for the operation</param>
    /// <param name="isSuccess">Whether the operation succeeded</param>
    /// <param name="metadata">Additional metadata to include in the log</param>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="operationName"/> is <see langword="null"/></exception>
    public static void LogPerformance(
        this ILogger logger,
        string operationName,
        TimeSpan elapsed,
        bool isSuccess = true,
        string? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        var logLevel = elapsed > TimeSpan.FromSeconds(5)
            ? LogLevel.Warning
            : LogLevel.Information;

        logger.Log(logLevel,
            "Operation '{Operation}' completed in {ElapsedMs}ms | Success={IsSuccess} | {Metadata}",
            operationName, elapsed.TotalMilliseconds, isSuccess, metadata ?? "");
    }

    /// <summary>
    /// Logs price change information with percentages
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="asset">The cryptocurrency asset symbol</param>
    /// <param name="fiat">The fiat currency symbol</param>
    /// <param name="previousPrice">The previous price value</param>
    /// <param name="currentPrice">The current price value</param>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="asset"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="fiat"/> is <see langword="null"/></exception>
    public static void LogPriceChange(
        this ILogger logger,
        string asset,
        string fiat,
        decimal previousPrice,
        decimal currentPrice)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrWhiteSpace(asset);
        ArgumentException.ThrowIfNullOrWhiteSpace(fiat);

        var changePercentage = previousPrice == 0
            ? 0
            : ((currentPrice - previousPrice) / previousPrice * 100);

        var direction = currentPrice > previousPrice ? "📈" : "📉";
        logger.LogInformation(
            "Price changed for {Asset}/{Fiat}: {PreviousPrice:F8} → {CurrentPrice:F8} ({ChangePercentage:+0.00;-0.00;0}%) {Direction}",
            asset, fiat, previousPrice, currentPrice, changePercentage, direction);
    }

    /// <summary>
    /// Logs an alert with context
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="alertType">Type/category of alert</param>
    /// <param name="asset">The cryptocurrency asset symbol</param>
    /// <param name="fiat">The fiat currency symbol</param>
    /// <param name="reason">Reason for the alert</param>
    /// <param name="metadata">Additional metadata dictionary</param>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="alertType"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="asset"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="fiat"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="reason"/> is <see langword="null"/></exception>
    public static void LogAlert(
        this ILogger logger,
        string alertType,
        string asset,
        string fiat,
        string reason,
        Dictionary<string, string>? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrWhiteSpace(alertType);
        ArgumentException.ThrowIfNullOrWhiteSpace(asset);
        ArgumentException.ThrowIfNullOrWhiteSpace(fiat);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        var metadataStr = metadata is not null
            ? " | " + string.Join(", ", metadata.Select(kvp => $"{kvp.Key}={kvp.Value}"))
            : string.Empty;

        logger.LogWarning("🔔 Alert [{AlertType}] {Asset}/{Fiat}: {Reason}{Metadata}",
            alertType, asset, fiat, reason, metadataStr);
    }

    /// <summary>
    /// Logs database operation metrics
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="operation">Database operation type</param>
    /// <param name="table">Table name affected</param>
    /// <param name="affectedRows">Number of rows affected</param>
    /// <param name="elapsed">Time elapsed for the operation</param>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="operation"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <see langword="null"/></exception>
    public static void LogDatabaseOperation(
        this ILogger logger,
        string operation,
        string table,
        int affectedRows,
        TimeSpan elapsed)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        ArgumentException.ThrowIfNullOrWhiteSpace(table);

        logger.LogDebug(
            "Database operation: {Operation} on table '{Table}' affected {Rows} rows in {ElapsedMs}ms",
            operation, table, affectedRows, elapsed.TotalMilliseconds);
    }
}

/// <summary>
/// Simple file logger provider for file-based logging
/// </summary>
internal sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _logPath;

    public FileLoggerProvider(string logPath)
    {
        _logPath = logPath;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(_logPath, categoryName);
    }

    public void Dispose() { }
}

internal sealed class FileLogger : ILogger
{
    private readonly string _logPath;
    private readonly string _categoryName;

    public FileLogger(string logPath, string categoryName)
    {
        _logPath = logPath;
        _categoryName = categoryName;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return NullScope.Instance;
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);

        var message = formatter(state, exception);
        var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{logLevel}] [{_categoryName}] {message}";

        if (exception is not null)
            logEntry += Environment.NewLine + exception;

        try
        {
            File.AppendAllText(_logPath, logEntry + Environment.NewLine);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or PathTooLongException)
        {
            // Log to console as fallback, but don't throw - file logging is best-effort
            Console.Error.WriteLine($"Failed to write to log file {_logPath}: {ex.Message}");
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose() { }
    }
}