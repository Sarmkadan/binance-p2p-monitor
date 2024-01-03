#nullable enable

using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Exceptions;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using BinanceP2pMonitor.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
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
            DefaultSpreadThreshold = 0.3m,
            DatabaseConnectionString = "DataSource=:memory:"
        };
        _mockLogger = Substitute.For<ILogger<SpreadAnalysisService>>();
        _spreadAnalysisService = new SpreadAnalysisService(
            _mockPriceRepository,
            _mockPriceHistoryService,
            _appSettings,
            _mockLogger);
    }

    private static List<PriceHistory> GetSampleHistory(string asset, string fiat) =>
        new()
        {
            new PriceHistory { Asset = asset, Fiat = fiat, BuyPrice = 100, SellPrice = 101, RecordedAt = DateTime.UtcNow.AddHours(-5) }, // 1%
            new PriceHistory { Asset = asset, Fiat = fiat, BuyPrice = 100, SellPrice = 102, RecordedAt = DateTime.UtcNow.AddHours(-4) }, // 2%
            new PriceHistory { Asset = asset, Fiat = fiat, BuyPrice = 100, SellPrice = 103, RecordedAt = DateTime.UtcNow.AddHours(-3) }, // 3%
        };

    [Fact]
    public async Task GetSpreadAnalysisAsync_ShouldInitializeFromHistory_WhenCacheEmptyAndHistoryExists()
    {
        var asset = "BTC";
        var fiat = "USD";
        var history = GetSampleHistory(asset, fiat);
        _mockPriceHistoryService.GetHistoryAsync(asset, fiat, _appSettings.SpreadAnalysisHistoryHours)
            .Returns(history);
        _mockPriceRepository.GetLatestByAssetAndFiatAsync(asset, fiat)
            .Returns(new Price { Asset = asset, Fiat = fiat, BuyPrice = 100, SellPrice = 104 }); // 4% current

        var result = await _spreadAnalysisService.GetSpreadAnalysisAsync(asset, fiat);

        result.Should().NotBeNull();
        result!.Asset.Should().Be(asset);
        result.Fiat.Should().Be(fiat);
        result.MinSpreadPercent.Should().Be(1m);
        result.MaxSpreadPercent.Should().Be(4m); // includes current
        result.CurrentSpreadPercent.Should().Be(4m);
    }

    [Fact]
    public async Task GetSpreadAnalysisAsync_ShouldReturnNullOrEmptySpread_WhenNoHistoryAndNoCurrentPrice()
    {
        var asset = "BTC";
        var fiat = "USD";
        _mockPriceHistoryService.GetHistoryAsync(asset, fiat, _appSettings.SpreadAnalysisHistoryHours)
            .Returns(Enumerable.Empty<PriceHistory>());
        _mockPriceRepository.GetLatestByAssetAndFiatAsync(asset, fiat)
            .ReturnsNull();

        var result = await _spreadAnalysisService.GetSpreadAnalysisAsync(asset, fiat);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSpreadAnalysisAsync_ShouldReturnExistingSpread_WhenLatestPriceIsNull()
    {
        var asset = "BTC";
        var fiat = "USD";
        var history = GetSampleHistory(asset, fiat);
        _mockPriceHistoryService.GetHistoryAsync(asset, fiat, _appSettings.SpreadAnalysisHistoryHours)
            .Returns(history);
        _mockPriceRepository.GetLatestByAssetAndFiatAsync(asset, fiat)
            .Returns(new Price { Asset = asset, Fiat = fiat, BuyPrice = 100, SellPrice = 103 });

        var initialSpread = await _spreadAnalysisService.GetSpreadAnalysisAsync(asset, fiat);
        _mockPriceRepository.GetLatestByAssetAndFiatAsync(asset, fiat).ReturnsNull();

        var result = await _spreadAnalysisService.GetSpreadAnalysisAsync(asset, fiat);

        result.Should().Be(initialSpread);
    }

    [Fact]
    public async Task GetSpreadAnalysisAsync_ShouldThrowException_WhenRepositoryThrows()
    {
        _mockPriceRepository.GetLatestByAssetAndFiatAsync(Arg.Any<string>(), Arg.Any<string>())
            .ThrowsAsync(new InvalidOperationException("DB error"));
        _mockPriceHistoryService.GetHistoryAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Enumerable.Empty<PriceHistory>());

        Func<Task> action = async () => await _spreadAnalysisService.GetSpreadAnalysisAsync("BTC", "USD");

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("DB error");
    }

    [Fact]
    public async Task AnalyzeSpreadAsync_ShouldReturnCorrectSpread()
    {
        var result = await _spreadAnalysisService.AnalyzeSpreadAsync(100, 105);

        result.Should().Be(5.0000m);
    }

    [Fact]
    public async Task AnalyzeSpreadAsync_ShouldThrowInvalidPriceException_WhenBuyPriceIsZero()
    {
        Func<Task> action = async () => await _spreadAnalysisService.AnalyzeSpreadAsync(0, 105).AsTask();

        await action.Should().ThrowAsync<InvalidPriceException>()
            .WithMessage("Buy price must be positive");
    }

    [Fact]
    public async Task UpdateSpreadAsync_ShouldReturnTrue_WhenSpreadIsValid()
    {
        var spread = new Spread
        {
            Asset = "BTC",
            Fiat = "USD",
            CurrentSpreadPercent = 1.5m,
            AverageSpreadPercent = 1.0m,
            MinSpreadPercent = 0.5m,
            MaxSpreadPercent = 2.0m,
            SampleCount = 10,
            LastUpdatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var result = await _spreadAnalysisService.UpdateSpreadAsync(spread).AsTask();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateSpreadAsync_ShouldThrowException_WhenSpreadIsNull()
    {
        Func<Task> action = async () => await _spreadAnalysisService.UpdateSpreadAsync(null!).AsTask();

        await action.Should().ThrowAsync<InvalidPriceException>()
            .WithMessage("Spread data is invalid");
    }

    [Fact]
    public async Task GetCrossCurrencySpreadAsync_ShouldReturnNull_WhenBasePriceIsMissing()
    {
        _mockPriceRepository.GetLatestByAssetAndFiatAsync("BTC", "USD").ReturnsNull();
        _mockPriceRepository.GetLatestByAssetAndFiatAsync("BTC", "EUR")
            .Returns(new Price { Asset = "BTC", Fiat = "EUR", BuyPrice = 9000, SellPrice = 9050 });

        var result = await _spreadAnalysisService.GetCrossCurrencySpreadAsync("BTC", "USD", "EUR", 0.9m);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCrossCurrencySpreadAsync_ShouldThrowArgumentException_WhenConversionRateIsZero()
    {
        Func<Task> action = async () =>
            await _spreadAnalysisService.GetCrossCurrencySpreadAsync("BTC", "USD", "EUR", 0);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Conversion rate must be positive*");
    }

    [Theory]
    [InlineData("BTC", "", "EUR", 0.9)]
    [InlineData("", "USD", "EUR", 0.9)]
    [InlineData("BTC", "USD", "", 0.9)]
    public async Task GetCrossCurrencySpreadAsync_ShouldThrowArgumentException_WhenParametersAreEmpty(
        string asset, string baseFiat, string quoteFiat, double conversionRate)
    {
        Func<Task> action = async () =>
            await _spreadAnalysisService.GetCrossCurrencySpreadAsync(asset, baseFiat, quoteFiat, (decimal)conversionRate);

        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task FindAnomalousSpreadAsync_ShouldReturnEmpty_WhenNoSpreads()
    {
        _mockPriceRepository.GetAllActiveAsync().Returns(Enumerable.Empty<Price>());

        var result = await _spreadAnalysisService.FindAnomalousSpreadAsync();

        result.Should().BeEmpty();
    }
}
