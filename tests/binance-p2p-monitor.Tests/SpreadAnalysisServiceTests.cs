using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Exceptions;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using BinanceP2pMonitor.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Tests for the SpreadAnalysisService class.
/// </summary>
public class SpreadAnalysisServiceTests
{
    private readonly Mock<IPriceRepository> _priceRepositoryMock = new();
    private readonly Mock<IPriceHistoryService> _historyServiceMock = new();
    private readonly AppSettings _settings = new() { DefaultSpreadThreshold = 1.0m, SpreadAnalysisHistoryHours = 24 };
    private readonly Mock<ILogger<SpreadAnalysisService>> _loggerMock = new();
    private readonly SpreadAnalysisService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpreadAnalysisServiceTests"/> class.
    /// </summary>
    public SpreadAnalysisServiceTests()
    {
        _service = new SpreadAnalysisService(
            _priceRepositoryMock.Object,
            _historyServiceMock.Object,
            _settings,
            _loggerMock.Object);
    }

    /// <summary>
    /// Verifies that the AnalyzeSpreadAsync method returns the correct spread when given valid prices.
    /// </summary>
    [Fact]
    public async Task AnalyzeSpreadAsync_ValidPrices_ReturnsCorrectSpread()
    {
        var result = await _service.AnalyzeSpreadAsync(100m, 102m);
        result.Should().Be(2.0m);
    }

    /// <summary>
    /// Verifies that the AnalyzeSpreadAsync method throws an InvalidPriceException when given a zero buy price.
    /// </summary>
    [Fact]
    public async Task AnalyzeSpreadAsync_ZeroBuyPrice_ThrowsInvalidPriceException()
    {
        Func<Task> act = () => _service.AnalyzeSpreadAsync(0m, 102m).AsTask();
        await act.Should().ThrowAsync<InvalidPriceException>();
    }

    /// <summary>
    /// Verifies that the UpdateSpreadAsync method returns true when given a valid spread.
    /// </summary>
    [Fact]
    public async Task UpdateSpreadAsync_ValidSpread_ReturnsTrue()
    {
        var spread = new Spread
        {
            Asset = "BTC",
            Fiat = "USD",
            CurrentSpreadPercent = 0.5m,
            AverageSpreadPercent = 0.5m,
            MinSpreadPercent = 0.4m,
            MaxSpreadPercent = 0.6m,
            SampleCount = 1,
            LastUpdatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        var result = await _service.UpdateSpreadAsync(spread);
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that the UpdateSpreadAsync method throws an InvalidPriceException when given an invalid spread.
    /// </summary>
    [Fact]
    public async Task UpdateSpreadAsync_InvalidSpread_ThrowsInvalidPriceException()
    {
        var spread = new Spread { Asset = "", Fiat = "USD" }; // Invalid (empty asset)
        Func<Task> act = () => _service.UpdateSpreadAsync(spread).AsTask();
        await act.Should().ThrowAsync<InvalidPriceException>();
    }

    /// <summary>
    /// Verifies that the GetCrossCurrencySpreadAsync method returns the spread when given valid data.
    /// </summary>
    [Fact]
    public async Task GetCrossCurrencySpreadAsync_ValidData_ReturnsSpread()
    {
        var asset = "BTC";
        var baseFiat = "USD";
        var quoteFiat = "EUR";
        var conversionRate = 0.9m;

        _priceRepositoryMock.Setup(r => r.GetLatestByAssetAndFiatAsync(asset, baseFiat))
            .ReturnsAsync(new Price { Asset = asset, Fiat = baseFiat, BuyPrice = 100m });
        _priceRepositoryMock.Setup(r => r.GetLatestByAssetAndFiatAsync(asset, quoteFiat))
            .ReturnsAsync(new Price { Asset = asset, Fiat = quoteFiat, SellPrice = 120m });

        var result = await _service.GetCrossCurrencySpreadAsync(asset, baseFiat, quoteFiat, conversionRate);

        // sellPriceConverted = 120 * 0.9 = 108
        // spread = (108 - 100) / 100 * 100 = 8%
        result.Should().NotBeNull();
        result!.SpreadPercent.Should().Be(8.0m);
    }

    /// <summary>
    /// Verifies that the GetCrossCurrencySpreadAsync method returns null when given missing data.
    /// </summary>
    [Fact]
    public async Task GetCrossCurrencySpreadAsync_MissingData_ReturnsNull()
    {
        var asset = "BTC";
        var baseFiat = "USD";
        var quoteFiat = "EUR";
        var conversionRate = 0.9m;

        _priceRepositoryMock.Setup(r => r.GetLatestByAssetAndFiatAsync(asset, baseFiat))
            .ReturnsAsync((Price?)null);

        var result = await _service.GetCrossCurrencySpreadAsync(asset, baseFiat, quoteFiat, conversionRate);

        result.Should().BeNull();
    }
}
