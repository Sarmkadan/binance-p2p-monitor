#nullable enable
using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Exceptions;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests for the <see cref="AppSettings"/> class.
/// </summary>
public class ConfigurationValidationTests
{
    /// <summary>
    /// Tests that <see cref="AppSettings.Validate()"/> does not throw an exception when the settings are valid.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="AppSettings.Validate()"/> throws a <see cref="ConfigurationException"/> when the database connection string is invalid.
    /// </summary>
    /// <param name="connectionString">The database connection string to test.</param>
    /// <param name="expectedMessage">The expected error message.</param>
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

    /// <summary>
    /// Tests that <see cref="AppSettings.Validate()"/> throws a <see cref="ConfigurationException"/> when the monitoring interval seconds is invalid.
    /// </summary>
    /// <param name="interval">The monitoring interval seconds to test.</param>
    /// <param name="expectedMessage">The expected error message.</param>
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

    /// <summary>
    /// Tests that <see cref="AppSettings.Validate()"/> throws a <see cref="ConfigurationException"/> when the alert cooldown minutes is invalid.
    /// </summary>
    /// <param name="cooldown">The alert cooldown minutes to test.</param>
    /// <param name="expectedMessage">The expected error message.</param>
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

    /// <summary>
    /// Tests that <see cref="AppSettings.Validate()"/> throws a <see cref="ConfigurationException"/> when the max alerts per user is invalid.
    /// </summary>
    /// <param name="maxAlerts">The max alerts per user to test.</param>
    /// <param name="expectedMessage">The expected error message.</param>
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

    /// <summary>
    /// Tests that <see cref="AppSettings.Validate()"/> throws a <see cref="ConfigurationException"/> when the history retention days is invalid.
    /// </summary>
    /// <param name="retentionDays">The history retention days to test.</param>
    /// <param name="expectedMessage">The expected error message.</param>
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

    /// <summary>
    /// Tests that <see cref="AppSettings.Validate()"/> throws a <see cref="ConfigurationException"/> when the default price change threshold is negative.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="AppSettings.Validate()"/> throws a <see cref="ConfigurationException"/> when the default spread threshold is negative.
    /// </summary>
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
