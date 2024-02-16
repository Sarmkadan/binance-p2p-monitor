#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;

namespace BinanceP2pMonitor.Services;

/// <summary>
/// Provides validation helpers for <see cref="PriceMonitoringService"/> instances
/// </summary>
public static class PriceMonitoringServiceValidation
{
    private static readonly FieldInfo _priceRepositoryField = typeof(PriceMonitoringService).GetField(
        "_priceRepository", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException("Cannot find _priceRepository field");

    private static readonly FieldInfo _historyServiceField = typeof(PriceMonitoringService).GetField(
        "_historyService", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException("Cannot find _historyService field");

    private static readonly FieldInfo _alertServiceField = typeof(PriceMonitoringService).GetField(
        "_alertService", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException("Cannot find _alertService field");

    private static readonly FieldInfo _spreadAnalysisServiceField = typeof(PriceMonitoringService).GetField(
        "_spreadAnalysisService", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException("Cannot find _spreadAnalysisService field");

    private static readonly FieldInfo _eventBusField = typeof(PriceMonitoringService).GetField(
        "_eventBus", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException("Cannot find _eventBus field");

    private static readonly FieldInfo _webSocketServiceField = typeof(PriceMonitoringService).GetField(
        "_webSocketService", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException("Cannot find _webSocketService field");

    private static readonly FieldInfo _settingsField = typeof(PriceMonitoringService).GetField(
        "_settings", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException("Cannot find _settings field");

    private static readonly FieldInfo _loggerField = typeof(PriceMonitoringService).GetField(
        "_logger", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException("Cannot find _logger field");

    private static readonly FieldInfo _isMonitoringField = typeof(PriceMonitoringService).GetField(
        "_isMonitoring", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException("Cannot find _isMonitoring field");

    /// <summary>
    /// Validates a PriceMonitoringService instance and returns a list of human-readable validation problems
    /// </summary>
    /// <param name="value">The PriceMonitoringService instance to validate</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this PriceMonitoringService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate injected dependencies using reflection to access private fields
        if (_priceRepositoryField.GetValue(value) is null)
        {
            problems.Add("Price repository (_priceRepository) cannot be null");
        }

        if (_historyServiceField.GetValue(value) is null)
        {
            problems.Add("History service (_historyService) cannot be null");
        }

        if (_alertServiceField.GetValue(value) is null)
        {
            problems.Add("Alert service (_alertService) cannot be null");
        }

        if (_spreadAnalysisServiceField.GetValue(value) is null)
        {
            problems.Add("Spread analysis service (_spreadAnalysisService) cannot be null");
        }

        if (_eventBusField.GetValue(value) is null)
        {
            problems.Add("Event bus (_eventBus) cannot be null");
        }

        if (_webSocketServiceField.GetValue(value) is null)
        {
            problems.Add("WebSocket service (_webSocketService) cannot be null");
        }

        if (_settingsField.GetValue(value) is null)
        {
            problems.Add("Settings (_settings) cannot be null");
        }

        if (_loggerField.GetValue(value) is null)
        {
            problems.Add("Logger (_logger) cannot be null");
        }

        // Validate _isMonitoring state
        var isMonitoringValue = (bool)_isMonitoringField.GetValue(value)!;
        // No specific validation needed for boolean state

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified PriceMonitoringService instance is valid
    /// </summary>
    /// <param name="value">The PriceMonitoringService instance to check</param>
    /// <returns>True if the PriceMonitoringService is valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static bool IsValid(this PriceMonitoringService value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified PriceMonitoringService instance is valid, throwing an exception if it is not
    /// </summary>
    /// <param name="value">The PriceMonitoringService instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown if the service instance has validation problems</exception>
    public static void EnsureValid(this PriceMonitoringService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"PriceMonitoringService validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }
}