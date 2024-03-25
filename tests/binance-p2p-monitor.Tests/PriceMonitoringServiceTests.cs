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

/// <summary>
/// Tests for the PriceMonitoringService class.
/// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="PriceMonitoringServiceTests"/> class.
    /// </summary>
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

    /// <summary>
    /// Verifies that the <see cref="PriceMonitoringService.GetCurrentPriceAsync(string, string)"/> method returns a price when it exists.
    /// </summary>
    /// <param name="asset">The asset to get the price for.</param>
    /// <param name="fiat">The fiat to get the price for.</param>
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

    /// <summary>
    /// Verifies that the <see cref="PriceMonitoringService.GetCurrentPriceAsync(string, string)"/> method returns null when the price does not exist.
    /// </summary>
    /// <param name="asset">The asset to get the price for.</param>
    /// <param name="fiat">The fiat to get the price for.</param>
    [Fact]
    public async Task GetCurrentPriceAsync_ShouldReturnNull_WhenPriceDoesNotExist()
    {
        var asset = "USDT";
        var fiat = "UAH";
        _mockPriceRepository.GetLatestByAssetAndFiatAsync(asset, fiat).Returns((Price?)null);

        var result = await _priceMonitoringService.GetCurrentPriceAsync(asset, fiat);

        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that the <see cref="PriceMonitoringService.UpdatePriceAsync(Price)"/> method adds the price and records history and checks alerts when the price is valid.
    /// </summary>
    /// <param name="price">The price to update.</param>
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

    /// <summary>
    /// Verifies that the <see cref="PriceMonitoringService.UpdatePriceAsync(Price)"/> method throws an <see cref="ArgumentException"/> when the price is invalid.
    /// </summary>
    /// <param name="invalidPrice">The invalid price to update.</param>
    [Fact]
    public async Task UpdatePriceAsync_ShouldThrowArgumentException_WhenPriceIsInvalid()
    {
        var invalidPrice = new Price { Asset = "USDT", BuyPrice = -1.0m };

        Func<Task> action = async () => await _priceMonitoringService.UpdatePriceAsync(invalidPrice);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Price must have valid asset and fiat");
        await _mockPriceRepository.DidNotReceive().AddAsync(Arg.Any<Price>());
    }

    /// <summary>
    /// Verifies that the <see cref="PriceMonitoringService.GetAveragePriceAsync(string, string, int)"/> method returns the average price.
    /// </summary>
    /// <param name="asset">The asset to get the average price for.</param>
    /// <param name="fiat">The fiat to get the average price for.</param>
    /// <param name="hours">The number of hours to average over.</param>
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

    /// <summary>
    /// Verifies that the <see cref="PriceMonitoringService.GetPricesWithSignificantChangeAsync(decimal)"/> method returns prices meeting the threshold.
    /// </summary>
    /// <param name="threshold">The threshold to check for significant change.</param>
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
