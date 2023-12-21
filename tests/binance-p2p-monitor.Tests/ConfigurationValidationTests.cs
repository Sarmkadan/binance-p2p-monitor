// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Exceptions;
using FluentAssertions;
using Xunit;

namespace BinanceP2pMonitor.Tests;

public class ConfigurationValidationTests
{
    [Fact]
    public void Validate_ShouldNotThrowException_WhenSettingsAreValid()
    {
        // Arrange
        var settings = new AppSettings
        {
            DatabaseConnectionString = "Data Source=test.db",
            MonitoringIntervalSeconds = 30,
            AlertCooldownMinutes = 5,
            MaxAlertsPerUser = 20,
            HistoryRetentionDays = 30,
            DefaultPriceChangeThreshold = 0.0m,
            DefaultSpreadThreshold = 0.0m
        };

        // Act
        Action action = () => settings.Validate();

        // Assert
        action.Should().NotThrow<ConfigurationException>();
    }

    [Theory]
    [InlineData(null, "DatabaseConnectionString is required")]
    [InlineData("", "DatabaseConnectionString is required")]
    public void Validate_ShouldThrowException_WhenDatabaseConnectionStringIsInvalid(string connectionString, string expectedMessage)
    {
        // Arrange
        var settings = new AppSettings
        {
            DatabaseConnectionString = connectionString,
            MonitoringIntervalSeconds = 30,
            AlertCooldownMinutes = 5,
            MaxAlertsPerUser = 20,
            HistoryRetentionDays = 30,
            DefaultPriceChangeThreshold = 0.0m,
            DefaultSpreadThreshold = 0.0m
        };

        // Act
        Action action = () => settings.Validate();

        // Assert
        action.Should().Throw<ConfigurationException>()
            .WithMessage($"Configuration validation failed: {expectedMessage}");
    }

    [Theory]
    [InlineData(0, "MonitoringIntervalSeconds must be at least 5")]
    [InlineData(4, "MonitoringIntervalSeconds must be at least 5")]
    public void Validate_ShouldThrowException_WhenMonitoringIntervalSecondsIsInvalid(int interval, string expectedMessage)
    {
        // Arrange
        var settings = new AppSettings
        {
            DatabaseConnectionString = "Data Source=test.db",
            MonitoringIntervalSeconds = interval,
            AlertCooldownMinutes = 5,
            MaxAlertsPerUser = 20,
            HistoryRetentionDays = 30,
            DefaultPriceChangeThreshold = 0.0m,
            DefaultSpreadThreshold = 0.0m
        };

        // Act
        Action action = () => settings.Validate();

        // Assert
        action.Should().Throw<ConfigurationException>()
            .WithMessage($"Configuration validation failed: {expectedMessage}");
    }

    [Theory]
    [InlineData(0, "AlertCooldownMinutes must be at least 1")]
    public void Validate_ShouldThrowException_WhenAlertCooldownMinutesIsInvalid(int cooldown, string expectedMessage)
    {
        // Arrange
        var settings = new AppSettings
        {
            DatabaseConnectionString = "Data Source=test.db",
            MonitoringIntervalSeconds = 30,
            AlertCooldownMinutes = cooldown,
            MaxAlertsPerUser = 20,
            HistoryRetentionDays = 30,
            DefaultPriceChangeThreshold = 0.0m,
            DefaultSpreadThreshold = 0.0m
        };

        // Act
        Action action = () => settings.Validate();

        // Assert
        action.Should().Throw<ConfigurationException>()
            .WithMessage($"Configuration validation failed: {expectedMessage}");
    }

    [Theory]
    [InlineData(0, "MaxAlertsPerUser must be at least 1")]
    public void Validate_ShouldThrowException_WhenMaxAlertsPerUserIsInvalid(int maxAlerts, string expectedMessage)
    {
        // Arrange
        var settings = new AppSettings
        {
            DatabaseConnectionString = "Data Source=test.db",
            MonitoringIntervalSeconds = 30,
            AlertCooldownMinutes = 5,
            MaxAlertsPerUser = maxAlerts,
            HistoryRetentionDays = 30,
            DefaultPriceChangeThreshold = 0.0m,
            DefaultSpreadThreshold = 0.0m
        };

        // Act
        Action action = () => settings.Validate();

        // Assert
        action.Should().Throw<ConfigurationException>()
            .WithMessage($"Configuration validation failed: {expectedMessage}");
    }

    [Theory]
    [InlineData(0, "HistoryRetentionDays must be at least 1")]
    public void Validate_ShouldThrowException_WhenHistoryRetentionDaysIsInvalid(int retentionDays, string expectedMessage)
    {
        // Arrange
        var settings = new AppSettings
        {
            DatabaseConnectionString = "Data Source=test.db",
            MonitoringIntervalSeconds = 30,
            AlertCooldownMinutes = 5,
            MaxAlertsPerUser = 20,
            HistoryRetentionDays = retentionDays,
            DefaultPriceChangeThreshold = 0.0m,
            DefaultSpreadThreshold = 0.0m
        };

        // Act
        Action action = () => settings.Validate();

        // Assert
        action.Should().Throw<ConfigurationException>()
            .WithMessage($"Configuration validation failed: {expectedMessage}");
    }

    [Fact]
    public void Validate_ShouldThrowException_WhenDefaultPriceChangeThresholdIsNegative()
    {
        // Arrange
        var settings = new AppSettings
        {
            DatabaseConnectionString = "Data Source=test.db",
            MonitoringIntervalSeconds = 30,
            AlertCooldownMinutes = 5,
            MaxAlertsPerUser = 20,
            HistoryRetentionDays = 30,
            DefaultPriceChangeThreshold = -1.0m,
            DefaultSpreadThreshold = 0.0m
        };

        // Act
        Action action = () => settings.Validate();

        // Assert
        action.Should().Throw<ConfigurationException>()
            .WithMessage("Configuration validation failed: DefaultPriceChangeThreshold cannot be negative");
    }

    [Fact]
    public void Validate_ShouldThrowException_WhenDefaultSpreadThresholdIsNegative()
    {
        // Arrange
        var settings = new AppSettings
        {
            DatabaseConnectionString = "Data Source=test.db",
            MonitoringIntervalSeconds = 30,
            AlertCooldownMinutes = 5,
            MaxAlertsPerUser = 20,
            HistoryRetentionDays = 30,
            DefaultPriceChangeThreshold = 0.0m,
            DefaultSpreadThreshold = -1.0m
        };

        // Act
        Action action = () => settings.Validate();

        // Assert
        action.Should().Throw<ConfigurationException>()
            .WithMessage("Configuration validation failed: DefaultSpreadThreshold cannot be negative");
    }
}
