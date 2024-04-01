#nullable enable

using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Constants;
using BinanceP2pMonitor.Exceptions;
using FluentAssertions;

namespace BinanceP2pMonitor.Tests;

public static class AlertServiceTestsExtensions
{
    /// <summary>
    /// Creates a valid price alert with the specified parameters for testing.
    /// </summary>
    /// <param name="userId">The user identifier. Defaults to 1.</param>
    /// <param name="asset">The asset symbol. Defaults to "USDT".</param>
    /// <param name="fiat">The fiat currency. Defaults to "UAH".</param>
    /// <returns>A new <see cref="PriceAlert"/> instance with default valid values.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="userId"/> is less than 1.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="asset"/> or <paramref name="fiat"/> is null or whitespace.</exception>
    public static PriceAlert CreateValidAlert(int userId = 1, string asset = "USDT", string fiat = "UAH")
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(userId, 1);
        ArgumentException.ThrowIfNullOrWhiteSpace(asset);
        ArgumentException.ThrowIfNullOrWhiteSpace(fiat);

        return new PriceAlert
        {
            UserId = userId,
            Asset = asset,
            Fiat = fiat,
            AlertType = AlertType.PriceChange,
            Condition = AlertCondition.GreaterThan,
            Threshold = 1.0m,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a price alert with the specified ID for testing.
    /// </summary>
    /// <param name="id">The alert identifier.</param>
    /// <returns>The same <see cref="PriceAlert"/> instance with updated ID.</returns>
    public static PriceAlert WithId(this PriceAlert alert, int id)
    {
        ArgumentNullException.ThrowIfNull(alert);
        ArgumentOutOfRangeException.ThrowIfLessThan(id, 1);

        alert.Id = id;
        return alert;
    }

    /// <summary>
    /// Creates a list of alerts for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="count">The number of alerts to create. Defaults to 3.</param>
    /// <param name="baseAsset">The base asset symbol. Defaults to "USDT".</param>
    /// <returns>A list of <see cref="PriceAlert"/> instances.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="userId"/> is less than 1 or <paramref name="count"/> is less than 0.</exception>
    public static List<PriceAlert> CreateAlertList(int userId, int count = 3, string baseAsset = "USDT")
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(userId, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 0);
        ArgumentException.ThrowIfNullOrWhiteSpace(baseAsset);

        var alerts = new List<PriceAlert>(count);
        for (int i = 0; i < count; i++)
        {
            alerts.Add(new PriceAlert
            {
                UserId = userId,
                Asset = $"{baseAsset}{i}",
                Fiat = "UAH",
                AlertType = AlertType.PriceChange,
                Condition = AlertCondition.GreaterThan,
                Threshold = 1.0m + i,
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-i)
            }.WithId(i + 1));
        }
        return alerts;
    }

    /// <summary>
    /// Asserts that an alert has the expected properties.
    /// </summary>
    /// <param name="expectedUserId">The expected user identifier.</param>
    /// <param name="expectedAsset">The expected asset symbol.</param>
    /// <param name="expectedFiat">The expected fiat currency.</param>
    /// <param name="expectedType">The expected alert type.</param>
    /// <param name="expectedCondition">The expected alert condition.</param>
    /// <param name="expectedThreshold">The expected threshold value.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="alert"/> is null.</exception>
    public static void ShouldHaveExpectedProperties(this PriceAlert alert, int expectedUserId, string expectedAsset, string expectedFiat, AlertType expectedType, AlertCondition expectedCondition, decimal expectedThreshold)
    {
        ArgumentNullException.ThrowIfNull(alert);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedAsset);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedFiat);

        alert.UserId.Should().Be(expectedUserId);
        alert.Asset.Should().Be(expectedAsset);
        alert.Fiat.Should().Be(expectedFiat);
        alert.AlertType.Should().Be(expectedType);
        alert.Condition.Should().Be(expectedCondition);
        alert.Threshold.Should().Be(expectedThreshold);
    }

    /// <summary>
    /// Creates an alert with a specific threshold value.
    /// </summary>
    /// <param name="threshold">The threshold value to set.</param>
    /// <returns>The same <see cref="PriceAlert"/> instance with updated threshold.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="alert"/> is null.</exception>
    public static PriceAlert WithThreshold(this PriceAlert alert, decimal threshold)
    {
        ArgumentNullException.ThrowIfNull(alert);

        alert.Threshold = threshold;
        return alert;
    }

    /// <summary>
    /// Creates an alert with a specific condition.
    /// </summary>
    /// <param name="condition">The alert condition to set.</param>
    /// <returns>The same <see cref="PriceAlert"/> instance with updated condition.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="alert"/> is null.</exception>
    public static PriceAlert WithCondition(this PriceAlert alert, AlertCondition condition)
    {
        ArgumentNullException.ThrowIfNull(alert);

        alert.Condition = condition;
        return alert;
    }

    /// <summary>
    /// Creates an alert with a specific alert type.
    /// </summary>
    /// <param name="type">The alert type to set.</param>
    /// <returns>The same <see cref="PriceAlert"/> instance with updated alert type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="alert"/> is null.</exception>
    public static PriceAlert WithType(this PriceAlert alert, AlertType type)
    {
        ArgumentNullException.ThrowIfNull(alert);

        alert.AlertType = type;
        return alert;
    }

    /// <summary>
    /// Creates an alert that is disabled.
    /// </summary>
    /// <returns>The same <see cref="PriceAlert"/> instance with IsEnabled set to false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="alert"/> is null.</exception>
    public static PriceAlert Disabled(this PriceAlert alert)
    {
        ArgumentNullException.ThrowIfNull(alert);

        alert.IsEnabled = false;
        return alert;
    }
}