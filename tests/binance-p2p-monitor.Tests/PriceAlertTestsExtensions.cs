#nullable enable

using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Constants;

namespace BinanceP2pMonitor.Tests;

public static class PriceAlertTestsExtensions
{
    /// <summary>
    /// Creates a test PriceAlert with the specified parameters
    /// </summary>
    public static PriceAlert CreateTestAlert(
        this PriceAlertTests _,
        AlertCondition condition = AlertCondition.GreaterThan,
        decimal threshold = 5m,
        bool enabled = true,
        string asset = "BTC",
        string fiat = "USD")
    {
        return new PriceAlert
        {
            Asset = asset,
            Fiat = fiat,
            AlertType = AlertType.PriceChange,
            Condition = condition,
            Threshold = threshold,
            IsEnabled = enabled,
            UserId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test Spread with the specified parameters
    /// </summary>
    public static Spread CreateTestSpread(
        this PriceAlertTests _,
        decimal currentSpread = 0.5m,
        decimal averageSpread = 0.5m,
        decimal minSpread = 0.3m,
        decimal maxSpread = 0.8m,
        long sampleCount = 10)
    {
        return new Spread
        {
            Asset = "BTC",
            Fiat = "USD",
            CurrentSpreadPercent = currentSpread,
            AverageSpreadPercent = averageSpread,
            MinSpreadPercent = minSpread,
            MaxSpreadPercent = maxSpread,
            SampleCount = sampleCount,
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Returns the result of ShouldTrigger for fluent assertions
    /// </summary>
    public static bool ShouldTrigger(
        this PriceAlertTests _,
        PriceAlert alert,
        decimal currentChange)
    {
        return alert.ShouldTrigger(currentChange);
    }

    /// <summary>
    /// Returns whether an alert is in cooldown period
    /// </summary>
    public static bool IsInCooldownPeriod(
        this PriceAlertTests _,
        PriceAlert alert,
        int cooldownMinutes = 5)
    {
        return alert.IsInCooldownPeriod(cooldownMinutes);
    }

    /// <summary>
    /// Returns the trigger count of an alert
    /// </summary>
    public static int TriggerCount(
        this PriceAlertTests _,
        PriceAlert alert)
    {
        return alert.TriggerCount;
    }

    /// <summary>
    /// Returns the last triggered timestamp of an alert
    /// </summary>
    public static DateTime? LastTriggeredAt(
        this PriceAlertTests _,
        PriceAlert alert)
    {
        if (alert.LastTriggeredAt is null)
        {
            return null;
        }
        return DateTime.FromBinary(alert.LastTriggeredAt.Value);
    }

    /// <summary>
    /// Returns whether an alert is enabled
    /// </summary>
    public static bool IsEnabled(
        this PriceAlertTests _,
        PriceAlert alert)
    {
        return alert.IsEnabled;
    }

    /// <summary>
    /// Returns the description of an alert
    /// </summary>
    public static string GetDescription(
        this PriceAlertTests _,
        PriceAlert alert)
    {
        return alert.GetDescription();
    }

    /// <summary>
    /// Returns whether an alert is valid
    /// </summary>
    public static bool IsValid(
        this PriceAlertTests _,
        PriceAlert alert)
    {
        return alert.IsValid();
    }

    /// <summary>
    /// Returns the risk level of a spread
    /// </summary>
    public static string GetRiskLevel(
        this PriceAlertTests _,
        Spread spread)
    {
        return spread.GetRiskLevel();
    }

    /// <summary>
    /// Returns whether a spread is high
    /// </summary>
    public static bool IsHighSpread(
        this PriceAlertTests _,
        Spread spread,
        decimal threshold = 1.5m)
    {
        return spread.IsHighSpread(threshold);
    }

    /// <summary>
    /// Returns whether a spread is low
    /// </summary>
    public static bool IsLowSpread(
        this PriceAlertTests _,
        Spread spread,
        decimal threshold = 0.3m)
    {
        return spread.IsLowSpread(threshold);
    }

    /// <summary>
    /// Returns whether a spread is normal
    /// </summary>
    public static bool IsNormal(
        this PriceAlertTests _,
        Spread spread)
    {
        return spread.IsNormal();
    }

    /// <summary>
    /// Returns the variance from average of a spread
    /// </summary>
    public static decimal GetVarianceFromAverage(
        this PriceAlertTests _,
        Spread spread)
    {
        return spread.GetVarianceFromAverage();
    }

    /// <summary>
    /// Returns the sample count of a spread
    /// </summary>
    public static long SampleCount(
        this PriceAlertTests _,
        Spread spread)
    {
        return spread.SampleCount;
    }

    /// <summary>
    /// Returns whether a spread is valid
    /// </summary>
    public static bool IsValid(
        this PriceAlertTests _,
        Spread spread)
    {
        return spread.IsValid();
    }

    /// <summary>
    /// Records a trigger on an alert and returns the updated alert
    /// </summary>
    public static PriceAlert RecordTrigger(
        this PriceAlertTests _,
        PriceAlert alert)
    {
        alert.RecordTrigger();
        return alert;
    }

    /// <summary>
    /// Toggles an alert and returns the updated alert
    /// </summary>
    public static PriceAlert Toggle(
        this PriceAlertTests _,
        PriceAlert alert)
    {
        alert.Toggle();
        return alert;
    }

    /// <summary>
    /// Updates spread statistics and returns the updated spread
    /// </summary>
    public static Spread UpdateStatistics(
        this PriceAlertTests _,
        Spread spread,
        decimal newSpread)
    {
        spread.UpdateStatistics(newSpread);
        return spread;
    }
}
