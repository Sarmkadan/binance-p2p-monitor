#nullable enable
using BinanceP2pMonitor.Configuration;
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
}
