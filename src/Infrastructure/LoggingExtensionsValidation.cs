#nullable enable

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BinanceP2pMonitor.Infrastructure;

/// <summary>
/// Provides validation helpers for <see cref="LoggingExtensions"/> to ensure configuration and usage are correct
/// </summary>
/// <remarks>
/// This class contains extension methods for validating logging configuration and parameters before actual logging occurs.
/// All validation methods return a list of problems; if the list is empty, validation passed.
/// Convenience methods like <see cref="IsValid"/> and <see cref="EnsureValid"/> are provided for common validation patterns.
/// </remarks>
public static class LoggingExtensionsValidation
{
    private const decimal MaxReasonableCryptoPrice = 1_000_000m;
    private const int MaxMetadataLength = 1024;
    private const int MaxReasonLength = 512;
    private const int MaxMetadataKeyLength = 128;
    private const int MaxMetadataValueLength = 256;
    private const int MaxMetadataEntries = 20;
    private const int MaxAffectedRows = 1_000_000;
    private const int MaxDatabaseOperationMs = 300_000; // 5 minutes

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

        // LogLevel.None is a special value meaning "no logging", so we check if logger is enabled for any actual log level
        if (!value.IsEnabled(LogLevel.Trace)
            && !value.IsEnabled(LogLevel.Debug)
            && !value.IsEnabled(LogLevel.Information)
            && !value.IsEnabled(LogLevel.Warning)
            && !value.IsEnabled(LogLevel.Error)
            && !value.IsEnabled(LogLevel.Critical))
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
    /// <exception cref="ArgumentNullException">Thrown when logPath is null</exception>
    /// <exception cref="ArgumentException">Thrown when logPath is empty or whitespace</exception>
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
    /// <exception cref="ArgumentException">Thrown when operationName is empty or whitespace</exception>
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

