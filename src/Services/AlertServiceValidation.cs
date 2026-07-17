#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;

namespace BinanceP2pMonitor.Services;

/// <summary>
/// Provides validation helpers for <see cref="AlertService"/> instances
/// </summary>
public static class AlertServiceValidation
{
    private static readonly FieldInfo _alertRepositoryField = typeof(AlertService).GetField(
        "_alertRepository", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidOperationException("Cannot find _alertRepository field in AlertService");

    private static readonly FieldInfo _settingsField = typeof(AlertService).GetField(
        "_settings", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidOperationException("Cannot find _settings field in AlertService");

    private static readonly FieldInfo _loggerField = typeof(AlertService).GetField(
        "_logger", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidOperationException("Cannot find _logger field in AlertService");

    private static readonly FieldInfo _telegramNotificationClientField = typeof(AlertService).GetField(
        "_telegramNotificationClient", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidOperationException("Cannot find _telegramNotificationClient field in AlertService");

    private static readonly FieldInfo _webhookNotificationClientField = typeof(AlertService).GetField(
        "_webhookNotificationClient", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidOperationException("Cannot find _webhookNotificationClient field in AlertService");

    private static readonly FieldInfo _isDisposedField = typeof(AlertService).GetField(
        "_isDisposed", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidOperationException("Cannot find _isDisposed field in AlertService");

    /// <summary>
    /// Validates an AlertService instance for common issues including null dependencies and invalid configuration.
    /// </summary>
    /// <param name="value">The AlertService instance to validate.</param>
    /// <returns>A list of validation error messages; empty if the service is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this AlertService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate injected dependencies using reflection to access private fields
        if (_alertRepositoryField.GetValue(value) is null)
        {
            problems.Add("Alert repository (_alertRepository) cannot be null");
        }

        if (_settingsField.GetValue(value) is null)
        {
            problems.Add("Settings (_settings) cannot be null");
        }
        else
        {
            // Validate settings values with pattern matching for safety
            var settings = _settingsField.GetValue(value);
            if (settings is AppSettings appSettings)
            {
                if (appSettings.MaxAlertsPerUser <= 0)
                {
                    problems.Add($"Settings.MaxAlertsPerUser must be positive, got {appSettings.MaxAlertsPerUser}");
                }

                if (appSettings.AlertCooldownMinutes < 0)
                {
                    problems.Add($"Settings.AlertCooldownMinutes cannot be negative, got {appSettings.AlertCooldownMinutes}");
                }
            }
            else
            {
                problems.Add("Settings field does not contain a valid AppSettings instance");
            }
        }

        if (_loggerField.GetValue(value) is null)
        {
            problems.Add("Logger (_logger) cannot be null");
        }

        if (_telegramNotificationClientField.GetValue(value) is null)
        {
            problems.Add("Telegram notification client (_telegramNotificationClient) cannot be null");
        }

        if (_webhookNotificationClientField.GetValue(value) is null)
        {
            problems.Add("Webhook notification client (_webhookNotificationClient) cannot be null");
        }

        // Validate disposal state with explicit cast for clarity
        var isDisposed = (bool)_isDisposedField.GetValue(value)!;
        if (isDisposed)
        {
            problems.Add("AlertService has been disposed");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an AlertService instance is valid.
    /// </summary>
    /// <param name="value">The AlertService instance to check.</param>
    /// <returns>True if the service is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this AlertService value)
        => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that an AlertService instance is valid, throwing an <see cref="ArgumentException"/> if validation fails.
    /// </summary>
    /// <param name="value">The AlertService instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the service instance has validation problems.</exception>
    public static void EnsureValid(this AlertService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"AlertService validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}",
                nameof(value));
        }
    }
}