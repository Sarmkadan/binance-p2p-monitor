#nullable enable

using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Constants;

namespace BinanceP2pMonitor.Tests;

public static class PriceAlertTestsExtensions
{
    /// <summary>
    /// Creates a test PriceAlert with the specified parameters
    /// </summary>
    /// <param name="condition">The alert condition to use.</param>
    /// <param name="threshold">The threshold value for the alert.</param>
    /// <param name="enabled">Whether the alert is enabled.</param>
    /// <param name="asset">The asset to monitor.</param>
    /// <param name="fiat">The fiat currency to use.</param>
    /// <returns>A new PriceAlert instance with test data.</returns>
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
    /// <param name="currentSpread">The current spread percentage.</param>
    /// <param name="averageSpread">The average spread percentage.</param>
    /// <param name="minSpread">The minimum spread percentage.</param>
    /// <param name="maxSpread">The maximum spread percentage.</param>
    /// <param name="sampleCount">The number of samples.</param>
    /// <returns>A new Spread instance with test data.</returns>
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
    /// <param name="alert">The price alert to check.</param>
    /// <param name="currentChange">The current price change percentage to test against.</param>
    /// <returns>True if the alert should trigger; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="alert"/> is null.</exception>
    public static bool ShouldTrigger(
        this PriceAlertTests _,
        PriceAlert alert,
        decimal currentChange)
    {
        ArgumentNullException.ThrowIfNull(alert);

        return alert.ShouldTrigger(currentChange);
    }

    /// <summary>
    /// Returns whether an alert is in cooldown period
    /// </summary>
    /// <param name="alert">The price alert to check.</param>
    /// <param name="cooldownMinutes">The cooldown period in minutes.</param>
    /// <returns>True if the alert is in cooldown; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="alert"/> is null.</exception>
    public static bool IsInCooldownPeriod(
        this PriceAlertTests _,
        PriceAlert alert,
        int cooldownMinutes = 5)
    {
        ArgumentNullException.ThrowIfNull(alert);

        return alert.IsInCooldownPeriod(cooldownMinutes);
    }

    /// <summary>
    /// Returns the trigger count of an alert
    /// </summary>
    /// <param name="alert">The price alert to check.</param>
    /// <returns>The number of times the alert has triggered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="alert"/> is null.</exception>
    public static int TriggerCount(
        this PriceAlertTests _,
        PriceAlert alert)
    {
        ArgumentNullException.ThrowIfNull(alert);

        return alert.TriggerCount;
    }

    /// <summary>
    /// Returns the last triggered timestamp of an alert
    /// </summary>
    /// <param name="alert">The price alert to check.</param>
    /// <returns>The last triggered timestamp as DateTime, or null if never triggered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="alert"/> is null.</exception>
    public static DateTime? LastTriggeredAt(
        this PriceAlertTests _,
        PriceAlert alert)
    {
        ArgumentNullException.ThrowIfNull(alert);

        return alert.LastTriggeredAt.HasValue
            ? DateTime.FromBinary(alert.LastTriggeredAt.Value)
            : null;
    }

    /// <summary>
    /// Returns whether an alert is enabled
    /// </summary>
    /// <param name="alert">The price alert to check.</param>
    /// <returns>True if the alert is enabled; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="alert"/> is null.</exception>
    public static bool IsEnabled(
        this PriceAlertTests _,
        PriceAlert alert)
    {
        ArgumentNullException.ThrowIfNull(alert);

        return alert.IsEnabled;
    }

    /// <summary>
    /// Returns the description of an alert
    /// </summary>
    /// <param name="alert">The price alert to get the description for.</param>
    /// <returns>A human-readable description of the alert.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="alert"/> is null.</exception>
    public static string GetDescription(
        this PriceAlertTests _,
        PriceAlert alert)
    {
        ArgumentNullException.ThrowIfNull(alert);

        return alert.GetDescription();
    }

    /// <summary>
    /// Returns whether an alert is valid
    /// </summary>
    /// <param name="alert">The price alert to validate.</param>
    /// <returns>True if the alert configuration is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="alert"/> is null.</exception>
    public static bool IsValid(
        this PriceAlertTests _,
        PriceAlert alert)
    {
        ArgumentNullException.ThrowIfNull(alert);

        return alert.IsValid();
    }