        if (metadata is not null && metadata.Length > MaxMetadataLength)
        {
            problems.Add($"Metadata string exceeds maximum length of {MaxMetadataLength} characters");
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
    /// <exception cref="ArgumentException">Thrown when asset or fiat is empty or whitespace</exception>
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
        if (previousPrice > MaxReasonableCryptoPrice)
        {
            problems.Add($"Previous price exceeds reasonable maximum value of ${MaxReasonableCryptoPrice:N0}");
        }

        if (currentPrice > MaxReasonableCryptoPrice)
        {
            problems.Add($"Current price exceeds reasonable maximum value of ${MaxReasonableCryptoPrice:N0}");
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
    /// <exception cref="ArgumentException">Thrown when any required parameter is empty or whitespace</exception>
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
        else if (reason.Length > MaxReasonLength)
        {
            problems.Add($"Alert reason exceeds maximum length of {MaxReasonLength} characters");
        }

        if (metadata is not null)
        {
            if (metadata.Count > MaxMetadataEntries)
            {
                problems.Add($"Metadata dictionary exceeds maximum of {MaxMetadataEntries} entries");
            }

            foreach (var kvp in metadata)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key) || kvp.Key.Length > MaxMetadataKeyLength)
                {
                    problems.Add($"Metadata key cannot be null, empty, or exceed {MaxMetadataKeyLength} characters");
                    break;
                }

                if (kvp.Value is not null && kvp.Value.Length > MaxMetadataValueLength)
                {
                    problems.Add($"Metadata value exceeds maximum length of {MaxMetadataValueLength} characters");
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
    /// <exception cref="ArgumentException">Thrown when operation or table is empty or whitespace</exception>
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
        if (affectedRows > MaxAffectedRows)
        {
            problems.Add($"Affected rows count exceeds reasonable maximum of {MaxAffectedRows:N0}");
        }

        if (elapsed.TotalMilliseconds > MaxDatabaseOperationMs)
        {
            problems.Add($"Database operation elapsed time exceeds reasonable maximum of {MaxDatabaseOperationMs / 1000 / 60} minutes");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ILoggingBuilder"/> instance is valid
    /// </summary>
    /// <param name="value">The logging builder to check</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/></returns>
    public static bool IsValid(this ILoggingBuilder value) => value.Validate() is [];

    /// <summary>
    /// Determines whether the specified <see cref="ILogger"/> instance is valid
    /// </summary>
    /// <param name="value">The logger to check</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/></returns>
    public static bool IsValid(this ILogger value) => value.Validate() is [];

    /// <summary>
    /// Determines whether the specified log path is valid
    /// </summary>
    /// <param name="logPath">The log file path to check</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/></returns>
    public static bool IsValid(string logPath) => Validate(logPath) is [];

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
        string? metadata = null) => Validate(operationName, elapsed, isSuccess, metadata) is [];

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
        decimal currentPrice) => Validate(asset, fiat, previousPrice, currentPrice) is [];

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
        Dictionary<string, string>? metadata = null) => Validate(alertType, asset, fiat, reason, metadata) is [];

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
        TimeSpan elapsed) => Validate(operation, table, affectedRows, elapsed) is [];

    /// <summary>
    /// Ensures that the specified <see cref="ILoggingBuilder"/> instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The logging builder to validate</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException">Validation failed with one or more problems</exception>
    /// <remarks>
    /// This method calls <see cref="Validate(ILoggingBuilder)"/> and throws an <see cref="ArgumentException"/> if any validation problems are found.
    /// The exception message will contain all validation errors joined by " | ".
    /// </remarks>
    public static void EnsureValid(this ILoggingBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);

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
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException">Validation failed with one or more problems</exception>
    /// <remarks>
    /// This method calls <see cref="Validate(ILogger)"/> and throws an <see cref="ArgumentException"/> if any validation problems are found.
    /// The exception message will contain all validation errors joined by " | ".
    /// </remarks>
    public static void EnsureValid(this ILogger value)
    {
        ArgumentNullException.ThrowIfNull(value);

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
    /// <exception cref="ArgumentNullException"><paramref name="logPath"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValid(string logPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(logPath);

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
    /// <exception cref="ArgumentNullException"><paramref name="operationName"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException">Thrown when operationName is empty or whitespace</exception>
    public static void EnsureValid(
        string operationName,
        TimeSpan elapsed,
        bool isSuccess = true,
        string? metadata = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(operationName);

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
    /// <exception cref="ArgumentNullException"><paramref name="asset"/> or <paramref name="fiat"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException">Thrown when asset or fiat is empty or whitespace</exception>
    public static void EnsureValid(
        string asset,
        string fiat,
        decimal previousPrice,
        decimal currentPrice)
    {
        ArgumentException.ThrowIfNullOrEmpty(asset);
        ArgumentException.ThrowIfNullOrEmpty(fiat);

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
    /// <exception cref="ArgumentNullException"><paramref name="alertType"/>, <paramref name="asset"/>, <paramref name="fiat"/>, or <paramref name="reason"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException">Thrown when any required parameter is empty or whitespace</exception>
    public static void EnsureValid(
        string alertType,
        string asset,
        string fiat,
        string reason,
        Dictionary<string, string>? metadata = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(alertType);
        ArgumentException.ThrowIfNullOrEmpty(asset);
        ArgumentException.ThrowIfNullOrEmpty(fiat);
        ArgumentException.ThrowIfNullOrEmpty(reason);

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
    /// <exception cref="ArgumentNullException"><paramref name="operation"/> or <paramref name="table"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException">Thrown when operation or table is empty or whitespace</exception>
    public static void EnsureValid(
        string operation,
        string table,
        int affectedRows,
        TimeSpan elapsed)
    {
        ArgumentException.ThrowIfNullOrEmpty(operation);
        ArgumentException.ThrowIfNullOrEmpty(table);

        var problems = Validate(operation, table, affectedRows, elapsed);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" | ", problems));
        }
    }
}