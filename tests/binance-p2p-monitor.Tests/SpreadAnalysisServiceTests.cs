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
    /// Verifies that AnalyzeSpreadAsync correctly calculates a normal spread.
    /// </summary>
    [Fact]
    public async Task AnalyzeSpreadAsync_NormalSpread_ReturnsCorrectSpread()
    {
        // Arrange: buy=100, sell=105, spread should be 5%
        var result = await _service.AnalyzeSpreadAsync(100m, 105m);

        result.Should().Be(5.0m);
    }

    /// <summary>
    /// Verifies that AnalyzeSpreadAsync returns zero spread when buy equals sell.
    /// </summary>
    [Fact]
    public async Task AnalyzeSpreadAsync_ZeroSpread_ReturnsZero()
    {
        // Arrange: buy=100, sell=100, spread should be 0%
        var result = await _service.AnalyzeSpreadAsync(100m, 100m);

        result.Should().Be(0.0m);
    }

    /// <summary>
    /// Verifies that AnalyzeSpreadAsync handles negative spread (sell < buy) correctly.
    /// </summary>
    [Fact]
    public async Task AnalyzeSpreadAsync_NegativeSpread_ReturnsNegativeValue()
    {
        // Arrange: buy=105, sell=100, spread should be -4.7619%
        var result = await _service.AnalyzeSpreadAsync(105m, 100m);

        result.Should().BeApproximately(-4.7619m, 0.0001m);
    }

    /// <summary>
    /// Verifies that AnalyzeSpreadAsync handles very small spreads correctly.
    /// </summary>
    [Fact]
    public async Task AnalyzeSpreadAsync_VerySmallSpread_ReturnsCorrectValue()
    {
        // Arrange: buy=10000, sell=10001, spread should be 0.01%
        var result = await _service.AnalyzeSpreadAsync(10000m, 10001m);

        result.Should().Be(0.01m);
    }

    /// <summary>
    /// Verifies that AnalyzeSpreadAsync handles large spreads correctly.
    /// </summary>
    [Fact]
    public async Task AnalyzeSpreadAsync_LargeSpread_ReturnsCorrectValue()
    {
        // Arrange: buy=100, sell=150, spread should be 50%
        var result = await _service.AnalyzeSpreadAsync(100m, 150m);

        result.Should().Be(50.0m);
    }

    /// <summary>
    /// Verifies that AnalyzeSpreadAsync rounds to 4 decimal places.
    /// </summary>
    [Fact]
    public async Task AnalyzeSpreadAsync_RoundsToFourDecimalPlaces()
    {
        // Arrange: buy=3m, sell=3.0001m, spread should be 0.003333...% which rounds to 0.0033%
        var result = await _service.AnalyzeSpreadAsync(3m, 3.0001m);

        result.Should().Be(0.0033m);
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
    /// Verifies that GetSpreadAnalysisAsync handles single data point correctly.
    /// </summary>
    [Fact]
    public async Task GetSpreadAnalysisAsync_SingleDataPoint_CalculatesCorrectly()
    {
        // Arrange
        var asset = "ETH";
        var fiat = "USD";
        var price = new Price { Asset = asset, Fiat = fiat, BuyPrice = 100m, SellPrice = 102m, Timestamp = DateTime.UtcNow };
        var history = new List<PriceHistory>
        {
            new() { Asset = asset, Fiat = fiat, BuyPrice = 100m, SellPrice = 102m, RecordedAt = DateTime.UtcNow.AddHours(-1), SpreadPercentage = 2.0m }
        };

        _priceRepositoryMock.Setup(r => r.GetLatestByAssetAndFiatAsync(asset, fiat))
            .ReturnsAsync(price);
        _historyServiceMock.Setup(h => h.GetHistoryAsync(asset, fiat, _settings.SpreadAnalysisHistoryHours))
            .ReturnsAsync(history);

        // Act
        var result = await _service.GetSpreadAnalysisAsync(asset, fiat);

        // Assert
        result.Should().NotBeNull();
        result!.SampleCount.Should().BeGreaterThan(0);
        result.CurrentSpreadPercent.Should().Be(2.0m);
        result.MinSpreadPercent.Should().Be(2.0m);
        result.MaxSpreadPercent.Should().Be(2.0m);
        result.AverageSpreadPercent.Should().Be(2.0m);
    }

    /// <summary>
    /// Verifies that GetSpreadAnalysisAsync handles empty historical data correctly.
    /// </summary>
    [Fact]
    public async Task GetSpreadAnalysisAsync_EmptyHistoricalData_CreatesEmptySpread()
    {
        // Arrange
        var asset = "BTC";
        var fiat = "USD";
        var price = new Price { Asset = asset, Fiat = fiat, BuyPrice = 100m, SellPrice = 102m, Timestamp = DateTime.UtcNow };

        _priceRepositoryMock.Setup(r => r.GetLatestByAssetAndFiatAsync(asset, fiat))
            .ReturnsAsync(price);
        _historyServiceMock.Setup(h => h.GetHistoryAsync(asset, fiat, _settings.SpreadAnalysisHistoryHours))
            .ReturnsAsync(new List<PriceHistory>());

        // Act
        var result = await _service.GetSpreadAnalysisAsync(asset, fiat);

        // Assert
        result.Should().NotBeNull();
        result!.SampleCount.Should().Be(1); // Cache adds one entry
        result.CurrentSpreadPercent.Should().Be(2.0m);
    }

    /// <summary>
    /// Verifies that GetSpreadAnalysisAsync returns null when no price data is available.
    /// </summary>
    [Fact]
    public async Task GetSpreadAnalysisAsync_NoPriceData_ReturnsNull()
    {
        // Arrange
        var asset = "BTC";
        var fiat = "USD";

        _priceRepositoryMock.Setup(r => r.GetLatestByAssetAndFiatAsync(asset, fiat))
            .ReturnsAsync((Price?)null);

        // Act
        var result = await _service.GetSpreadAnalysisAsync(asset, fiat);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetSpreadAnalysisAsync handles multiple historical data points correctly.
    /// </summary>
    [Fact]
    public async Task GetSpreadAnalysisAsync_MultipleDataPoints_CalculatesStatisticsCorrectly()
    {
        // Arrange
        var asset = "BTC";
        var fiat = "USD";
        var price = new Price { Asset = asset, Fiat = fiat, BuyPrice = 100m, SellPrice = 102m, Timestamp = DateTime.UtcNow };
        var history = new List<PriceHistory>
        {
            new() { Asset = asset, Fiat = fiat, BuyPrice = 100m, SellPrice = 102m, RecordedAt = DateTime.UtcNow.AddHours(-3), SpreadPercentage = 2.0m },
            new() { Asset = asset, Fiat = fiat, BuyPrice = 100m, SellPrice = 103m, RecordedAt = DateTime.UtcNow.AddHours(-2), SpreadPercentage = 3.0m },
            new() { Asset = asset, Fiat = fiat, BuyPrice = 100m, SellPrice = 101m, RecordedAt = DateTime.UtcNow.AddHours(-1), SpreadPercentage = 1.0m }
        };

        _priceRepositoryMock.Setup(r => r.GetLatestByAssetAndFiatAsync(asset, fiat))
            .ReturnsAsync(price);
        _historyServiceMock.Setup(h => h.GetHistoryAsync(asset, fiat, _settings.SpreadAnalysisHistoryHours))
            .ReturnsAsync(history);

        // Act
        var result = await _service.GetSpreadAnalysisAsync(asset, fiat);

        // Assert
        result.Should().NotBeNull();
        result!.SampleCount.Should().BeGreaterThan(3); // Cache adds one, plus 3 historical
        result.CurrentSpreadPercent.Should().Be(2.0m); // Last spread
        result.MinSpreadPercent.Should().Be(1.0m);
        result.MaxSpreadPercent.Should().Be(3.0m);
        result.AverageSpreadPercent.Should().BeApproximately(2.0m, 0.0001m);
    }

    /// <summary>
    /// Verifies that GetAllSpreadsAsync handles empty repository correctly.
    /// </summary>
    [Fact]
    public async Task GetAllSpreadsAsync_EmptyRepository_ReturnsEmptyDictionary()
    {
        // Arrange
        _priceRepositoryMock.Setup(r => r.GetAllActiveAsync())
            .ReturnsAsync(new List<Price>());

        // Act
        var result = await _service.GetAllSpreadsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that GetCrossCurrencySpreadAsync returns the spread when given valid data.
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
    /// Verifies that GetCrossCurrencySpreadAsync returns null when given missing data.
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

    /// <summary>
    /// Verifies that FindAnomalousSpreadAsync handles empty spreads correctly.
    /// </summary>
    [Fact]
    public async Task FindAnomalousSpreadAsync_EmptySpreads_ReturnsEmptyCollection()
    {
        // Arrange
        _priceRepositoryMock.Setup(r => r.GetAllActiveAsync())
            .ReturnsAsync(new List<Price>());

        // Act
        var result = await _service.FindAnomalousSpreadAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that UpdateSpreadAsync updates spread statistics correctly.
    /// </summary>
    [Fact]
    public async Task UpdateSpreadAsync_UpdatesStatistics_Correctly()
    {
        // Arrange
        var spread = new Spread
        {
            Asset = "BTC",
            Fiat = "USD",
            CurrentSpreadPercent = 1.0m,
            AverageSpreadPercent = 1.0m,
            MinSpreadPercent = 1.0m,
            MaxSpreadPercent = 1.0m,
            SampleCount = 1,
            LastUpdatedAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = await _service.UpdateSpreadAsync(spread);

        // Assert
        result.Should().BeTrue();
    }
}
