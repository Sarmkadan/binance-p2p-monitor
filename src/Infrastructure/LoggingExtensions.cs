#nullable enable
namespace BinanceP2pMonitor.Infrastructure;

/// <summary>
/// Extension methods for logging configuration and structured logging
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Configures file logging with daily rotation
    /// </summary>
    public static ILoggingBuilder AddFileLogging(this ILoggingBuilder builder, string logPath)
    {
        if (!Directory.Exists(logPath))
            Directory.CreateDirectory(logPath);

        var timestamp = DateTime.Now.ToString("yyyy-MM-dd");
        var logFile = Path.Combine(logPath, $"app-{timestamp}.log");

        return builder.AddProvider(new FileLoggerProvider(logFile));
    }

    /// <summary>
    /// Logs performance metrics for a time-consuming operation
    /// </summary>
    public static void LogPerformance(
        this ILogger logger,
        string operationName,
        TimeSpan elapsed,
        bool isSuccess = true,
        string? metadata = null)
    {
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
    public static void LogPriceChange(
        this ILogger logger,
        string asset,
        string fiat,
        decimal previousPrice,
        decimal currentPrice)
    {
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
    public static void LogAlert(
        this ILogger logger,
        string alertType,
        string asset,
        string fiat,
        string reason,
        Dictionary<string, string>? metadata = null)
    {
        var metadataStr = metadata is not null
            ? " | " + string.Join(", ", metadata.Select(kvp => $"{kvp.Key}={kvp.Value}"))
            : string.Empty;

        logger.LogWarning("🔔 Alert [{AlertType}] {Asset}/{Fiat}: {Reason}{Metadata}",
            alertType, asset, fiat, reason, metadataStr);
    }

    /// <summary>
    /// Logs database operation metrics
    /// </summary>
    public static void LogDatabaseOperation(
        this ILogger logger,
        string operation,
        string table,
        int affectedRows,
        TimeSpan elapsed)
    {
        logger.LogDebug(
            "Database operation: {Operation} on table '{Table}' affected {Rows} rows in {ElapsedMs}ms",
            operation, table, affectedRows, elapsed.TotalMilliseconds);
    }
}

/// <summary>
/// Simple file logger provider for file-based logging
/// </summary>
internal class FileLoggerProvider : ILoggerProvider
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

internal class FileLogger : ILogger
{
    private readonly string _logPath;
    private readonly string _categoryName;

    public FileLogger(string logPath, string categoryName)
    {
        _logPath = logPath;
        _categoryName = categoryName;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
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
        var message = formatter(state, exception);
        var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{logLevel}] [{_categoryName}] {message}";

        if (exception is not null)
            logEntry += Environment.NewLine + exception;

        try
        {
            File.AppendAllText(_logPath, logEntry + Environment.NewLine);
        }
        catch { /* Ignore file write errors */ }
    }

    private class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}
