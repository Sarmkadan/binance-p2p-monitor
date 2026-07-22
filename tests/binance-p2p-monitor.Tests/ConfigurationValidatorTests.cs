using BinanceP2pMonitor.Infrastructure;
using BinanceP2pMonitor.Configuration;
using FluentAssertions;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;

public class ConfigurationValidatorTests
{
    private readonly Mock<ILogger<ConfigurationValidator>> _mockLogger;

    public ConfigurationValidatorTests()
    {
        _mockLogger = new Mock<ILogger<ConfigurationValidator>>();
    }

    [Fact]
    public void Validate_ValidConfig_ReturnsEmptyList()
    {
        // Arrange
        var config = new AppSettings
        {
            DatabaseConnectionString = "Data Source=test.db",
            MonitoringIntervalSeconds = 30,
            AlertCooldownMinutes = 5,
            MaxAlertsPerUser = 20,
            HistoryRetentionDays = 30,
            DefaultPriceChangeThreshold = 0.0m,
            DefaultSpreadThreshold = 0.0m,
            MaxHistoryRecords = 100000,
            DatabaseCommandTimeoutSeconds = 30,
            EnableWebSocket = true,
            EnableTelegramNotifications = false,
            MonitoredAssets = new List<string> { "USDT" },
            MonitoredFiats = new List<string> { "RUB" }
        };

        var validator = new ConfigurationValidator(config, _mockLogger.Object);

        // Act
        var errors = validator.Validate();

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_InvalidDatabaseConnectionString_ReturnsError()
    {
        // Arrange
        var config = new AppSettings
        {
            DatabaseConnectionString = "",
            MonitoringIntervalSeconds = 30,
            AlertCooldownMinutes = 5,
            MaxAlertsPerUser = 20,
            HistoryRetentionDays = 30,
            DefaultPriceChangeThreshold = 0.0m,
            DefaultSpreadThreshold = 0.0m,
            MaxHistoryRecords = 100000,
            DatabaseCommandTimeoutSeconds = 30,
            EnableWebSocket = true,
            EnableTelegramNotifications = false,
            MonitoredAssets = new List<string> { "USDT" },
            MonitoredFiats = new List<string> { "RUB" }
        };

        var validator = new ConfigurationValidator(config, _mockLogger.Object);

        // Act
        var errors = validator.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "DatabaseConnectionString is required");
    }

