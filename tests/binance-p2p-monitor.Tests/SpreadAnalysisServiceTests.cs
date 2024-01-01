#nullable enable

using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Exceptions;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using BinanceP2pMonitor.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace BinanceP2pMonitor.Tests;

public class SpreadAnalysisServiceTests
{
    private readonly IPriceRepository _mockPriceRepository;
    private readonly IPriceHistoryService _mockPriceHistoryService;
    private readonly AppSettings _appSettings;
    private readonly ILogger<SpreadAnalysisService> _mockLogger;
    private readonly SpreadAnalysisService _spreadAnalysisService;

    public SpreadAnalysisServiceTests()
    {
        _mockPriceRepository = Substitute.For<IPriceRepository>();
        _mockPriceHistoryService = Substitute.For<IPriceHistoryService>();
        _appSettings = new AppSettings
        {
            SpreadAnalysisHistoryHours = 24,
            DefaultSpreadThreshold = 0.3m
        };
        _mockLogger = Substitute.For<ILogger<SpreadAnalysisService>>();
        _spreadAnalysisService = new SpreadAnalysisService(
            _mockPriceRepository,
            _mockPriceHistoryService,
            _appSettings,
            _mockLogger);
    }

    private static List<Price> GetSamplePriceHistory(string asset, string fiat)
    {
        return new List<Price>
        {
            new Price { Asset = asset, Fiat = fiat, BuyPrice = 100, SellPrice = 101, RecordedAt = DateTime.UtcNow.AddHours(-5) }, // 1% spread
            new Price { Asset = asset, Fiat = fiat, BuyPrice = 100, SellPrice = 102, RecordedAt = DateTime.UtcNow.AddHours(-4) }, // 2% spread
            new Price { Asset = asset, Fiat = fiat, BuyPrice = 100, SellPrice = 103, RecordedAt = DateTime.UtcNow.AddHours(-3) }, // 3% spread
        };
    }

    [Fact]
    public async Task GetSpreadAnalysisAsync_ShouldInitializeFromHistory_WhenCacheEmptyAndHistoryExists()
    {
        // Arrange
        var asset = "BTC";
        var fiat = "USD";
        var historicalPrices = GetSamplePriceHistory(asset, fiat);
        _mockPriceHistoryService.GetHistoryAsync(asset, fiat, _appSettings.SpreadAnalysisHistoryHours)
            .Returns(historicalPrices);
        _mockPriceRepository.GetLatestByAssetAndFiatAsync(asset, fiat)
            .Returns(new Price { Asset = asset, Fiat = fiat, BuyPrice = 100, SellPrice = 104 }); // 4% current spread

        // Act
        var result = await _spreadAnalysisService.GetSpreadAnalysisAsync(asset, fiat);

        // Assert
        result.Should().NotBeNull();
        result!.Asset.Should().Be(asset);
        result.Fiat.Should().Be(fiat);
        result.MinSpreadPercent.Should().Be(1m);
        result.MaxSpreadPercent.Should().Be(3m);
        result.AverageSpreadPercent.Should().Be(2m);
        result.CurrentSpreadPercent.Should().Be(4m);
        result.SampleCount.Should().Be(historicalPrices.Count);
        result.StandardDeviation.Should().BeApproximately(0.816m, 0.001m); // Std dev of (1, 2, 3)
    }

    [Fact]
    public async Task GetSpreadAnalysisAsync_ShouldInitializeWithZeroSampleCount_WhenNoHistory()
    {
        // Arrange
        var asset = "BTC";
        var fiat = "USD";
        _mockPriceHistoryService.GetHistoryAsync(asset, fiat, _appSettings.SpreadAnalysisHistoryHours)
            .Returns(new List<Price>());
        _mockPriceRepository.GetLatestByAssetAndFiatAsync(asset, fiat)
            .ReturnsNull(); // No latest price either

        // Act
        var result = await _spreadAnalysisService.GetSpreadAnalysisAsync(asset, fiat);

        // Assert
        result.Should().NotBeNull();
        result!.SampleCount.Should().Be(0);
        result.CurrentSpreadPercent.Should().Be(0m); // Default value as no current price
    }
    
