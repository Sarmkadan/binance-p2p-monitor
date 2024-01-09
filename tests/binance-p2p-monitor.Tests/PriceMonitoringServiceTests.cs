#nullable enable

using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Events;
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
    private readonly ISpreadAnalysisService _mockSpreadAnalysisService;
    private readonly IEventBus _mockEventBus;
    private readonly IWebSocketService _mockWebSocketService;
    private readonly AppSettings _appSettings;
    private readonly ILogger<PriceMonitoringService> _mockLogger;
    private readonly PriceMonitoringService _priceMonitoringService;

    public PriceMonitoringServiceTests()
    {
        _mockPriceRepository = Substitute.For<IPriceRepository>();
        _mockPriceHistoryService = Substitute.For<IPriceHistoryService>();
        _mockAlertService = Substitute.For<IAlertService>();
        _mockSpreadAnalysisService = Substitute.For<ISpreadAnalysisService>();
        _mockEventBus = Substitute.For<IEventBus>();
        _mockWebSocketService = Substitute.For<IWebSocketService>();
        _appSettings = new AppSettings
        {
            DatabaseConnectionString = "DataSource=:memory:",
            EnableWebSocket = false
        };
        _mockLogger = Substitute.For<ILogger<PriceMonitoringService>>();
        _priceMonitoringService = new PriceMonitoringService(
            _mockPriceRepository,
            _mockPriceHistoryService,
            _mockAlertService,
            _mockSpreadAnalysisService,
            _mockEventBus,
            _mockWebSocketService,
            _appSettings,
            _mockLogger);
    }

    [Fact]
    public async Task GetCurrentPriceAsync_ShouldReturnPrice_WhenPriceExists()
    {
        var asset = "USDT";
        var fiat = "UAH";
        var expectedPrice = new Price { Asset = asset, Fiat = fiat, BuyPrice = 38.0m, SellPrice = 38.5m };
        _mockPriceRepository.GetLatestByAssetAndFiatAsync(asset, fiat).Returns(expectedPrice);

        var result = await _priceMonitoringService.GetCurrentPriceAsync(asset, fiat);

        result.Should().BeEquivalentTo(expectedPrice);
    }

    [Fact]
    public async Task GetCurrentPriceAsync_ShouldReturnNull_WhenPriceDoesNotExist()
    {
        var asset = "USDT";
        var fiat = "UAH";
        _mockPriceRepository.GetLatestByAssetAndFiatAsync(asset, fiat).Returns((Price?)null);

        var result = await _priceMonitoringService.GetCurrentPriceAsync(asset, fiat);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdatePriceAsync_ShouldAddPriceAndRecordHistoryAndCheckAlerts_WhenPriceIsValid()
    {
        var price = new Price
        {
            Asset = "USDT",
            Fiat = "UAH",
            BuyPrice = 38.0m,
            SellPrice = 38.5m,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _mockPriceRepository.AddAsync(Arg.Any<Price>()).Returns(1);
        _mockAlertService.CheckTriggersAsync(Arg.Any<Price>()).Returns(new List<PriceAlert>());

        var result = await _priceMonitoringService.UpdatePriceAsync(price);

        result.Should().BeTrue();
        await _mockPriceRepository.Received(1).AddAsync(Arg.Is<Price>(p => p.Asset == price.Asset));
        await _mockPriceHistoryService.Received(1).RecordPriceAsync(Arg.Is<Price>(p => p.Asset == price.Asset));
        await _mockAlertService.Received(1).CheckTriggersAsync(Arg.Is<Price>(p => p.Asset == price.Asset));
    }

    [Fact]
    public async Task UpdatePriceAsync_ShouldThrowArgumentException_WhenPriceIsInvalid()
    {
        var invalidPrice = new Price { Asset = "USDT", BuyPrice = -1.0m };

        Func<Task> action = async () => await _priceMonitoringService.UpdatePriceAsync(invalidPrice);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Price must have valid asset and fiat");
        await _mockPriceRepository.DidNotReceive().AddAsync(Arg.Any<Price>());
    }

    [Fact]
    public async Task GetAveragePriceAsync_ShouldReturnAveragePrice()
    {
        var asset = "USDT";
        var fiat = "UAH";
        var expectedAverage = 38.25m;
        _mockPriceRepository.GetAveragePriceAsync(asset, fiat, 24).Returns(expectedAverage);

        var result = await _priceMonitoringService.GetAveragePriceAsync(asset, fiat, 24);

        result.Should().Be(expectedAverage);
    }

    [Fact]
    public async Task GetPricesWithSignificantChangeAsync_ShouldReturnPricesMeetingThreshold()
    {
        var prices = new List<Price>
        {
            new() { Asset = "USDT", Fiat = "UAH", BuyPrice = 38.0m, SellPrice = 38.5m, BuyChangePercent = 5.0m, SellChangePercent = 2.0m },
            new() { Asset = "BTC", Fiat = "UAH", BuyPrice = 60000m, SellPrice = 60500m, BuyChangePercent = 1.0m, SellChangePercent = 0.5m }
        };
        _mockPriceRepository.GetAllActiveAsync().Returns(prices);

        var result = await _priceMonitoringService.GetPricesWithSignificantChangeAsync(3.0m);

        result.Should().ContainSingle(p => p.Asset == "USDT");
        result.Should().NotContain(p => p.Asset == "BTC");
    }
}
