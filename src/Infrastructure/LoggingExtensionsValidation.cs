#nullable enable

using Microsoft.Extensions.Logging;

namespace BinanceP2pMonitor.Infrastructure;

/// <summary>
/// Provides validation helpers for <see cref="LoggingExtensions"/> to ensure configuration and usage are correct
/// </summary>
public static class LoggingExtensionsValidation
{
    /// <summary>
    /// Validates that a <see cref="ILoggingBuilder"/> instance is properly configured for file logging
    /// </summary>
    /// <param name="value">The logging builder to validate</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this ILoggingBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate that there's at least one provider
        if (!value.Services.Any(s => s.ServiceType == typeof(ILoggerProvider)))
        {
            problems.Add("ILoggingBuilder has no logger providers configured");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that a <see cref="ILogger"/> instance is ready for logging operations
    /// </summary>
    /// <param name="value">The logger to validate</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this ILogger value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Check if logger is enabled for any level
        if (!value.IsEnabled(LogLevel.None))
        {
            problems.Add("Logger is disabled for all log levels");
        }

        return problems.AsReadOnly();
    }


    /// <summary>
    /// Validates that logging configuration for AddFileLogging is correct
    /// </summary>
    /// <param name="logPath">The log file path to validate</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    public static IReadOnlyList<string> Validate(string logPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(logPath);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(logPath))
        {
            problems.Add("Log path cannot be null, empty, or whitespace");
        }
        else if (logPath.Any(c => Path.GetInvalidPathChars().Contains(c)))
        {
            problems.Add("Log path contains invalid characters");
        }
        else if (Path.IsPathRooted(logPath) && !Path.GetPathRoot(logPath)!.EndsWith(Path.DirectorySeparatorChar))
        {
            problems.Add("Log path must be a valid directory path");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that parameters for LogPerformance are within acceptable ranges
    /// </summary>
    /// <param name="operationName">Name of the operation being logged</param>
    /// <param name="elapsed">Time elapsed for the operation</param>
    /// <param name="isSuccess">Whether the operation succeeded</param>
    /// <param name="metadata">Optional metadata dictionary</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    public static IReadOnlyList<string> Validate(
        string operationName,
        TimeSpan elapsed,
        bool isSuccess = true,
        string? metadata = null)
    {
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(operationName))
        {
            problems.Add("Operation name cannot be null, empty, or whitespace");
        }

        if (elapsed < TimeSpan.Zero)
        {
            problems.Add("Elapsed time cannot be negative");
        }

        if (elapsed.TotalMilliseconds > int.MaxValue)
        {
            problems.Add("Elapsed time exceeds maximum representable milliseconds");
        }

        if (metadata is not null && metadata.Length > 1024)
        {
            problems.Add("Metadata string exceeds maximum length of 1024 characters");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that parameters for LogPriceChange are within acceptable ranges
    /// </summary>
    /// <param name="asset">The cryptocurrency asset symbol</param>
    /// <param name="fiat">The fiat currency symbol</param>
    /// <param name="previousPrice">The previous price value</param>
    /// <param name="currentPrice">The current price value</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    public static IReadOnlyList<string> Validate(
        string asset,
        string fiat,
        decimal previousPrice,
        decimal currentPrice)
    {
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(asset))
        {
            problems.Add("Asset symbol cannot be null, empty, or whitespace");
        }

        if (string.IsNullOrWhiteSpace(fiat))
        {
            problems.Add("Fiat currency cannot be null, empty, or whitespace");
        }

        if (previousPrice < 0)
        {
            problems.Add("Previous price cannot be negative");
        }

        if (currentPrice < 0)
        {
            problems.Add("Current price cannot be negative");
        }

        // Check for reasonable price values (cryptocurrency prices typically don't exceed $1M)
        if (previousPrice > 1_000_000m)
        {
            problems.Add("Previous price exceeds reasonable maximum value of $1,000,000");
        }

        if (currentPrice > 1_000_000m)
        {
            problems.Add("Current price exceeds reasonable maximum value of $1,000,000");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that parameters for LogAlert are within acceptable ranges
    /// </summary>
    /// <param name="alertType">Type/category of the alert</param>
    /// <param name="asset">The cryptocurrency asset symbol</param>
    /// <param name="fiat">The fiat currency symbol</param>
    /// <param name="reason">Reason for the alert</param>
    /// <param name="metadata">Optional metadata dictionary</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    public static IReadOnlyList<string> Validate(
        string alertType,
        string asset,
        string fiat,
        string reason,
        Dictionary<string, string>? metadata = null)
    {
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(alertType))
        {
            problems.Add("Alert type cannot be null, empty, or whitespace");
        }

