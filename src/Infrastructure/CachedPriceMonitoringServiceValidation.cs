#nullable enable

using BinanceP2pMonitor.Models;
using System.Globalization;
using System.Reflection;

namespace BinanceP2pMonitor.Infrastructure;

/// <summary>
/// Provides validation helpers for <see cref="CachedPriceMonitoringService"/> instances
/// </summary>
public static class CachedPriceMonitoringServiceValidation
{
    private static readonly FieldInfo _innerServiceField = typeof(CachedPriceMonitoringService).GetField(
        "_innerService", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException("Cannot find _innerService field");

    private static readonly FieldInfo _cacheField = typeof(CachedPriceMonitoringService).GetField(
        "_cache", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException("Cannot find _cache field");

    private static readonly FieldInfo _loggerField = typeof(CachedPriceMonitoringService).GetField(
        "_logger", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException("Cannot find _logger field");

    private static readonly FieldInfo _cacheDurationField = typeof(CachedPriceMonitoringService).GetField(
        "_cacheDuration", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException("Cannot find _cacheDuration field");

    /// <summary>
    /// Validates the specified <see cref="CachedPriceMonitoringService"/> instance.
    /// </summary>
    /// <param name="value">The service instance to validate</param>
    /// <returns>A list of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this CachedPriceMonitoringService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate injected dependencies (these are constructor parameters)
        if (_innerServiceField.GetValue(value) is null)
        {
            problems.Add("Inner service (_innerService) cannot be null");
        }

        if (_cacheField.GetValue(value) is null)
        {
            problems.Add("Cache (_cache) cannot be null");
        }

        if (_loggerField.GetValue(value) is null)
        {
            problems.Add("Logger (_logger) cannot be null");
        }

        // Validate cache duration
        if (_cacheDurationField.GetValue(value) is not TimeSpan cacheDuration || cacheDuration <= TimeSpan.Zero)
        {
            problems.Add("Cache duration (_cacheDuration) must be positive");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="CachedPriceMonitoringService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to check</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/></returns>
    public static bool IsValid(this CachedPriceMonitoringService? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="CachedPriceMonitoringService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if the service instance has validation problems</exception>
    public static void EnsureValid(this CachedPriceMonitoringService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"CachedPriceMonitoringService is not valid. Problems: {string.Join(", ", problems)}");
    }
}