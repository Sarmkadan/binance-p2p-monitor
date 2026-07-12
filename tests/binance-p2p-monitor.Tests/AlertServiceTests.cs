#nullable enable

using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Constants;
using BinanceP2pMonitor.Exceptions;
using BinanceP2pMonitor.Integration;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using BinanceP2pMonitor.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

/// <summary>
/// Tests for the AlertService class.
/// </summary>
public class AlertServiceTests
{
    private readonly IAlertRepository _mockAlertRepository;
    private readonly AppSettings _appSettings;
    private readonly ILogger<AlertService> _mockLogger;
    private readonly AlertService _alertService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlertServiceTests"/> class.
    /// </summary>
    public AlertServiceTests()
    {
        _mockAlertRepository = Substitute.For<IAlertRepository>();
        _appSettings = new AppSettings
        {
            MaxAlertsPerUser = 5,
            EnableTelegramNotifications = false,
            DatabaseConnectionString = "DataSource=:memory:"
        };
        _mockLogger = Substitute.For<ILogger<AlertService>>();

        var mockTelegram = Substitute.For<ITelegramNotificationClient>();
        var mockWebhook = Substitute.For<IWebhookNotificationClient>();

        _alertService = new AlertService(_mockAlertRepository, _appSettings, _mockLogger, mockTelegram, mockWebhook);
    }

    /// <summary>
    /// Creates a valid alert with the specified user ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>A valid alert.</returns>
    private static PriceAlert ValidAlert(int userId = 1) => new()
    {
        UserId = userId,
        Asset = "USDT",
        Fiat = "UAH",
        AlertType = AlertType.PriceChange,
        Condition = AlertCondition.GreaterThan,
        Threshold = 1.0m,
        IsEnabled = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    /// <summary>
    /// Tests that creating an alert with a valid configuration and not reaching the maximum number of alerts returns the alert ID.
    /// </summary>
    [Fact]
    public async Task CreateAlertAsync_ShouldReturnAlertId_WhenAlertIsValidAndMaxAlertsNotReached()
    {
        var alert = ValidAlert();
        _mockAlertRepository.GetUserAlertCountAsync(alert.UserId).Returns(2);
        _mockAlertRepository.AddAsync(Arg.Any<PriceAlert>()).Returns(1);

        var result = await _alertService.CreateAlertAsync(alert);

        result.Should().Be(1);
        await _mockAlertRepository.Received(1).AddAsync(Arg.Is<PriceAlert>(a => a.UserId == alert.UserId));
    }

    /// <summary>
    /// Tests that creating an alert with an invalid configuration throws an <see cref="InvalidAlertException"/>.
    /// </summary>
    [Fact]
    public async Task CreateAlertAsync_ShouldThrowInvalidAlertException_WhenAlertIsInvalid()
    {
        var invalidAlert = new PriceAlert { UserId = 1 };

        Func<Task> action = async () => await _alertService.CreateAlertAsync(invalidAlert);

        await action.Should().ThrowAsync<InvalidAlertException>()
            .WithMessage("Alert configuration is invalid");
        await _mockAlertRepository.DidNotReceive().AddAsync(Arg.Any<PriceAlert>());
    }

    /// <summary>
    /// Tests that creating an alert when the maximum number of alerts is reached throws an <see cref="InvalidAlertException"/>.
    /// </summary>
    [Fact]
    public async Task CreateAlertAsync_ShouldThrowInvalidAlertException_WhenMaxAlertsReached()
    {
        var alert = ValidAlert();
        _mockAlertRepository.GetUserAlertCountAsync(alert.UserId).Returns(_appSettings.MaxAlertsPerUser);

        Func<Task> action = async () => await _alertService.CreateAlertAsync(alert);

        await action.Should().ThrowAsync<InvalidAlertException>()
            .WithMessage($"Maximum number of alerts ({_appSettings.MaxAlertsPerUser}) reached");
        await _mockAlertRepository.DidNotReceive().AddAsync(Arg.Any<PriceAlert>());
    }

    /// <summary>
    /// Tests that updating an alert with a valid configuration and existing alert returns true.
    /// </summary>
    [Fact]
    public async Task UpdateAlertAsync_ShouldReturnTrue_WhenAlertIsValidAndExists()
    {
        var alert = ValidAlert();
        alert.Id = 1;
        _mockAlertRepository.UpdateAsync(Arg.Any<PriceAlert>()).Returns(true);

        var result = await _alertService.UpdateAlertAsync(alert);

        result.Should().BeTrue();
        await _mockAlertRepository.Received(1).UpdateAsync(Arg.Is<PriceAlert>(a => a.Id == alert.Id));
    }

    /// <summary>
    /// Tests that updating an alert that does not exist returns false.
    /// </summary>
    [Fact]
    public async Task UpdateAlertAsync_ShouldReturnFalse_WhenAlertDoesNotExist()
    {
        var alert = ValidAlert();
        alert.Id = 99;
        _mockAlertRepository.UpdateAsync(Arg.Any<PriceAlert>()).Returns(false);

        var result = await _alertService.UpdateAlertAsync(alert);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that deleting an alert returns true when the alert exists.
    /// </summary>
    [Fact]
    public async Task DeleteAlertAsync_ShouldReturnTrue_WhenAlertExists()
    {
        _mockAlertRepository.DeleteAsync(1).Returns(true);

        var result = await _alertService.DeleteAlertAsync(1);

        result.Should().BeTrue();
        await _mockAlertRepository.Received(1).DeleteAsync(1);
    }

    /// <summary>
    /// Tests that getting user alerts returns the alerts when the user has alerts.
    /// </summary>
    [Fact]
    public async Task GetUserAlertsAsync_ShouldReturnAlerts_WhenUserHasAlerts()
    {
        var alerts = new List<PriceAlert>
        {
            ValidAlert(1),
            ValidAlert(1)
        };
        alerts[0].Id = 1;
        alerts[1].Id = 2;
        alerts[1].Asset = "BTC";
        alerts[1].Fiat = "USD";

        _mockAlertRepository.GetUserAlertsAsync(1).Returns(alerts);

        var result = await _alertService.GetUserAlertsAsync(1);

        result.Should().BeEquivalentTo(alerts);
    }
}