    [Fact]
    public async Task GetSpreadAnalysisAsync_ShouldUpdateStatistics_WhenCacheExistsAndNewPriceAvailable()
    {
        // Arrange - first call to populate cache
        var asset = "BTC";
        var fiat = "USD";
        var historicalPrices = GetSamplePriceHistory(asset, fiat);
        _mockPriceHistoryService.GetHistoryAsync(asset, fiat, _appSettings.SpreadAnalysisHistoryHours)
            .Returns(historicalPrices);
        _mockPriceRepository.GetLatestByAssetAndFiatAsync(asset, fiat)
            .Returns(new Price { Asset = asset, Fiat = fiat, BuyPrice = 100, SellPrice = 101 }); // Initial 1% spread

        await _spreadAnalysisService.GetSpreadAnalysisAsync(asset, fiat); // Populate cache

        // Arrange - second call with updated latest price
        _mockPriceRepository.GetLatestByAssetAndFiatAsync(asset, fiat)
            .Returns(new Price { Asset = asset, Fiat = fiat, BuyPrice = 100, SellPrice = 105 }); // New 5% spread

        // Act
        var result = await _spreadAnalysisService.GetSpreadAnalysisAsync(asset, fiat);

        // Assert
        result.Should().NotBeNull();
        result!.CurrentSpreadPercent.Should().Be(5m); // Should reflect the latest price
        // The statistics should now include 5% as well
        result.MinSpreadPercent.Should().Be(1m);
        result.MaxSpreadPercent.Should().Be(5m);
        result.SampleCount.Should().Be(historicalPrices.Count + 1); // Historical + current
    }

    [Fact]
    public async Task GetSpreadAnalysisAsync_ShouldReturnExistingSpread_WhenLatestPriceIsNull()
    {
        // Arrange
        var asset = "BTC";
        var fiat = "USD";
        var historicalPrices = GetSamplePriceHistory(asset, fiat);
        _mockPriceHistoryService.GetHistoryAsync(asset, fiat, _appSettings.SpreadAnalysisHistoryHours)
            .Returns(historicalPrices);
        _mockPriceRepository.GetLatestByAssetAndFiatAsync(asset, fiat)
            .Returns(new Price { Asset = asset, Fiat = fiat, BuyPrice = 100, SellPrice = 103 });

        var initialSpread = await _spreadAnalysisService.GetSpreadAnalysisAsync(asset, fiat);

        // Arrange - clear latest price
        _mockPriceRepository.GetLatestByAssetAndFiatAsync(asset, fiat)
            .ReturnsNull();

        // Act
        var result = await _spreadAnalysisService.GetSpreadAnalysisAsync(asset, fiat);

        // Assert
        result.Should().Be(initialSpread); // Should return the previously cached spread
        result!.CurrentSpreadPercent.Should().Be(3m); // Current spread remains as per initial latest price
    }

    [Fact]
    public async Task GetSpreadAnalysisAsync_ShouldThrowException_WhenRepositoryThrows()
    {
        // Arrange
        _mockPriceRepository.GetLatestByAssetAndFiatAsync(Arg.Any<string>(), Arg.Any<string>())
            .ThrowsAsync(new InvalidOperationException("DB error"));

        // Act
        Func<Task> action = async () => await _spreadAnalysisService.GetSpreadAnalysisAsync("BTC", "USD");

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("DB error");
        _mockLogger.Received(1).LogError(
            Arg.Any<Exception>(),
            "Error analyzing spread for {Asset}/{Fiat}", "BTC", "USD");
    }

    [Fact]
    public async Task GetTopSpreadOpportunitiesAsync_ShouldFilterAndSortByThreshold()
    {
        // Arrange
        var spread1 = new Spread { Asset = "BTC", Fiat = "USD", CurrentSpreadPercent = 1.0m }; // Above default
        var spread2 = new Spread { Asset = "ETH", Fiat = "USD", CurrentSpreadPercent = 0.2m }; // Below default
        var spread3 = new Spread { Asset = "BNB", Fiat = "USD", CurrentSpreadPercent = 1.5m }; // Above default
        var spread4 = new Spread { Asset = "BTC", Fiat = "EUR", CurrentSpreadPercent = 0.5m }; // Below override (0.6)
        var spread5 = new Spread { Asset = "ETH", Fiat = "EUR", CurrentSpreadPercent = 0.9m }; // Equal to override (0.9) - should pass

        _mockPriceRepository.GetAllActiveAsync().Returns(new List<Price>
        {
            new Price { Asset = "BTC", Fiat = "USD", BuyPrice = 100, SellPrice = 101 },
            new Price { Asset = "ETH", Fiat = "USD", BuyPrice = 100, SellPrice = 100.2m },
            new Price { Asset = "BNB", Fiat = "USD", BuyPrice = 100, SellPrice = 101.5m },
            new Price { Asset = "BTC", Fiat = "EUR", BuyPrice = 100, SellPrice = 100.5m },
            new Price { Asset = "ETH", Fiat = "EUR", BuyPrice = 100, SellPrice = 100.9m },
        });

        _mockPriceHistoryService.GetHistoryAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(new List<Price>()); // No history needed for this test scenario

        // Need to simulate caching for GetAllSpreadsAsync to work
        await _spreadAnalysisService.GetSpreadAnalysisAsync(spread1.Asset, spread1.Fiat);
        await _spreadAnalysisService.GetSpreadAnalysisAsync(spread2.Asset, spread2.Fiat);
        await _spreadAnalysisService.GetSpreadAnalysisAsync(spread3.Asset, spread3.Fiat);
        await _spreadAnalysisService.GetSpreadAnalysisAsync(spread4.Asset, spread4.Fiat);
        await _spreadAnalysisService.GetSpreadAnalysisAsync(spread5.Asset, spread5.Fiat);

        // Act
        var result = await _spreadAnalysisService.GetTopSpreadOpportunitiesAsync(5);

        // Assert
        result.Should().HaveCount(3);
        result.First().Asset.Should().Be("BNB"); // 1.5m
        result.Skip(1).First().Asset.Should().Be("BTC"); // 1.0m
        result.Skip(2).First().Asset.Should().Be("ETH"); // 0.9m
    }
    
