#nullable enable
using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Exceptions;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using BinanceP2pMonitor.Services;
using Microsoft.Extensions.Logging;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Contains unit tests for the <see cref="PriceHistoryService"/> class.
/// </summary>
public class PriceHistoryServiceTests
{
    private readonly Mock<IHistoryRepository> _repoMock = new();
    private readonly AppSettings _settings = new() { DatabaseConnectionString = "Data Source=:memory:" };
    private readonly ILogger<PriceHistoryService> _logger = Mock.Of<ILogger<PriceHistoryService>>();

    /// <summary>
    /// Creates a new instance of <see cref="PriceHistoryService"/> with mocked dependencies.
    /// </summary>
    /// <returns>A new <see cref="PriceHistoryService"/> instance.</returns>
    private PriceHistoryService CreateService() =>
        new(_repoMock.Object, _settings, _logger);

    /// <summary>
    /// Verifies that <see cref="PriceHistoryService.GetPriceTrendAsync"/> returns a positive trend when prices have increased between two records.
    /// </summary>
    [Fact]
    public async Task GetPriceTrendAsync_TwoRecordsWithRisingPrice_ReturnsPositiveTrend()
    {
        var earlier = new PriceHistory
        {
            Asset = "BTC", Fiat = "USD",
            BuyPrice = 40000m, SellPrice = 40100m,
            RecordedAt = DateTime.UtcNow.AddHours(-2), CreatedAt = DateTime.UtcNow
        };
        var later = new PriceHistory
        {
            Asset = "BTC", Fiat = "USD",
            BuyPrice = 44000m, SellPrice = 44100m,
            RecordedAt = DateTime.UtcNow.AddHours(-1), CreatedAt = DateTime.UtcNow
        };

        _repoMock
            .Setup(r => r.GetHistoryByAssetAndFiatAsync("BTC", "USD", 24))
            .ReturnsAsync(new[] { earlier, later });

        var service = CreateService();
        var trend = await service.GetPriceTrendAsync("BTC", "USD", 24).ConfigureAwait(false);

        trend.Should().BePositive();
    }

    /// <summary>
    /// Verifies that <see cref="PriceHistoryService.GetPriceTrendAsync"/> returns 0 when there is only a single record.
    /// </summary>
    [Fact]
    public async Task GetPriceTrendAsync_SingleRecord_ReturnsZero()
    {
        var single = new PriceHistory
        {
            Asset = "BTC", Fiat = "USD",
            BuyPrice = 50000m, SellPrice = 50100m,
            RecordedAt = DateTime.UtcNow.AddHours(-1), CreatedAt = DateTime.UtcNow
        };

        _repoMock
            .Setup(r => r.GetHistoryByAssetAndFiatAsync("BTC", "USD", 24))
            .ReturnsAsync(new[] { single });

        var service = CreateService();
        var trend = await service.GetPriceTrendAsync("BTC", "USD", 24).ConfigureAwait(false);

        trend.Should().Be(0m);
    }

    /// <summary>
    /// Verifies that <see cref="PriceHistoryService.GetPriceTrendAsync"/> returns 0 when the history is empty.
    /// </summary>
    [Fact]
    public async Task GetPriceTrendAsync_EmptyHistory_ReturnsZero()
    {
        _repoMock
            .Setup(r => r.GetHistoryByAssetAndFiatAsync("BTC", "USD", 24))
            .ReturnsAsync(Array.Empty<PriceHistory>());

        var service = CreateService();
        var trend = await service.GetPriceTrendAsync("BTC", "USD", 24).ConfigureAwait(false);

        trend.Should().Be(0m);
    }

    /// <summary>
    /// Verifies that <see cref="PriceHistoryService.GetPriceStatsAsync"/> returns all zeros when the history is empty.
    /// </summary>
    [Fact]
    public async Task GetPriceStatsAsync_EmptyHistory_ReturnsAllZeroTuple()
    {
        _repoMock
            .Setup(r => r.GetHistoryByAssetAndFiatAsync("BTC", "USD", 24))
            .ReturnsAsync(Array.Empty<PriceHistory>());

        var service = CreateService();
        var (high, low, avg) = await service.GetPriceStatsAsync("BTC", "USD", 24).ConfigureAwait(false);

        high.Should().Be(0m);
        low.Should().Be(0m);
        avg.Should().Be(0m);
    }

