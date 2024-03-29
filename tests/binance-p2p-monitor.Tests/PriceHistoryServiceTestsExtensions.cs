#nullable enable
using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using BinanceP2pMonitor.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace BinanceP2pMonitor.Tests;

public static class PriceHistoryServiceTestsExtensions
{
    /// <summary>
    /// Creates a mock PriceHistoryService with default dependencies for testing
    /// </summary>
    /// <param name="_">The test fixture instance (unused parameter).</param>
    /// <returns>A new <see cref="PriceHistoryService"/> instance with mocked dependencies.</returns>
    /// <exception cref="ArgumentNullException">Thrown if any dependency is null.</exception>
    public static PriceHistoryService CreateDefaultService(this PriceHistoryServiceTests _) =>
        new(
            Mock.Of<IHistoryRepository>(),
            new AppSettings { DatabaseConnectionString = "Data Source=:memory:" },
            Mock.Of<ILogger<PriceHistoryService>>()
        );

    /// <summary>
    /// Creates a PriceHistoryService with a mock repository that returns specified history data
    /// </summary>
    /// <param name="_">The test fixture instance (unused parameter).</param>
    /// <param name="history">The history data to return from the mock repository.</param>
    /// <param name="hoursBack">The number of hours to look back (default 24).</param>
    /// <returns>A new <see cref="PriceHistoryService"/> instance with configured mock repository.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="history"/> is null.</exception>
    public static PriceHistoryService CreateServiceWithHistory(
        this PriceHistoryServiceTests _,
        IEnumerable<PriceHistory> history,
        int hoursBack = 24)
    {
        ArgumentNullException.ThrowIfNull(history);

        var repoMock = new Mock<IHistoryRepository>();
        repoMock
            .Setup(r => r.GetHistoryByAssetAndFiatAsync(It.IsAny<string>(), It.IsAny<string>(), hoursBack))
            .ReturnsAsync(history.ToArray());

        return new PriceHistoryService(
            repoMock.Object,
            new AppSettings { DatabaseConnectionString = "Data Source=:memory:" },
            Mock.Of<ILogger<PriceHistoryService>>()
        );
    }

    /// <summary>
    /// Creates a PriceHistoryService with a mock repository that returns specified count for GetTotalHistoryCountAsync
    /// </summary>
    /// <param name="_">The test fixture instance (unused parameter).</param>
    /// <param name="totalCount">The total count to return from the mock repository.</param>
    /// <returns>A new <see cref="PriceHistoryService"/> instance with configured mock repository.</returns>
    public static PriceHistoryService CreateServiceWithHistoryCount(
        this PriceHistoryServiceTests _,
        long totalCount)
    {
        var repoMock = new Mock<IHistoryRepository>();
        repoMock
            .Setup(r => r.GetTotalHistoryCountAsync())
            .ReturnsAsync(totalCount);

        return new PriceHistoryService(
            repoMock.Object,
            new AppSettings { DatabaseConnectionString = "Data Source=:memory:" },
            Mock.Of<ILogger<PriceHistoryService>>()
        );
    }

    /// <summary>
    /// Creates a PriceHistoryService with a mock repository that returns specified result for CleanupOldHistoryAsync
    /// </summary>
    /// <param name="_">The test fixture instance (unused parameter).</param>
    /// <param name="cleanupResult">The cleanup result to return from the mock repository.</param>
    /// <param name="daysOld">The number of days old for cleanup (default 30).</param>
    /// <returns>A new <see cref="PriceHistoryService"/> instance with configured mock repository.</returns>
    public static PriceHistoryService CreateServiceWithCleanupResult(
        this PriceHistoryServiceTests _,
        bool cleanupResult,
        int daysOld = 30)
    {
        var repoMock = new Mock<IHistoryRepository>();
        repoMock
            .Setup(r => r.DeleteOldRecordsAsync(daysOld))
            .ReturnsAsync(cleanupResult);

        return new PriceHistoryService(
            repoMock.Object,
            new AppSettings { DatabaseConnectionString = "Data Source=:memory:" },
            Mock.Of<ILogger<PriceHistoryService>>()
        );
    }