    /// <summary>
    /// Returns the risk level of a spread
    /// </summary>
    /// <param name="spread">The spread to evaluate.</param>
    /// <returns>A string representing the risk level.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="spread"/> is null.</exception>
    public static string GetRiskLevel(
        this PriceAlertTests _,
        Spread spread)
    {
        ArgumentNullException.ThrowIfNull(spread);

        return spread.GetRiskLevel();
    }

    /// <summary>
    /// Returns whether a spread is high
    /// </summary>
    /// <param name="spread">The spread to check.</param>
    /// <param name="threshold">The threshold percentage to consider as high spread.</param>
    /// <returns>True if the spread is high; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="spread"/> is null.</exception>
    public static bool IsHighSpread(
        this PriceAlertTests _,
        Spread spread,
        decimal threshold = 1.5m)
    {
        ArgumentNullException.ThrowIfNull(spread);

        return spread.IsHighSpread(threshold);
    }

    /// <summary>
    /// Returns whether a spread is low
    /// </summary>
    /// <param name="spread">The spread to check.</param>
    /// <param name="threshold">The threshold percentage to consider as low spread.</param>
    /// <returns>True if the spread is low; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="spread"/> is null.</exception>
    public static bool IsLowSpread(
        this PriceAlertTests _,
        Spread spread,
        decimal threshold = 0.3m)
    {
        ArgumentNullException.ThrowIfNull(spread);

        return spread.IsLowSpread(threshold);
    }

    /// <summary>
    /// Returns whether a spread is normal
    /// </summary>
    /// <param name="spread">The spread to check.</param>
    /// <returns>True if the spread is within normal range; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="spread"/> is null.</exception>
    public static bool IsNormal(
        this PriceAlertTests _,
        Spread spread)
    {
        ArgumentNullException.ThrowIfNull(spread);

        return spread.IsNormal();
    }

    /// <summary>
    /// Returns the variance from average of a spread
    /// </summary>
    /// <param name="spread">The spread to calculate variance for.</param>
    /// <returns>The variance percentage from the average spread.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="spread"/> is null.</exception>
    public static decimal GetVarianceFromAverage(
        this PriceAlertTests _,
        Spread spread)
    {
        ArgumentNullException.ThrowIfNull(spread);

        return spread.GetVarianceFromAverage();
    }

    /// <summary>
    /// Returns the sample count of a spread
    /// </summary>
    /// <param name="spread">The spread to get the sample count for.</param>
    /// <returns>The number of samples collected.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="spread"/> is null.</exception>
    public static long SampleCount(
        this PriceAlertTests _,
        Spread spread)
    {
        ArgumentNullException.ThrowIfNull(spread);

        return spread.SampleCount;
    }

    /// <summary>
    /// Returns whether a spread is valid
    /// </summary>
    /// <param name="spread">The spread to validate.</param>
    /// <returns>True if the spread data is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="spread"/> is null.</exception>
    public static bool IsValid(
        this PriceAlertTests _,
        Spread spread)
    {
        ArgumentNullException.ThrowIfNull(spread);

        return spread.IsValid();
    }

    /// <summary>
    /// Records a trigger on an alert and returns the updated alert
    /// </summary>
    /// <param name="alert">The price alert to record the trigger for.</param>
    /// <returns>The updated price alert with incremented trigger count.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="alert"/> is null.</exception>
    public static PriceAlert RecordTrigger(
        this PriceAlertTests _,
        PriceAlert alert)
    {
        ArgumentNullException.ThrowIfNull(alert);

        alert.RecordTrigger();
        return alert;
    }

    /// <summary>
    /// Toggles an alert and returns the updated alert
    /// </summary>
    /// <param name="alert">The price alert to toggle.</param>
    /// <returns>The updated price alert with toggled enabled state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="alert"/> is null.</exception>
    public static PriceAlert Toggle(
        this PriceAlertTests _,
        PriceAlert alert)
    {
        ArgumentNullException.ThrowIfNull(alert);

        alert.Toggle();
        return alert;
    }

    /// <summary>
    /// Updates spread statistics and returns the updated spread
    /// </summary>
    /// <param name="spread">The spread to update.</param>
    /// <param name="newSpread">The new spread percentage to add to statistics.</param>
    /// <returns>The updated spread with new statistics.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="spread"/> is null.</exception>
    public static Spread UpdateStatistics(
        this PriceAlertTests _,
        Spread spread,
        decimal newSpread)
    {
        ArgumentNullException.ThrowIfNull(spread);

        spread.UpdateStatistics(newSpread);
        return spread;
    }
}