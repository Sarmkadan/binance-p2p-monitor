#nullable enable

using System;
using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Models;

/// <summary>
/// Extension methods for PriceAlert providing additional functionality
/// </summary>
public static class PriceAlertExtensions
{
    /// <summary>
    /// Creates a deep copy of the PriceAlert instance
    /// </summary>
    /// <param name="alert">The alert to copy</param>
    /// <returns>A new PriceAlert instance with the same values</returns>
    public static PriceAlert Clone(this PriceAlert alert)
    {
        if (alert == null)
            throw new ArgumentNullException(nameof(alert));

        return new PriceAlert
        {
            Id = alert.Id,
            Asset = alert.Asset,
            Fiat = alert.Fiat,
            AlertType = alert.AlertType,
            Threshold = alert.Threshold,
            Condition = alert.Condition,
            IsEnabled = alert.IsEnabled,
            UserId = alert.UserId,
            CreatedAt = alert.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
            LastTriggeredAt = alert.LastTriggeredAt,
            TriggerCount = alert.TriggerCount,
            Notes = alert.Notes
        };
    }

    /// <summary>
    /// Determines if the alert has been triggered at least the specified number of times
    /// </summary>
    /// <param name="alert">The alert to check</param>
    /// <param name="minTriggerCount">Minimum number of triggers required</param>
    /// <returns>True if triggered count meets or exceeds the minimum</returns>
    public static bool HasTriggeredAtLeast(this PriceAlert alert, int minTriggerCount)
    {
        if (alert == null)
            throw new ArgumentNullException(nameof(alert));

        return alert.TriggerCount >= minTriggerCount;
    }

    /// <summary>
    /// Updates the threshold value while maintaining the same alert condition
    /// </summary>
    /// <param name="alert">The alert to update</param>
    /// <param name="newThreshold">The new threshold value</param>
    /// <returns>True if the threshold was updated, false if the new value is invalid</returns>
    public static bool UpdateThreshold(this PriceAlert alert, decimal newThreshold)
    {
        if (alert == null)
            throw new ArgumentNullException(nameof(alert));

        if (newThreshold < 0 || newThreshold > 100)
            return false;

        alert.Threshold = newThreshold;
        alert.UpdatedAt = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Gets the age of the alert in days
    /// </summary>
    /// <param name="alert">The alert to check</param>
    /// <returns>Age in days, or 0 if CreatedAt is not set</returns>
    public static double GetAgeInDays(this PriceAlert alert)
    {
        if (alert == null)
            throw new ArgumentNullException(nameof(alert));

        if (alert.CreatedAt == default)
            return 0;

        var age = DateTime.UtcNow - alert.CreatedAt;
        return age.TotalDays;
    }

    /// <summary>
    /// Determines if the alert is currently active and ready to trigger
    /// </summary>
    /// <param name="alert">The alert to check</param>
    /// <param name="currentChange">Current price change percentage</param>
    /// <param name="cooldownMinutes">Cooldown period in minutes</param>
    /// <returns>True if the alert should fire based on conditions</returns>
    public static bool ShouldFire(this PriceAlert alert, decimal currentChange, int cooldownMinutes = 5)
    {
        if (alert == null)
            throw new ArgumentNullException(nameof(alert));

        return alert.IsEnabled
            && alert.ShouldTrigger(currentChange)
            && !alert.IsInCooldownPeriod(cooldownMinutes);
    }

    /// <summary>
    /// Updates the notes with additional information
    /// </summary>
    /// <param name="alert">The alert to update</param>
    /// <param name="additionalNotes">Text to append to existing notes</param>
    public static void AppendNotes(this PriceAlert alert, string additionalNotes)
    {
        if (alert == null)
            throw new ArgumentNullException(nameof(alert));

        if (string.IsNullOrWhiteSpace(additionalNotes))
            return;

        alert.Notes = string.IsNullOrWhiteSpace(alert.Notes)
            ? additionalNotes
            : $"{alert.Notes}\n{additionalNotes}";

        alert.UpdatedAt = DateTime.UtcNow;
    }
}