        if (string.IsNullOrWhiteSpace(asset))
        {
            problems.Add("Asset symbol cannot be null, empty, or whitespace");
        }

        if (string.IsNullOrWhiteSpace(fiat))
        {
            problems.Add("Fiat currency cannot be null, empty, or whitespace");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            problems.Add("Alert reason cannot be null, empty, or whitespace");
        }

        if (reason.Length > 512)
        {
            problems.Add("Alert reason exceeds maximum length of 512 characters");
        }

        if (metadata is not null)
        {
            if (metadata.Count > 20)
            {
                problems.Add("Metadata dictionary exceeds maximum of 20 entries");
            }

            foreach (var kvp in metadata)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key) || kvp.Key.Length > 128)
                {
                    problems.Add("Metadata key cannot be null, empty, or exceed 128 characters");
                    break;
                }

                if (kvp.Value is not null && kvp.Value.Length > 256)
                {
                    problems.Add("Metadata value exceeds maximum length of 256 characters");
                    break;
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that parameters for LogDatabaseOperation are within acceptable ranges
    /// </summary>
    /// <param name="operation">Database operation type</param>
    /// <param name="table">Table name being operated on</param>
    /// <param name="affectedRows">Number of rows affected</param>
    /// <param name="elapsed">Time elapsed for the operation</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    public static IReadOnlyList<string> Validate(
        string operation,
        string table,
        int affectedRows,
        TimeSpan elapsed)
    {
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(operation))
        {
            problems.Add("Database operation type cannot be null, empty, or whitespace");
        }

        if (string.IsNullOrWhiteSpace(table))
        {
            problems.Add("Table name cannot be null, empty, or whitespace");
        }

        if (affectedRows < 0)
        {
            problems.Add("Affected rows count cannot be negative");
        }

        if (elapsed < TimeSpan.Zero)
        {
            problems.Add("Elapsed time cannot be negative");
        }

        // Reasonable upper bounds for database operations
        if (affectedRows > 1_000_000)
        {
            problems.Add("Affected rows count exceeds reasonable maximum of 1,000,000");
        }

        if (elapsed.TotalMilliseconds > 300_000) // 5 minutes
        {
            problems.Add("Database operation elapsed time exceeds reasonable maximum of 5 minutes");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ILoggingBuilder"/> instance is valid
    /// </summary>
    /// <param name="value">The logging builder to check</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/></returns>
    public static bool IsValid(this ILoggingBuilder value) => value.Validate().Count == 0;

    /// <summary>
    /// Determines whether the specified <see cref="ILogger"/> instance is valid
    /// </summary>
    /// <param name="value">The logger to check</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/></returns>
    public static bool IsValid(this ILogger value) => value.Validate().Count == 0;


    /// <summary>
    /// Determines whether the specified log path is valid
    /// </summary>
    /// <param name="logPath">The log file path to check</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/></returns>
    public static bool IsValid(string logPath) => Validate(logPath).Count == 0;

    /// <summary>
    /// Determines whether the specified parameters for LogPerformance are valid
    /// </summary>
    /// <param name="operationName">Name of the operation being logged</param>
    /// <param name="elapsed">Time elapsed for the operation</param>
    /// <param name="isSuccess">Whether the operation succeeded</param>
    /// <param name="metadata">Optional metadata dictionary</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/></returns>
    public static bool IsValid(
        string operationName,
        TimeSpan elapsed,
        bool isSuccess = true,
        string? metadata = null) => Validate(operationName, elapsed, isSuccess, metadata).Count == 0;

    /// <summary>
    /// Determines whether the specified parameters for LogPriceChange are valid
    /// </summary>
    /// <param name="asset">The cryptocurrency asset symbol</param>
    /// <param name="fiat">The fiat currency symbol</param>
    /// <param name="previousPrice">The previous price value</param>
    /// <param name="currentPrice">The current price value</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/></returns>
    public static bool IsValid(
        string asset,
        string fiat,
        decimal previousPrice,
        decimal currentPrice) => Validate(asset, fiat, previousPrice, currentPrice).Count == 0;

    /// <summary>
    /// Determines whether the specified parameters for LogAlert are valid
    /// </summary>
    /// <param name="alertType">Type/category of the alert</param>
    /// <param name="asset">The cryptocurrency asset symbol</param>
    /// <param name="fiat">The fiat currency symbol</param>
    /// <param name="reason">Reason for the alert</param>
    /// <param name="metadata">Optional metadata dictionary</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/></returns>
    public static bool IsValid(
        string alertType,
        string asset,
        string fiat,
        string reason,
        Dictionary<string, string>? metadata = null) => Validate(alertType, asset, fiat, reason, metadata).Count == 0;

    /// <summary>
    /// Determines whether the specified parameters for LogDatabaseOperation are valid
    /// </summary>
    /// <param name="operation">Database operation type</param>
    /// <param name="table">Table name being operated on</param>
    /// <param name="affectedRows">Number of rows affected</param>
    /// <param name="elapsed">Time elapsed for the operation</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/></returns>
    public static bool IsValid(
        string operation,
        string table,
        int affectedRows,
        TimeSpan elapsed) => Validate(operation, table, affectedRows, elapsed).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="ILoggingBuilder"/> instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The logging builder to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValid(this ILoggingBuilder value)
    {
        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" | ", problems));
        }
    }

    /// <summary>
    /// Ensures that the specified <see cref="ILogger"/> instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The logger to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValid(this ILogger value)
    {
        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" | ", problems));
        }
    }


    /// <summary>
    /// Ensures that the specified log path is valid, throwing an exception if not
    /// </summary>
    /// <param name="logPath">The log file path to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValid(string logPath)
    {
        var problems = Validate(logPath);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" | ", problems));
        }
    }

    /// <summary>
    /// Ensures that the specified parameters for LogPerformance are valid, throwing an exception if not
    /// </summary>
    /// <param name="operationName">Name of the operation being logged</param>
    /// <param name="elapsed">Time elapsed for the operation</param>
    /// <param name="isSuccess">Whether the operation succeeded</param>
    /// <param name="metadata">Optional metadata dictionary</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValid(
        string operationName,
        TimeSpan elapsed,
        bool isSuccess = true,
        string? metadata = null)
    {
        var problems = Validate(operationName, elapsed, isSuccess, metadata);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" | ", problems));
        }
    }

    /// <summary>
    /// Ensures that the specified parameters for LogPriceChange are valid, throwing an exception if not
    /// </summary>
    /// <param name="asset">The cryptocurrency asset symbol</param>
    /// <param name="fiat">The fiat currency symbol</param>
    /// <param name="previousPrice">The previous price value</param>
    /// <param name="currentPrice">The current price value</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValid(
        string asset,
        string fiat,
        decimal previousPrice,
        decimal currentPrice)
    {
        var problems = Validate(asset, fiat, previousPrice, currentPrice);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" | ", problems));
        }
    }

    /// <summary>
    /// Ensures that the specified parameters for LogAlert are valid, throwing an exception if not
    /// </summary>
    /// <param name="alertType">Type/category of the alert</param>
    /// <param name="asset">The cryptocurrency asset symbol</param>
    /// <param name="fiat">The fiat currency symbol</param>
    /// <param name="reason">Reason for the alert</param>
    /// <param name="metadata">Optional metadata dictionary</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValid(
        string alertType,
        string asset,
        string fiat,
        string reason,
        Dictionary<string, string>? metadata = null)
    {
        var problems = Validate(alertType, asset, fiat, reason, metadata);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" | ", problems));
        }
    }

    /// <summary>
    /// Ensures that the specified parameters for LogDatabaseOperation are valid, throwing an exception if not
    /// </summary>
    /// <param name="operation">Database operation type</param>
    /// <param name="table">Table name being operated on</param>
    /// <param name="affectedRows">Number of rows affected</param>
    /// <param name="elapsed">Time elapsed for the operation</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValid(
        string operation,
        string table,
        int affectedRows,
        TimeSpan elapsed)
    {
        var problems = Validate(operation, table, affectedRows, elapsed);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" | ", problems));
        }
    }
}