    [Fact]
    public void Validate_EmptyDatabaseConnectionString_ReturnsError()
    {
        // Arrange
        var config = new AppSettings
        {
            DatabaseConnectionString = "   ",
            MonitoringIntervalSeconds = 30,
            AlertCooldownMinutes = 5,
            MaxAlertsPerUser = 20,
            HistoryRetentionDays = 30,
            DefaultPriceChangeThreshold = 0.0m,
            DefaultSpreadThreshold = 0.0m,
            MaxHistoryRecords = 100000,
            DatabaseCommandTimeoutSeconds = 30,
            EnableWebSocket = true,
            EnableTelegramNotifications = false,
            MonitoredAssets = new List<string> { "USDT" },
            MonitoredFiats = new List<string> { "RUB" }
        };

        var validator = new ConfigurationValidator(config, _mockLogger.Object);

        // Act
        var errors = validator.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "DatabaseConnectionString is required");
    }

    [Fact]
    public void Validate_HistoryRetentionDaysTooLow_ReturnsError()
    {
        // Arrange
        var config = new AppSettings
        {
            DatabaseConnectionString = "Data Source=test.db",
            MonitoringIntervalSeconds = 30,
            AlertCooldownMinutes = 5,
            MaxAlertsPerUser = 20,
            HistoryRetentionDays = 0,
            DefaultPriceChangeThreshold = 0.0m,
            DefaultSpreadThreshold = 0.0m,
            MaxHistoryRecords = 100000,
            DatabaseCommandTimeoutSeconds = 30,
            EnableWebSocket = true,
            EnableTelegramNotifications = false,
            MonitoredAssets = new List<string> { "USDT" },
            MonitoredFiats = new List<string> { "RUB" }
        };

        var validator = new ConfigurationValidator(config, _mockLogger.Object);

        // Act
        var errors = validator.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "HistoryRetentionDays must be at least 1");
    }

    [Fact]
    public void Validate_MaxHistoryRecordsTooLow_ReturnsError()
    {
        // Arrange
        var config = new AppSettings
        {
            DatabaseConnectionString = "Data Source=test.db",
            MonitoringIntervalSeconds = 30,
            AlertCooldownMinutes = 5,
            MaxAlertsPerUser = 20,
            HistoryRetentionDays = 30,
            DefaultPriceChangeThreshold = 0.0m,
            DefaultSpreadThreshold = 0.0m,
            MaxHistoryRecords = 50,
            DatabaseCommandTimeoutSeconds = 30,
            EnableWebSocket = true,
            EnableTelegramNotifications = false,
            MonitoredAssets = new List<string> { "USDT" },
            MonitoredFiats = new List<string> { "RUB" }
        };

        var validator = new ConfigurationValidator(config, _mockLogger.Object);

        // Act
        var errors = validator.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "MaxHistoryRecords must be at least 100");
    }

    [Fact]
    public void Validate_DatabaseCommandTimeoutTooLow_ReturnsError()
    {
        // Arrange
        var config = new AppSettings
        {
            DatabaseConnectionString = "Data Source=test.db",
            MonitoringIntervalSeconds = 30,
            AlertCooldownMinutes = 5,
            MaxAlertsPerUser = 20,
            HistoryRetentionDays = 30,
            DefaultPriceChangeThreshold = 0.0m,
            DefaultSpreadThreshold = 0.0m,
            MaxHistoryRecords = 100000,
            DatabaseCommandTimeoutSeconds = 3,
            EnableWebSocket = true,
            EnableTelegramNotifications = false,
            MonitoredAssets = new List<string> { "USDT" },
            MonitoredFiats = new List<string> { "RUB" }
        };

        var validator = new ConfigurationValidator(config, _mockLogger.Object);

        // Act
        var errors = validator.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "DatabaseCommandTimeoutSeconds must be at least 5");
    }

    [Fact]
    public void Validate_MonitoringIntervalTooLow_ReturnsError()
    {
        // Arrange
        var config = new AppSettings
        {
            DatabaseConnectionString = "Data Source=test.db",
            MonitoringIntervalSeconds = 3,
            AlertCooldownMinutes = 5,
            MaxAlertsPerUser = 20,
            HistoryRetentionDays = 30,
            DefaultPriceChangeThreshold = 0.0m,
            DefaultSpreadThreshold = 0.0m,
            MaxHistoryRecords = 100000,
            DatabaseCommandTimeoutSeconds = 30,
            EnableWebSocket = true,
            EnableTelegramNotifications = false,
            MonitoredAssets = new List<string> { "USDT" },
            MonitoredFiats = new List<string> { "RUB" }
        };

        var validator = new ConfigurationValidator(config, _mockLogger.Object);

        // Act
        var errors = validator.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "MonitoringIntervalSeconds must be at least 5 seconds");
    }

    [Fact]
    public void Validate_NoNotificationMethodEnabled_ReturnsError()
    {
        // Arrange
        var config = new AppSettings
        {
            DatabaseConnectionString = "Data Source=test.db",
            MonitoringIntervalSeconds = 30,
            AlertCooldownMinutes = 5,
            MaxAlertsPerUser = 20,
            HistoryRetentionDays = 30,
            DefaultPriceChangeThreshold = 0.0m,
            DefaultSpreadThreshold = 0.0m,
            MaxHistoryRecords = 100000,
            DatabaseCommandTimeoutSeconds = 30,
            EnableWebSocket = false,
            EnableTelegramNotifications = false,
            MonitoredAssets = new List<string> { "USDT" },
            MonitoredFiats = new List<string> { "RUB" }
        };

        var validator = new ConfigurationValidator(config, _mockLogger.Object);

        // Act
        var errors = validator.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "At least one notification method must be enabled");
    }

    [Fact]
    public void Validate_AlertCooldownTooLow_ReturnsError()
    {
        // Arrange
        var config = new AppSettings
        {
            DatabaseConnectionString = "Data Source=test.db",
            MonitoringIntervalSeconds = 30,
            AlertCooldownMinutes = 0,
            MaxAlertsPerUser = 20,
            HistoryRetentionDays = 30,
            DefaultPriceChangeThreshold = 0.0m,
            DefaultSpreadThreshold = 0.0m,
            MaxHistoryRecords = 100000,
            DatabaseCommandTimeoutSeconds = 30,
            EnableWebSocket = true,
            EnableTelegramNotifications = false,
            MonitoredAssets = new List<string> { "USDT" },
            MonitoredFiats = new List<string> { "RUB" }
        };

        var validator = new ConfigurationValidator(config, _mockLogger.Object);

        // Act
        var errors = validator.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "AlertCooldownMinutes must be at least 1");
    }

    [Fact]
    public void Validate_MaxAlertsPerUserTooLow_ReturnsError()
    {
        // Arrange
        var config = new AppSettings
        {
            DatabaseConnectionString = "Data Source=test.db",
            MonitoringIntervalSeconds = 30,
            AlertCooldownMinutes = 5,
            MaxAlertsPerUser = 0,
            HistoryRetentionDays = 30,
            DefaultPriceChangeThreshold = 0.0m,
            DefaultSpreadThreshold = 0.0m,
            MaxHistoryRecords = 100000,
            DatabaseCommandTimeoutSeconds = 30,
            EnableWebSocket = true,
            EnableTelegramNotifications = false,
            MonitoredAssets = new List<string> { "USDT" },
            MonitoredFiats = new List<string> { "RUB" }
        };

        var validator = new ConfigurationValidator(config, _mockLogger.Object);

        // Act
        var errors = validator.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "MaxAlertsPerUser must be at least 1");
    }

    [Fact]
    public void Validate_NegativePriceChangeThreshold_ReturnsError()
    {
        // Arrange
        var config = new AppSettings
        {
            DatabaseConnectionString = "Data Source=test.db",
            MonitoringIntervalSeconds = 30,
            AlertCooldownMinutes = 5,
            MaxAlertsPerUser = 20,
            HistoryRetentionDays = 30,
            DefaultPriceChangeThreshold = -1.0m,
            DefaultSpreadThreshold = 0.0m,
            MaxHistoryRecords = 100000,
            DatabaseCommandTimeoutSeconds = 30,
            EnableWebSocket = true,
            EnableTelegramNotifications = false,
            MonitoredAssets = new List<string> { "USDT" },
            MonitoredFiats = new List<string> { "RUB" }
        };

        var validator = new ConfigurationValidator(config, _mockLogger.Object);

        // Act
        var errors = validator.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "DefaultPriceChangeThreshold cannot be negative");
    }

    [Fact]
    public void Validate_NegativeSpreadThreshold_ReturnsError()
    {
        // Arrange
        var config = new AppSettings
        {
            DatabaseConnectionString = "Data Source=test.db",
            MonitoringIntervalSeconds = 30,
            AlertCooldownMinutes = 5,
            MaxAlertsPerUser = 20,
            HistoryRetentionDays = 30,
            DefaultPriceChangeThreshold = 0.0m,
            DefaultSpreadThreshold = -1.0m,
            MaxHistoryRecords = 100000,
            DatabaseCommandTimeoutSeconds = 30,
            EnableWebSocket = true,
            EnableTelegramNotifications = false,
            MonitoredAssets = new List<string> { "USDT" },
            MonitoredFiats = new List<string> { "RUB" }
        };

        var validator = new ConfigurationValidator(config, _mockLogger.Object);

        // Act
        var errors = validator.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "DefaultSpreadThreshold cannot be negative");
    }

    [Fact]
    public void Validate_TelegramEnabledWithoutToken_ReturnsError()
    {
        // Arrange
        var config = new AppSettings
        {
            DatabaseConnectionString = "Data Source=test.db",
            MonitoringIntervalSeconds = 30,
            AlertCooldownMinutes = 5,
            MaxAlertsPerUser = 20,
            HistoryRetentionDays = 30,
            DefaultPriceChangeThreshold = 0.0m,
            DefaultSpreadThreshold = 0.0m,
            MaxHistoryRecords = 100000,
            DatabaseCommandTimeoutSeconds = 30,
            EnableWebSocket = true,
            EnableTelegramNotifications = true,
            TelegramBotToken = "",
            TelegramAdminChatId = "123456",
            MonitoredAssets = new List<string> { "USDT" },
            MonitoredFiats = new List<string> { "RUB" }
        };

        var validator = new ConfigurationValidator(config, _mockLogger.Object);

        // Act
        var errors = validator.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "TelegramBotToken is required when EnableTelegramNotifications is true");
    }

    [Fact]
    public void Validate_TelegramEnabledWithoutChatId_ReturnsError()
    {
        // Arrange
        var config = new AppSettings
        {
            DatabaseConnectionString = "Data Source=test.db",
            MonitoringIntervalSeconds = 30,
            AlertCooldownMinutes = 5,
            MaxAlertsPerUser = 20,
            HistoryRetentionDays = 30,
            DefaultPriceChangeThreshold = 0.0m,
            DefaultSpreadThreshold = 0.0m,
            MaxHistoryRecords = 100000,
            DatabaseCommandTimeoutSeconds = 30,
            EnableWebSocket = true,
            EnableTelegramNotifications = true,
            TelegramBotToken = "test-token",
            TelegramAdminChatId = "",
            MonitoredAssets = new List<string> { "USDT" },
            MonitoredFiats = new List<string> { "RUB" }
        };

        var validator = new ConfigurationValidator(config, _mockLogger.Object);

        // Act
        var errors = validator.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "TelegramAdminChatId is required when EnableTelegramNotifications is true");
    }

    [Fact]
    public void Validate_TelegramInvalidChatId_ReturnsError()
    {
        // Arrange
        var config = new AppSettings
        {
            DatabaseConnectionString = "Data Source=test.db",
            MonitoringIntervalSeconds = 30,
            AlertCooldownMinutes = 5,
            MaxAlertsPerUser = 20,
            HistoryRetentionDays = 30,
            DefaultPriceChangeThreshold = 0.0m,
            DefaultSpreadThreshold = 0.0m,
            MaxHistoryRecords = 100000,
            DatabaseCommandTimeoutSeconds = 30,
            EnableWebSocket = true,
            EnableTelegramNotifications = true,
            TelegramBotToken = "test-token",
            TelegramAdminChatId = "invalid-chat-id",
            MonitoredAssets = new List<string> { "USDT" },
            MonitoredFiats = new List<string> { "RUB" }
        };

        var validator = new ConfigurationValidator(config, _mockLogger.Object);

        // Act
        var errors = validator.Validate();

        // Assert
        errors.Should().ContainSingle(e => e == "TelegramAdminChatId must be a valid numeric chat ID");
    }
}