    /// <summary>
    /// Verifies that <see cref="PriceHistoryService.GetPriceStatsAsync"/> calculates the correct high and low prices from multiple records.
    /// </summary>
    [Fact]
    public async Task GetPriceStatsAsync_MultipleRecords_ReturnsCorrectHighLowAverage()
    {
        var records = new[]
        {
            new PriceHistory { Asset = "BTC", Fiat = "USD", BuyPrice = 40000m, SellPrice = 41000m,
                RecordedAt = DateTime.UtcNow.AddHours(-3), CreatedAt = DateTime.UtcNow },
            new PriceHistory { Asset = "BTC", Fiat = "USD", BuyPrice = 50000m, SellPrice = 51000m,
                RecordedAt = DateTime.UtcNow.AddHours(-1), CreatedAt = DateTime.UtcNow }
        };

        _repoMock
            .Setup(r => r.GetHistoryByAssetAndFiatAsync("BTC", "USD", 24))
            .ReturnsAsync(records);

        var service = CreateService();
        var (high, low, _) = await service.GetPriceStatsAsync("BTC", "USD", 24).ConfigureAwait(false);

        high.Should().Be(51000m);
        low.Should().Be(40000m);
    }

    /// <summary>
    /// Verifies that <see cref="PriceHistoryService.CleanupOldHistoryAsync"/> correctly delegates to the repository and returns the repository's result.
    /// </summary>
    [Fact]
    public async Task CleanupOldHistoryAsync_DelegatesToRepository_ReturnsRepoResult()
    {
        _repoMock
            .Setup(r => r.DeleteOldRecordsAsync(30))
            .ReturnsAsync(true);

        var service = CreateService();
        var result = await service.CleanupOldHistoryAsync(daysOld: 30).ConfigureAwait(false);

        result.Should().BeTrue();
        _repoMock.Verify(r => r.DeleteOldRecordsAsync(30), Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="PriceHistoryService.GetHistoryCountAsync"/> correctly delegates to the repository to return the total count.
    /// </summary>
    [Fact]
    public async Task GetHistoryCountAsync_DelegatesToRepository_ReturnsTotalCount()
    {
        _repoMock
            .Setup(r => r.GetTotalHistoryCountAsync())
            .ReturnsAsync(42L);

        var service = CreateService();
        var count = await service.GetHistoryCountAsync().ConfigureAwait(false);

        count.Should().Be(42L);
        _repoMock.Verify(r => r.GetTotalHistoryCountAsync(), Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="PriceHistoryService"/> constructor throws <see cref="ArgumentNullException"/> when the repository is null.
    /// </summary>
    [Fact]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        var act = () => new PriceHistoryService(null!, _settings, _logger);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("historyRepository");
    }

    /// <summary>
    /// Verifies that <see cref="PriceHistoryService"/> constructor throws <see cref="ArgumentNullException"/> when settings are null.
    /// </summary>
    [Fact]
    public void Constructor_NullSettings_ThrowsArgumentNullException()
    {
        var act = () => new PriceHistoryService(_repoMock.Object, null!, _logger);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("settings");
    }

    /// <summary>
    /// Verifies that <see cref="PriceHistoryService.RecordPriceAsync"/> throws <see cref="InvalidPriceException"/> when price is null.
    /// </summary>
    [Fact]
    public async Task RecordPriceAsync_NullPrice_ThrowsInvalidPriceException()
    {
        var service = CreateService();

        await service.Invoking(s => s.RecordPriceAsync(null!))
            .Should().ThrowAsync<InvalidPriceException>()
            .WithMessage("Price data is invalid for recording");
    }

    /// <summary>
    /// Verifies that <see cref="PriceHistoryService.RecordPriceAsync"/> throws <see cref="InvalidPriceException"/> when price is invalid.
    /// </summary>
    [Fact]
    public async Task RecordPriceAsync_InvalidPrice_ThrowsInvalidPriceException()
    {
        var invalidPrice = new Price
        {
            Asset = "BTC",
            Fiat = "USD",
            BuyPrice = 0, // Invalid price
            SellPrice = 50000,
            BuyChangePercent = 0,
            SellChangePercent = 0,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var service = CreateService();

        await service.Invoking(s => s.RecordPriceAsync(invalidPrice))
            .Should().ThrowAsync<InvalidPriceException>()
            .WithMessage("Price data is invalid for recording");
    }

    /// <summary>
    /// Verifies that <see cref="PriceHistoryService.RecordPriceAsync"/> correctly records a valid price and returns the repository result.
    /// </summary>
    [Fact]
    public async Task RecordPriceAsync_ValidPrice_RecordsAndReturnsResult()
    {
        var validPrice = new Price
        {
            Id = 1,
            Asset = "BTC",
            Fiat = "USD",
            BuyPrice = 50000m,
            SellPrice = 50100m,
            BuyChangePercent = 0.5m,
            SellChangePercent = 0.3m,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<PriceHistory>()))
            .ReturnsAsync(1);

        var service = CreateService();
        var result = await service.RecordPriceAsync(validPrice).ConfigureAwait(false);

        result.Should().Be(1);
        _repoMock.Verify(r => r.AddAsync(It.Is<PriceHistory>(ph =>
            ph.Asset == "BTC" &&
            ph.Fiat == "USD" &&
            ph.BuyPrice == 50000m &&
            ph.SellPrice == 50100m &&
            ph.SpreadPercentage == 0.2m && // (50100-50000)/50000*100
            ph.PriceChangePercent == 0.5m
        )), Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="PriceHistoryService.RecordPriceAsync"/> handles repository exceptions by wrapping them in <see cref="DataAccessException"/>.
    /// </summary>
    [Fact]
    public async Task RecordPriceAsync_RepositoryThrows_WrapsInDataAccessException()
    {
        var validPrice = new Price
        {
            Id = 1,
            Asset = "BTC",
            Fiat = "USD",
            BuyPrice = 50000m,
            SellPrice = 50100m,
            BuyChangePercent = 0.5m,
            SellChangePercent = 0.3m,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<PriceHistory>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var service = CreateService();

        await service.Invoking(s => s.RecordPriceAsync(validPrice))
            .Should().ThrowAsync<DataAccessException>()
            .WithMessage("Failed to record price history");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<PriceHistory>()), Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="PriceHistoryService.GetHistoryAsync"/> returns empty collection when no history exists.
    /// </summary>
    [Fact]
    public async Task GetHistoryAsync_NoHistory_ReturnsEmptyCollection()
    {
        _repoMock
            .Setup(r => r.GetHistoryByAssetAndFiatAsync("BTC", "USD", 24))
            .ReturnsAsync(Array.Empty<PriceHistory>());

        var service = CreateService();
        var result = await service.GetHistoryAsync("BTC", "USD", 24).ConfigureAwait(false);

        result.Should().BeEmpty();
        _repoMock.Verify(r => r.GetHistoryByAssetAndFiatAsync("BTC", "USD", 24), Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="PriceHistoryService.GetHistoryAsync"/> correctly retrieves history for a trading pair.
    /// </summary>
    [Fact]
    public async Task GetHistoryAsync_WithHistory_ReturnsHistoryCollection()
    {
        var history = new[]
        {
            new PriceHistory { Asset = "BTC", Fiat = "USD", BuyPrice = 50000m, SellPrice = 50100m, RecordedAt = DateTime.UtcNow.AddHours(-2), CreatedAt = DateTime.UtcNow },
            new PriceHistory { Asset = "BTC", Fiat = "USD", BuyPrice = 51000m, SellPrice = 51100m, RecordedAt = DateTime.UtcNow.AddHours(-1), CreatedAt = DateTime.UtcNow }
        };

        _repoMock
            .Setup(r => r.GetHistoryByAssetAndFiatAsync("BTC", "USD", 24))
            .ReturnsAsync(history);

        var service = CreateService();
        var result = await service.GetHistoryAsync("BTC", "USD", 24).ConfigureAwait(false);

        result.Should().HaveCount(2);
        result.Should().ContainInOrder(history);
        _repoMock.Verify(r => r.GetHistoryByAssetAndFiatAsync("BTC", "USD", 24), Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="PriceHistoryService.GetHistoryAsync"/> handles repository exceptions by wrapping them in <see cref="DataAccessException"/>.
    /// </summary>
    [Fact]
    public async Task GetHistoryAsync_RepositoryThrows_WrapsInDataAccessException()
    {
        _repoMock
            .Setup(r => r.GetHistoryByAssetAndFiatAsync("BTC", "USD", 24))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var service = CreateService();

        await service.Invoking(s => s.GetHistoryAsync("BTC", "USD", 24))
            .Should().ThrowAsync<DataAccessException>()
            .WithMessage("Failed to retrieve price history");

        _repoMock.Verify(r => r.GetHistoryByAssetAndFiatAsync("BTC", "USD", 24), Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="PriceHistoryService.GetDetailedAnalysisAsync"/> returns correct analysis when history exists.
    /// </summary>
    [Fact]
    public async Task GetDetailedAnalysisAsync_WithHistory_ReturnsCorrectAnalysis()
    {
        var history = new[]
        {
            new PriceHistory { Asset = "BTC", Fiat = "USD", BuyPrice = 50000m, SellPrice = 50100m, RecordedAt = DateTime.UtcNow.AddHours(-2), CreatedAt = DateTime.UtcNow, SpreadPercentage = 0.2m },
            new PriceHistory { Asset = "BTC", Fiat = "USD", BuyPrice = 51000m, SellPrice = 51100m, RecordedAt = DateTime.UtcNow.AddHours(-1), CreatedAt = DateTime.UtcNow, SpreadPercentage = 0.2m }
        };

        _repoMock
            .Setup(r => r.GetHistoryByAssetAndFiatAsync("BTC", "USD", 24))
            .ReturnsAsync(history);

        var service = CreateService();
        var result = await service.GetDetailedAnalysisAsync("BTC", "USD", 24).ConfigureAwait(false);

        result.Should().ContainKey("Asset").WhoseValue.Should().Be("BTC");
        result.Should().ContainKey("Fiat").WhoseValue.Should().Be("USD");
        result.Should().ContainKey("HighPrice").WhoseValue.Should().Be(51100m);
        result.Should().ContainKey("LowPrice").WhoseValue.Should().Be(50000m);
        result.Should().ContainKey("AveragePrice").WhoseValue.As<decimal>().Should().Be(50550m);
        result.Should().ContainKey("RecordCount").WhoseValue.Should().Be(2);
        result.Should().ContainKey("TimeSpanHours").WhoseValue.Should().Be(24);
        result.Should().ContainKey("HighestSpread").WhoseValue.Should().Be(0.2m);
        result.Should().ContainKey("LowestSpread").WhoseValue.Should().Be(0.2m);
        result.Should().ContainKey("AverageSpread").WhoseValue.Should().Be(0.2m);
        _repoMock.Verify(r => r.GetHistoryByAssetAndFiatAsync("BTC", "USD", 24), Times.AtLeastOnce); // Called by GetHistoryAsync, GetPriceStatsAsync, and GetPriceTrendAsync
    }

    /// <summary>
    /// Verifies that <see cref="PriceHistoryService.GetDetailedAnalysisAsync"/> returns correct analysis when no history exists.
    /// </summary>
    [Fact]
    public async Task GetDetailedAnalysisAsync_NoHistory_ReturnsZeroValues()
    {
        _repoMock
            .Setup(r => r.GetHistoryByAssetAndFiatAsync("BTC", "USD", 24))
            .ReturnsAsync(Array.Empty<PriceHistory>());

        var service = CreateService();
        var result = await service.GetDetailedAnalysisAsync("BTC", "USD", 24).ConfigureAwait(false);

        result.Should().ContainKey("Asset").WhoseValue.Should().Be("BTC");
        result.Should().ContainKey("Fiat").WhoseValue.Should().Be("USD");
        result.Should().ContainKey("HighPrice").WhoseValue.Should().Be(0m);
        result.Should().ContainKey("LowPrice").WhoseValue.Should().Be(0m);
        result.Should().ContainKey("AveragePrice").WhoseValue.Should().Be(0m);
        result.Should().ContainKey("RecordCount").WhoseValue.Should().Be(0);
        result.Should().ContainKey("HighestSpread").WhoseValue.Should().Be(0m);
        result.Should().ContainKey("LowestSpread").WhoseValue.Should().Be(0m);
        result.Should().ContainKey("AverageSpread").WhoseValue.Should().Be(0m);
        _repoMock.Verify(r => r.GetHistoryByAssetAndFiatAsync("BTC", "USD", 24), Times.AtLeastOnce); // Called by GetHistoryAsync, GetPriceStatsAsync, and GetPriceTrendAsync
    }

    /// <summary>
    /// Verifies that <see cref="PriceHistoryService.GetDetailedAnalysisAsync"/> propagates wrapped DataAccessException from GetHistoryAsync.
    /// </summary>
    [Fact]
    public async Task GetDetailedAnalysisAsync_RepositoryThrows_WrapsInDataAccessException()
    {
        _repoMock
            .Setup(r => r.GetHistoryByAssetAndFiatAsync("BTC", "USD", 24))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var service = CreateService();

        await service.Invoking(s => s.GetDetailedAnalysisAsync("BTC", "USD", 24))
            .Should().ThrowAsync<DataAccessException>()
            .WithMessage("Failed to retrieve price history");

        _repoMock.Verify(r => r.GetHistoryByAssetAndFiatAsync("BTC", "USD", 24), Times.AtLeastOnce);
    }
}