    [Fact]
    public async Task AnalyzeSpreadAsync_ShouldReturnCorrectSpread()
    {
        // Act
        var result = await _spreadAnalysisService.AnalyzeSpreadAsync(100, 105);

        // Assert
        result.Should().Be(5.0000m);
    }

    [Fact]
    public async Task AnalyzeSpreadAsync_ShouldHandleZeroBuyPrice()
    {
        // Act
        Func<Task> action = async () => await _spreadAnalysisService.AnalyzeSpreadAsync(0, 105);

        // Assert
        await action.Should().ThrowAsync<InvalidPriceException>()
            .WithMessage("Buy price must be positive");
    }

    [Fact]
    public async Task AnalyzeSpreadAsync_ShouldHandleNegativeBuyPrice()
    {
        // Act
        Func<Task> action = async () => await _spreadAnalysisService.AnalyzeSpreadAsync(-10, 105);

        // Assert
        await action.Should().ThrowAsync<InvalidPriceException>()
            .WithMessage("Buy price must be positive");
    }
    
    [Fact]
    public async Task UpdateSpreadAsync_ShouldUpdateCacheSuccessfully()
    {
        // Arrange
        var spread = new Spread { Asset = "TEST", Fiat = "COIN", CurrentSpreadPercent = 1.23m };
        spread.IsValid().Returns(true); // Mock IsValid for the spread object

        // Act
        var result = await _spreadAnalysisService.UpdateSpreadAsync(spread);

        // Assert
        result.Should().BeTrue();
        // Verify that the spread is in the cache (indirectly, by trying to retrieve it)
        _mockPriceRepository.GetLatestByAssetAndFiatAsync(spread.Asset, spread.Fiat)
            .Returns(new Price { Asset = spread.Asset, Fiat = spread.Fiat, BuyPrice = 100, SellPrice = 101.23m });
        var cachedSpread = await _spreadAnalysisService.GetSpreadAnalysisAsync(spread.Asset, spread.Fiat);
        cachedSpread!.CurrentSpreadPercent.Should().Be(spread.CurrentSpreadPercent);
    }
    
    [Fact]
    public async Task UpdateSpreadAsync_ShouldThrowException_WhenSpreadIsNull()
    {
        // Act
        Func<Task> action = async () => await _spreadAnalysisService.UpdateSpreadAsync(null!);

        // Assert
        await action.Should().ThrowAsync<InvalidPriceException>()
            .WithMessage("Spread data is invalid");
    }

    [Fact]
    public async Task UpdateSpreadAsync_ShouldThrowException_WhenSpreadIsInvalid()
    {
        // Arrange
        var invalidSpread = new Spread { Asset = "INV", Fiat = "LID", CurrentSpreadPercent = -5m }; // Example of invalid spread
        // We cannot directly mock IsValid() on the concrete Spread object as it's not an interface.
        // For unit testing this path, we would either rely on the actual IsValid() implementation
        // or ensure the test setup leads to IsValid() returning false.
        // For now, let's assume -5m makes IsValid() return false if implemented.

        // Act
        Func<Task> action = async () => await _spreadAnalysisService.UpdateSpreadAsync(invalidSpread);

        // Assert
        // This assertion assumes Spread.IsValid() would return false for CurrentSpreadPercent = -5m.
        // If IsValid() logic changes, this test might need adjustment or direct mocking of IsValid() if Spread were an interface.
        await action.Should().ThrowAsync<InvalidPriceException>()
            .WithMessage("Spread data is invalid");
    }

