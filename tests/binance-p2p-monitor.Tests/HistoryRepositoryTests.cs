// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BinanceP2pMonitor.Data;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Xunit;

namespace BinanceP2pMonitor.Tests;

public class HistoryRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DatabaseContext _context;
    private readonly HistoryRepository _historyRepository;

    public HistoryRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new DatabaseContext(_connection);
        _historyRepository = new HistoryRepository(_context);

        _context.ExecuteCommand(@"
            CREATE TABLE PriceHistory (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PriceId INTEGER NOT NULL,
                Asset TEXT NOT NULL,
                Fiat TEXT NOT NULL,
                BuyPrice REAL NOT NULL,
                SellPrice REAL NOT NULL,
                RecordedAt TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                SpreadPercentage REAL NOT NULL,
                PriceChangePercent REAL NOT NULL,
                Notes TEXT
            );");
        _context.ExecuteCommand(@"
            CREATE TABLE PriceAlerts (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Asset TEXT NOT NULL,
                Fiat TEXT NOT NULL,
                AlertType INTEGER NOT NULL,
                Threshold REAL NOT NULL,
                Condition INTEGER NOT NULL,
                IsEnabled INTEGER NOT NULL,
                UserId INTEGER NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                LastTriggeredAt INTEGER,
                TriggerCount INTEGER NOT NULL,
                Notes TEXT
            );");
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }

    private PriceHistory CreateTestPriceHistory(
        string asset = "USDT",
        string fiat = "UAH",
        decimal buyPrice = 38.0m,
        decimal sellPrice = 38.5m,
        DateTime? recordedAt = null)
    {
        return new PriceHistory
        {
            PriceId = 1,
            Asset = asset,
            Fiat = fiat,
            BuyPrice = buyPrice,
            SellPrice = sellPrice,
            RecordedAt = recordedAt ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            SpreadPercentage = 1.0m,
            PriceChangePercent = 0.5m,
            Notes = "Test History"
        };
    }

    [Fact]
    public async Task AddAsync_ShouldAddHistoryAndReturnId()
    {
        // Arrange
        var history = CreateTestPriceHistory();

        // Act
        var id = await _historyRepository.AddAsync(history);

        // Assert
        id.Should().BeGreaterThan(0);
        var storedHistory = await _historyRepository.GetByIdAsync(id);
        storedHistory.Should().NotBeNull();
        storedHistory!.Asset.Should().Be(history.Asset);
        storedHistory.BuyPrice.Should().Be(history.BuyPrice);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnHistory_WhenHistoryExists()
    {
        // Arrange
        var history = CreateTestPriceHistory();
        var id = await _historyRepository.AddAsync(history);

        // Act
        var storedHistory = await _historyRepository.GetByIdAsync(id);

        // Assert
        storedHistory.Should().NotBeNull();
        storedHistory!.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenHistoryDoesNotExist()
    {
        // Act
        var storedHistory = await _historyRepository.GetByIdAsync(999);

        // Assert
        storedHistory.Should().BeNull();
    }

    [Fact]
    public async Task GetHistoryByAssetAndFiatAsync_ShouldReturnHistoryForAssetAndFiatWithinHours()
    {
        // Arrange
        var asset = "USDT";
        var fiat = "UAH";
        await _historyRepository.AddAsync(CreateTestPriceHistory(asset, fiat, recordedAt: DateTime.UtcNow.AddHours(-1)));
        await _historyRepository.AddAsync(CreateTestPriceHistory(asset, fiat, recordedAt: DateTime.UtcNow.AddHours(-3)));
        await _historyRepository.AddAsync(CreateTestPriceHistory("BTC", "USD", recordedAt: DateTime.UtcNow.AddHours(-1))); // Different asset/fiat
        await _historyRepository.AddAsync(CreateTestPriceHistory(asset, fiat, recordedAt: DateTime.UtcNow.AddHours(-25))); // Too old

        // Act
        var history = await _historyRepository.GetHistoryByAssetAndFiatAsync(asset, fiat, hours: 24);

        // Assert
        history.Should().HaveCount(2);
        history.Should().AllSatisfy(h =>
        {
            h.Asset.Should().Be(asset);
            h.Fiat.Should().Be(fiat);
            h.RecordedAt.Should().BeAfter(DateTime.UtcNow.AddHours(-25));
        });
    }

    [Fact]
    public async Task DeleteOldRecordsAsync_ShouldDeleteRecordsOlderThanDays()
    {
        // Arrange
        await _historyRepository.AddAsync(CreateTestPriceHistory(recordedAt: DateTime.UtcNow.AddDays(-5)));
        await _historyRepository.AddAsync(CreateTestPriceHistory(recordedAt: DateTime.UtcNow.AddDays(-1)));

        // Act
        var result = await _historyRepository.DeleteOldRecordsAsync(daysOld: 3);

        // Assert
        result.Should().BeTrue();
        var count = await _historyRepository.GetTotalHistoryCountAsync();
        count.Should().Be(1);
    }

    [Fact]
    public async Task GetTotalHistoryCountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        await _historyRepository.AddAsync(CreateTestPriceHistory());
        await _historyRepository.AddAsync(CreateTestPriceHistory());

        // Act
        var count = await _historyRepository.GetTotalHistoryCountAsync();

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public async Task GetHighestPriceAsync_ShouldReturnHighestPrice_WithinHours()
    {
        // Arrange
        var asset = "USDT";
        var fiat = "UAH";
        await _historyRepository.AddAsync(CreateTestPriceHistory(asset, fiat, buyPrice: 38.0m, sellPrice: 38.5m, recordedAt: DateTime.UtcNow.AddHours(-1)));
        await _historyRepository.AddAsync(CreateTestPriceHistory(asset, fiat, buyPrice: 39.0m, sellPrice: 39.5m, recordedAt: DateTime.UtcNow.AddHours(-2)));
        await _historyRepository.AddAsync(CreateTestPriceHistory(asset, fiat, buyPrice: 37.0m, sellPrice: 37.5m, recordedAt: DateTime.UtcNow.AddHours(-3)));

        // Act
        var highestPrice = await _historyRepository.GetHighestPriceAsync(asset, fiat, 24);

        // Assert
        highestPrice.Should().Be(39.5m);
    }
}