    /// <summary>
    /// Creates a sequence of PriceHistory records with ascending prices for trend testing
    /// </summary>
    /// <param name="_">The test fixture instance (unused parameter).</param>
    /// <param name="asset">The asset symbol.</param>
    /// <param name="fiat">The fiat currency symbol.</param>
    /// <param name="count">The number of records to create.</param>
    /// <param name="startPrice">The starting price (default 40000).</param>
    /// <param name="hoursInterval">The hours interval between records (default 1).</param>
    /// <returns>An enumerable of <see cref="PriceHistory"/> records with ascending prices.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="asset"/> or <paramref name="fiat"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="count"/> is less than 1.</exception>
    public static IEnumerable<PriceHistory> CreateAscendingPriceHistory(
        this PriceHistoryServiceTests _,
        string asset,
        string fiat,
        int count,
        decimal startPrice = 40000m,
        int hoursInterval = 1)
    {
        ArgumentException.ThrowIfNullOrEmpty(asset);
        ArgumentException.ThrowIfNullOrEmpty(fiat);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(count, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(hoursInterval, 0);

        var now = DateTime.UtcNow;
        var baseTime = now.AddHours(-(hoursInterval * count));

        for (int i = 0; i < count; i++)
        {
            var currentTime = baseTime.AddHours(hoursInterval * i);
            yield return new PriceHistory
            {
                Asset = asset,
                Fiat = fiat,
                BuyPrice = startPrice + (i * 1000m),
                SellPrice = startPrice + (i * 1000m) + 100m,
                RecordedAt = currentTime,
                CreatedAt = now
            };
        }
    }

    /// <summary>
    /// Creates a sequence of PriceHistory records with descending prices for trend testing
    /// </summary>
    /// <param name="_">The test fixture instance (unused parameter).</param>
    /// <param name="asset">The asset symbol.</param>
    /// <param name="fiat">The fiat currency symbol.</param>
    /// <param name="count">The number of records to create.</param>
    /// <param name="startPrice">The starting price (default 50000).</param>
    /// <param name="hoursInterval">The hours interval between records (default 1).</param>
    /// <returns>An enumerable of <see cref="PriceHistory"/> records with descending prices.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="asset"/> or <paramref name="fiat"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="count"/> is less than 1.</exception>
    public static IEnumerable<PriceHistory> CreateDescendingPriceHistory(
        this PriceHistoryServiceTests _,
        string asset,
        string fiat,
        int count,
        decimal startPrice = 50000m,
        int hoursInterval = 1)
    {
        ArgumentException.ThrowIfNullOrEmpty(asset);
        ArgumentException.ThrowIfNullOrEmpty(fiat);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(count, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(hoursInterval, 0);

        var now = DateTime.UtcNow;
        var baseTime = now.AddHours(-(hoursInterval * count));

        for (int i = 0; i < count; i++)
        {
            var currentTime = baseTime.AddHours(hoursInterval * i);
            yield return new PriceHistory
            {
                Asset = asset,
                Fiat = fiat,
                BuyPrice = startPrice - (i * 1000m),
                SellPrice = startPrice - (i * 1000m) + 100m,
                RecordedAt = currentTime,
                CreatedAt = now
            };
        }
    }

    /// <summary>
    /// Creates a PriceHistoryService with a mock repository that throws an exception for testing error scenarios
    /// </summary>
    /// <param name="_">The test fixture instance (unused parameter).</param>
    /// <param name="exception">The exception to throw from the mock repository.</param>
    /// <returns>A new <see cref="PriceHistoryService"/> instance with configured mock repository.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    public static PriceHistoryService CreateServiceWithException(
        this PriceHistoryServiceTests _,
        Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var repoMock = new Mock<IHistoryRepository>();
        repoMock
            .Setup(r => r.GetHistoryByAssetAndFiatAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ThrowsAsync(exception);

        return new PriceHistoryService(
            repoMock.Object,
            new AppSettings { DatabaseConnectionString = "Data Source=:memory:" },
            Mock.Of<ILogger<PriceHistoryService>>()
        );
    }
}