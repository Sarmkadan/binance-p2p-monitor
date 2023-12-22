#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Exceptions;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using BinanceP2pMonitor.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace BinanceP2pMonitor.Tests;

public class PriceMonitoringServiceTests
{
    private readonly IPriceRepository _mockPriceRepository;
    private readonly IPriceHistoryService _mockPriceHistoryService;
    private readonly IAlertService _mockAlertService;
    private readonly AppSettings _appSettings;
    private readonly ILogger<PriceMonitoringService> _mockLogger;
    private readonly PriceMonitoringService _priceMonitoringService;

    public PriceMonitoringServiceTests()
    {
        _mockPriceRepository = Substitute.For<IPriceRepository>();
        _mockPriceHistoryService = Substitute.For<IPriceHistoryService>();
        _mockAlertService = Substitute.For<IAlertService>();
        _appSettings = new AppSettings();
        _mockLogger = Substitute.For<ILogger<PriceMonitoringService>>();
        _priceMonitoringService = new PriceMonitoringService(
            _mockPriceRepository,
            _mockPriceHistoryService,
            _mockAlertService,
            _appSettings,
            _mockLogger);
    }

    [Fact]
    public async Task GetCurrentPriceAsync_ShouldReturnPrice_WhenPriceExists()
    {
        // Arrange
        var asset = "USDT";
        var fiat = "UAH";
        var expectedPrice = new Price { Asset = asset, Fiat = fiat, BuyPrice = 38.0m, SellPrice = 38.5m };
        _mockPriceRepository.GetLatestByAssetAndFiatAsync(asset, fiat).Returns(expectedPrice);

        // Act
        var result = await _priceMonitoringService.GetCurrentPriceAsync(asset, fiat);

        // Assert
        result.Should().BeEquivalentTo(expectedPrice);
    }

    [Fact]
    public async Task GetCurrentPriceAsync_ShouldReturnNull_WhenPriceDoesNotExist()
    {
        // Arrange
        var asset = "USDT";
        var fiat = "UAH";
        _mockPriceRepository.GetLatestByAssetAndFiatAsync(asset, fiat).Returns((Price?)null);

        // Act
        var result = await _priceMonitoringService.GetCurrentPriceAsync(asset, fiat);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdatePriceAsync_ShouldAddPriceAndRecordHistoryAndCheckAlerts_WhenPriceIsValid()
    {
        // Arrange
        var price = new Price { Asset = "USDT", Fiat = "UAH", BuyPrice = 38.0m, SellPrice = 38.5m };
        _mockPriceRepository.AddAsync(Arg.Any<Price>()).Returns(1);
        _mockAlertService.CheckTriggersAsync(Arg.Any<Price>()).Returns(new List<PriceAlert>());

        // Act
        var result = await _priceMonitoringService.UpdatePriceAsync(price);

        // Assert
        result.Should().BeTrue();
        await _mockPriceRepository.Received(1).AddAsync(Arg.Is<Price>(p => p.Asset == price.Asset));
        await _mockPriceHistoryService.Received(1).RecordPriceAsync(Arg.Is<Price>(p => p.Asset == price.Asset));
        await _mockAlertService.Received(1).CheckTriggersAsync(Arg.Is<Price>(p => p.Asset == price.Asset));
    }

    [Fact]
    public async Task UpdatePriceAsync_ShouldThrowInvalidPriceException_WhenPriceIsInvalid()
    {
        // Arrange
        var invalidPrice = new Price { Asset = "USDT", BuyPrice = -1.0m }; // Invalid BuyPrice

        // Act
        Func<Task> action = async () => await _priceMonitoringService.UpdatePriceAsync(invalidPrice);

        // Assert
        await action.Should().ThrowAsync<InvalidPriceException>()
            .WithMessage("Invalid price data");
        await _mockPriceRepository.DidNotReceive().AddAsync(Arg.Any<Price>());
    }

    [Fact]
    public async Task GetAveragePriceAsync_ShouldReturnAveragePrice()
    {
        // Arrange
        var asset = "USDT";
        var fiat = "UAH";
        var expectedAverage = 38.25m;
        _mockPriceRepository.GetAveragePriceAsync(asset, fiat, 24).Returns(expectedAverage);

        // Act
        var result = await _priceMonitoringService.GetAveragePriceAsync(asset, fiat, 24);

        // Assert
        result.Should().Be(expectedAverage);
    }

    [Fact]
    public async Task GetPricesWithSignificantChangeAsync_ShouldReturnPricesMeetingThreshold()
    {
        // Arrange
        var prices = new List<Price>
        {
            new() { Asset = "USDT", Fiat = "UAH", BuyPrice = 38.0m, SellPrice = 38.5m, BuyChangePercent = 5.0m, SellChangePercent = 2.0m },
            new() { Asset = "BTC", Fiat = "UAH", BuyPrice = 60000m, SellPrice = 60500m, BuyChangePercent = 1.0m, SellChangePercent = 0.5m }
        };
        _mockPriceRepository.GetAllActiveAsync().Returns(prices);
        var changePercentThreshold = 3.0m;

        // Act
        var result = await _priceMonitoringService.GetPricesWithSignificantChangeAsync(changePercentThreshold);

        // Assert
        result.Should().ContainSingle(p => p.Asset == "USDT");
        result.Should().NotContain(p => p.Asset == "BTC");
    }

    [Theory]
    [InlineData("USDT", null)]
    [InlineData(null, "UAH")]
    [InlineData("", "UAH")]
    [InlineData("USDT", "")]
    public async Task GetSpreadAnalysisAsync_ShouldThrowArgumentException_WhenAssetOrFiatIsNullOrWhiteSpace(string asset, string fiat)
    {
        // Act
        Func<Task> action = async () => await _priceMonitoringService.GetSpreadAnalysisAsync(asset, fiat);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }
}