    [Fact]
    public async Task GetAllSpreadsAsync_ShouldReturnAllSpreads()
    {
        // Arrange
        var prices = new List<Price>
        {
            new Price { Asset = "BTC", Fiat = "USD", BuyPrice = 100, SellPrice = 101 },
            new Price { Asset = "ETH", Fiat = "USD", BuyPrice = 200, SellPrice = 203 },
        };
        _mockPriceRepository.GetAllActiveAsync().Returns(prices);
        _mockPriceHistoryService.GetHistoryAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(new List<Price>()); // No history for simplicity

        // Act
        var result = await _spreadAnalysisService.GetAllSpreadsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainKey("BTC/USD");
        result.Should().ContainKey("ETH/USD");
        result["BTC/USD"].CurrentSpreadPercent.Should().Be(1m);
        result["ETH/USD"].CurrentSpreadPercent.Should().Be(1.5m);
    }
    
    [Fact]
    public async Task GetCrossCurrencySpreadAsync_ShouldReturnCorrectSpread()
    {
        // Arrange
        var asset = "BTC";
        var baseFiat = "USD";
        var quoteFiat = "EUR";
        var conversionRate = 0.9m; // 1 USD = 0.9 EUR

        _mockPriceRepository.GetLatestByAssetAndFiatAsync(asset, baseFiat)
            .Returns(new Price { Asset = asset, Fiat = baseFiat, BuyPrice = 10000, SellPrice = 10100 }); // 10000 USD to buy BTC
        _mockPriceRepository.GetLatestByAssetAndFiatAsync(asset, quoteFiat)
            .Returns(new Price { Asset = asset, Fiat = quoteFiat, BuyPrice = 9000, SellPrice = 9050 }); // 9050 EUR to sell BTC

        // Act
        var result = await _spreadAnalysisService.GetCrossCurrencySpreadAsync(asset, baseFiat, quoteFiat, conversionRate);

        // Assert
        result.Should().NotBeNull();
        result!.Asset.Should().Be(asset);
        result.BaseFiat.Should().Be(baseFiat);
        result.QuoteFiat.Should().Be(quoteFiat);
        result.ConversionRate.Should().Be(conversionRate);
        result.BuyPriceInBaseFiat.Should().Be(10000);
        // SellPriceInBaseFiat = quotePrice.SellPrice (9050 EUR) / conversionRate (0.9 EUR/USD) -> 9050 / 0.9 = 10055.555... USD
        result.SellPriceInBaseFiat.Should().BeApproximately(10055.5556m, 0.0001m);
        // Spread = ((SellPriceInBaseFiat - BuyPriceInBaseFiat) / BuyPriceInBaseFiat) * 100
        // ((10055.5556 - 10000) / 10000) * 100 = 0.55556%
        result.SpreadPercent.Should().BeApproximately(0.5556m, 0.0001m);
    }

