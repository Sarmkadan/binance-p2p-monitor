// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Constants;
using BinanceP2pMonitor.Exceptions;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using BinanceP2pMonitor.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace BinanceP2pMonitor.Tests;

public class AlertServiceTests
{
    private readonly IAlertRepository _mockAlertRepository;
    private readonly AppSettings _appSettings;
    private readonly ILogger<AlertService> _mockLogger;
    private readonly AlertService _alertService;

    public AlertServiceTests()
    {
        _mockAlertRepository = Substitute.For<IAlertRepository>();
        _appSettings = new AppSettings
        {
            MaxAlertsPerUser = 5,
            EnableTelegramNotifications = true
        };
        _mockLogger = Substitute.For<ILogger<AlertService>>();
        _alertService = new AlertService(_mockAlertRepository, _appSettings, _mockLogger);
    }

    [Fact]
    public async Task CreateAlertAsync_ShouldReturnAlertId_WhenAlertIsValidAndMaxAlertsNotReached()
    {
        // Arrange
        var alert = new PriceAlert
        {
            UserId = 1,
            Asset = "USDT",
            Fiat = "UAH",
            TradeType = TradeType.Buy,
            AlertType = AlertType.PriceChange,
            Threshold = 1.0m
        };
        _mockAlertRepository.GetUserAlertCountAsync(alert.UserId).Returns(2);
        _mockAlertRepository.AddAsync(Arg.Any<PriceAlert>()).Returns(1);

        // Act
        var result = await _alertService.CreateAlertAsync(alert);

        // Assert
        result.Should().Be(1);
        await _mockAlertRepository.Received(1).AddAsync(Arg.Is<PriceAlert>(a => a.UserId == alert.UserId));
    }

    [Fact]
    public async Task CreateAlertAsync_ShouldThrowInvalidAlertException_WhenAlertIsInvalid()
    {
        // Arrange
        var invalidAlert = new PriceAlert { UserId = 1 }; // Missing required fields

        // Act
        Func<Task> action = async () => await _alertService.CreateAlertAsync(invalidAlert);

        // Assert
        await action.Should().ThrowAsync<InvalidAlertException>()
            .WithMessage("Alert configuration is invalid");
        await _mockAlertRepository.DidNotReceive().AddAsync(Arg.Any<PriceAlert>());
    }

    [Fact]
    public async Task CreateAlertAsync_ShouldThrowInvalidAlertException_WhenMaxAlertsReached()
    {
        // Arrange
        var alert = new PriceAlert
        {
            UserId = 1,
            Asset = "USDT",
            Fiat = "UAH",
            TradeType = TradeType.Buy,
            AlertType = AlertType.PriceChange,
            Threshold = 1.0m
        };
        _mockAlertRepository.GetUserAlertCountAsync(alert.UserId).Returns(_appSettings.MaxAlertsPerUser);

        // Act
        Func<Task> action = async () => await _alertService.CreateAlertAsync(alert);

        // Assert
        await action.Should().ThrowAsync<InvalidAlertException>()
            .WithMessage($"Maximum number of alerts ({_appSettings.MaxAlertsPerUser}) reached");
        await _mockAlertRepository.DidNotReceive().AddAsync(Arg.Any<PriceAlert>());
    }

    [Fact]
    public async Task UpdateAlertAsync_ShouldReturnTrue_WhenAlertIsValidAndExists()
    {
        // Arrange
        var existingAlert = new PriceAlert
        {
            Id = 1,
            UserId = 1,
            Asset = "USDT",
            Fiat = "UAH",
            TradeType = TradeType.Buy,
            AlertType = AlertType.PriceChange,
            Threshold = 1.0m
        };
        _mockAlertRepository.UpdateAsync(Arg.Any<PriceAlert>()).Returns(true);

        // Act
        var result = await _alertService.UpdateAlertAsync(existingAlert);

        // Assert
        result.Should().BeTrue();
        await _mockAlertRepository.Received(1).UpdateAsync(Arg.Is<PriceAlert>(a => a.Id == existingAlert.Id));
    }

    [Fact]
    public async Task UpdateAlertAsync_ShouldReturnFalse_WhenAlertDoesNotExist()
    {
        // Arrange
        var nonExistentAlert = new PriceAlert
        {
            Id = 99,
            UserId = 1,
            Asset = "USDT",
            Fiat = "UAH",
            TradeType = TradeType.Buy,
            AlertType = AlertType.PriceChange,
            Threshold = 1.0m
        };
        _mockAlertRepository.UpdateAsync(Arg.Any<PriceAlert>()).Returns(false);

        // Act
        var result = await _alertService.UpdateAlertAsync(nonExistentAlert);

        // Assert
        result.Should().BeFalse();
        await _mockAlertRepository.Received(1).UpdateAsync(Arg.Is<PriceAlert>(a => a.Id == nonExistentAlert.Id));
    }

    [Fact]
    public async Task UpdateAlertAsync_ShouldThrowInvalidAlertException_WhenAlertIsInvalid()
    {
        // Arrange
        var invalidAlert = new PriceAlert { Id = 1, UserId = 1 }; // Missing required fields

        // Act
        Func<Task> action = async () => await _alertService.UpdateAlertAsync(invalidAlert);

        // Assert
        await action.Should().ThrowAsync<InvalidAlertException>()
            .WithMessage("Alert configuration is invalid");
        await _mockAlertRepository.DidNotReceive().UpdateAsync(Arg.Any<PriceAlert>());
    }

    [Fact]
    public async Task DeleteAlertAsync_ShouldReturnTrue_WhenAlertExists()
    {
        // Arrange
        var alertId = 1;
        _mockAlertRepository.DeleteAsync(alertId).Returns(true);

        // Act
        var result = await _alertService.DeleteAlertAsync(alertId);

        // Assert
        result.Should().BeTrue();
        await _mockAlertRepository.Received(1).DeleteAsync(alertId);
    }

    [Fact]
    public async Task GetUserAlertsAsync_ShouldReturnAlerts_WhenUserHasAlerts()
    {
        // Arrange
        var userId = 1;
        var alerts = new List<PriceAlert>
        {
            new PriceAlert { Id = 1, UserId = userId, Asset = "USDT", Fiat = "UAH", TradeType = TradeType.Buy, AlertType = AlertType.PriceChange, Threshold = 1.0m },
            new PriceAlert { Id = 2, UserId = userId, Asset = "BTC", Fiat = "USD", TradeType = TradeType.Sell, AlertType = AlertType.HighSpreadAlert, Threshold = 0.5m }
        };
        _mockAlertRepository.GetUserAlertsAsync(userId).Returns(alerts);

        // Act
        var result = await _alertService.GetUserAlertsAsync(userId);

        // Assert
        result.Should().BeEquivalentTo(alerts);
        await _mockAlertRepository.Received(1).GetUserAlertsAsync(userId);
    }
}
