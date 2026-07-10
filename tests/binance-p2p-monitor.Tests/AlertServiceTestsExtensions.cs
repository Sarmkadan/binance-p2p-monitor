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
    public static PriceAlert CreateValidAlert(int userId = 1, string asset = "USDT", string fiat = "UAH")
    {
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
    public static PriceAlert WithId(this PriceAlert alert, int id)
    {
        alert.Id = id;
        return alert;
    }

    /// <summary>
    /// Creates a list of alerts for a specific user.
    /// </summary>
    public static List<PriceAlert> CreateAlertList(int userId, int count = 3, string baseAsset = "USDT")
    {
        var alerts = new List<PriceAlert>();
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
    public static void ShouldHaveExpectedProperties(this PriceAlert alert, int expectedUserId, string expectedAsset, string expectedFiat, AlertType expectedType, AlertCondition expectedCondition, decimal expectedThreshold)
    {
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
    public static PriceAlert WithThreshold(this PriceAlert alert, decimal threshold)
    {
        alert.Threshold = threshold;
        return alert;
    }

    /// <summary>
    /// Creates an alert with a specific condition.
    /// </summary>
    public static PriceAlert WithCondition(this PriceAlert alert, AlertCondition condition)
    {
        alert.Condition = condition;
        return alert;
    }

    /// <summary>
    /// Creates an alert with a specific alert type.
    /// </summary>
    public static PriceAlert WithType(this PriceAlert alert, AlertType type)
    {
        alert.AlertType = type;
        return alert;
    }

    /// <summary>
    /// Creates an alert that is disabled.
    /// </summary>
    public static PriceAlert Disabled(this PriceAlert alert)
    {
        alert.IsEnabled = false;
        return alert;
    }
}