    [Theory]
    [InlineData("BTC", "", "EUR", 0.9)]
    [InlineData("", "USD", "EUR", 0.9)]
    [InlineData("BTC", "USD", "", 0.9)]
    public async Task GetCrossCurrencySpreadAsync_ShouldThrowArgumentException_WhenFiatOrAssetIsEmpty(
        string asset, string baseFiat, string quoteFiat, decimal conversionRate)
    {
        // Act
        Func<Task> action = async () => await _spreadAnalysisService.GetCrossCurrencySpreadAsync(asset, baseFiat, quoteFiat, conversionRate);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetCrossCurrencySpreadAsync_ShouldThrowArgumentException_WhenConversionRateIsZero()
    {
        // Act
        Func<Task> action = async () => await _spreadAnalysisService.GetCrossCurrencySpreadAsync("BTC", "USD", "EUR", 0);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Conversion rate must be positive*");
    }

    [Fact]
    public async Task GetCrossCurrencySpreadAsync_ShouldReturnNull_WhenBasePriceIsMissing()
    {
        // Arrange
        _mockPriceRepository.GetLatestByAssetAndFiatAsync("BTC", "USD").ReturnsNull();
        _mockPriceRepository.GetLatestByAssetAndFiatAsync("BTC", "EUR")
            .Returns(new Price { Asset = "BTC", Fiat = "EUR", BuyPrice = 9000, SellPrice = 9050 });

        // Act
        var result = await _spreadAnalysisService.GetCrossCurrencySpreadAsync("BTC", "USD", "EUR", 0.9m);

        // Assert
        result.Should().BeNull();
        _mockLogger.Received(1).LogWarning(
            "Cross-currency spread unavailable: missing price data for {Asset}/{BaseFiat} or {Asset}/{QuoteFiat}",
            "BTC", "USD", "BTC", "EUR");
    }

    [Fact]
    public async Task FindAnomalousSpreadAsync_ShouldReturnEmpty_WhenNoSpreads()
    {
        // Arrange
        _mockPriceRepository.GetAllActiveAsync().Returns(new List<Price>()); // No active prices, so no spreads generated

        // Act
        var result = await _spreadAnalysisService.FindAnomalousSpreadAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindAnomalousSpreadAsync_ShouldReturnAnomalies_WhenSpreadsExceedZScoreThreshold()
    {
        // Arrange
        var prices = new List<Price>
        {
            new Price { Asset = "BTC", Fiat = "USD", BuyPrice = 100, SellPrice = 101 }, // 1%
            new Price { Asset = "ETH", Fiat = "USD", BuyPrice = 100, SellPrice = 102 }, // 2%
            new Price { Asset = "BNB", Fiat = "USD", BuyPrice = 100, SellPrice = 103 }, // 3%
            new Price { Asset = "XRP", Fiat = "USD", BuyPrice = 100, SellPrice = 106 }, // 6% - Anomaly
            new Price { Asset = "LTC", Fiat = "USD", BuyPrice = 100, SellPrice = 100.5m }, // 0.5%
        };
        _mockPriceRepository.GetAllActiveAsync().Returns(prices);
        _mockPriceHistoryService.GetHistoryAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(new List<Price>()); // No history for simplicity

        // Simulate filling the cache for GetAllSpreadsAsync
        foreach (var price in prices)
        {
            await _spreadAnalysisService.GetSpreadAnalysisAsync(price.Asset, price.Fiat);
        }

        // Act
        var result = await _spreadAnalysisService.FindAnomalousSpreadAsync(2.0m); // Default threshold

        // Assert
        result.Should().ContainSingle();
        result.First().Asset.Should().Be("XRP");
        result.First().Spread.Should().Be(6m);
    }

    [Fact]
    public async Task FindAnomalousSpreadAsync_ShouldReturnEmpty_WhenNoAnomalies()
    {
        // Arrange
        var prices = new List<Price>
        {
            new Price { Asset = "BTC", Fiat = "USD", BuyPrice = 100, SellPrice = 101 }, // 1%
            new Price { Asset = "ETH", Fiat = "USD", BuyPrice = 100, SellPrice = 102 }, // 2%
            new Price { Asset = "BNB", Fiat = "USD", BuyPrice = 100, SellPrice = 103 }, // 3%
        };
        _mockPriceRepository.GetAllActiveAsync().Returns(prices);
        _mockPriceHistoryService.GetHistoryAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(new List<Price>()); // No history for simplicity

        // Simulate filling the cache for GetAllSpreadsAsync
        foreach (var price in prices)
        {
            await _spreadAnalysisService.GetSpreadAnalysisAsync(price.Asset, price.Fiat);
        }

        // Act
        var result = await _spreadAnalysisService.FindAnomalousSpreadAsync(2.0m);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindAnomalousSpreadAsync_ShouldReturnEmpty_WhenStdDevIsZero()
    {
        // Arrange
        var prices = new List<Price>
        {
            new Price { Asset = "BTC", Fiat = "USD", BuyPrice = 100, SellPrice = 101 }, // 1%
            new Price { Asset = "ETH", Fiat = "USD", BuyPrice = 100, SellPrice = 101 }, // 1%
        };
        _mockPriceRepository.GetAllActiveAsync().Returns(prices);
        _mockPriceHistoryService.GetHistoryAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(new List<Price>()); // No history for simplicity

        // Simulate filling the cache for GetAllSpreadsAsync
        foreach (var price in prices)
        {
            await _spreadAnalysisService.GetSpreadAnalysisAsync(price.Asset, price.Fiat);
        }

        // Act
        var result = await _spreadAnalysisService.FindAnomalousSpreadAsync(2.0m);

        // Assert
        result.Should().BeEmpty(); // When std dev is 0, z-score is 0, so no anomalies
    }

    [Fact]
    public async Task FindAnomalousSpreadAsync_ShouldThrowException_WhenGetAllSpreadsThrows()
    {
        // Arrange
        _mockPriceRepository.GetAllActiveAsync().ThrowsAsync(new InvalidOperationException("DB connection failed"));

        // Act
        Func<Task> action = async () => await _spreadAnalysisService.FindAnomalousSpreadAsync();

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("DB connection failed");
        _mockLogger.Received(1).LogError(
            Arg.Any<Exception>(),
            "Error finding anomalous spreads");
    }
}