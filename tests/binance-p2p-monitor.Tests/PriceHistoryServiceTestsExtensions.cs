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
    public static PriceHistoryService CreateDefaultService(this PriceHistoryServiceTests _) =>
        new(
            Mock.Of<IHistoryRepository>(),
            new AppSettings { DatabaseConnectionString = "Data Source=:memory:" },
            Mock.Of<ILogger<PriceHistoryService>>()
        );

    /// <summary>
    /// Creates a PriceHistoryService with a mock repository that returns specified history data
    /// </summary>
    public static PriceHistoryService CreateServiceWithHistory(
        this PriceHistoryServiceTests _,
        IEnumerable<PriceHistory> history,
        int hoursBack = 24)
    {
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
    public static IEnumerable<PriceHistory> CreateAscendingPriceHistory(
        this PriceHistoryServiceTests _,
        string asset,
        string fiat,
        int count,
        decimal startPrice = 40000m,
        int hoursInterval = 1)
    {
        for (int i = 0; i < count; i++)
        {
            yield return new PriceHistory
            {
                Asset = asset,
                Fiat = fiat,
                BuyPrice = startPrice + (i * 1000m),
                SellPrice = startPrice + (i * 1000m) + 100m,
                RecordedAt = DateTime.UtcNow.AddHours(-(hoursInterval * (count - i))),
                CreatedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Creates a sequence of PriceHistory records with descending prices for trend testing
    /// </summary>
    public static IEnumerable<PriceHistory> CreateDescendingPriceHistory(
        this PriceHistoryServiceTests _,
        string asset,
        string fiat,
        int count,
        decimal startPrice = 50000m,
        int hoursInterval = 1)
    {
        for (int i = 0; i < count; i++)
        {
            yield return new PriceHistory
            {
                Asset = asset,
                Fiat = fiat,
                BuyPrice = startPrice - (i * 1000m),
                SellPrice = startPrice - (i * 1000m) + 100m,
                RecordedAt = DateTime.UtcNow.AddHours(-(hoursInterval * (count - i))),
                CreatedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Creates a PriceHistoryService with a mock repository that throws an exception for testing error scenarios
    /// </summary>
    public static PriceHistoryService CreateServiceWithException(
        this PriceHistoryServiceTests _,
        Exception exception)
    {
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