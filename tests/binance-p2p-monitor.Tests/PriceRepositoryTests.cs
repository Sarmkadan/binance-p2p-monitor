#nullable enable
using BinanceP2pMonitor.Data;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Xunit;

namespace BinanceP2pMonitor.Tests;

public class PriceRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DatabaseContext _context;
    private readonly PriceRepository _priceRepository;

    public PriceRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new DatabaseContext(_connection);
        _priceRepository = new PriceRepository(_context);

        _context.ExecuteCommand(@"
            CREATE TABLE Prices (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Asset TEXT NOT NULL,
                Fiat TEXT NOT NULL,
                BuyPrice REAL NOT NULL,
                SellPrice REAL NOT NULL,
                BuyChangePercent REAL NOT NULL,
                SellChangePercent REAL NOT NULL,
                Timestamp TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                Metadata TEXT
            );");
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
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }

    private Price CreateTestPrice(
        string asset = "USDT",
        string fiat = "UAH",
        decimal buyPrice = 38.0m,
        decimal sellPrice = 38.5m,
        DateTime? timestamp = null)
    {
        return new Price
        {
            Asset = asset,
            Fiat = fiat,
            BuyPrice = buyPrice,
            SellPrice = sellPrice,
            BuyChangePercent = 0.0m,
            SellChangePercent = 0.0m,
            Timestamp = timestamp ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task AddAsync_ShouldAddPriceAndReturnId()
    {
        // Arrange
        var price = CreateTestPrice();

        // Act
        var id = await _priceRepository.AddAsync(price).ConfigureAwait(false);

        // Assert
        id.Should().BeGreaterThan(0);
        var storedPrice = await _priceRepository.GetByIdAsync(id).ConfigureAwait(false);
        storedPrice.Should().NotBeNull();
        storedPrice!.Asset.Should().Be(price.Asset);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPrice_WhenPriceExists()
    {
        // Arrange
        var price = CreateTestPrice();
        var id = await _priceRepository.AddAsync(price).ConfigureAwait(false);

        // Act
        var storedPrice = await _priceRepository.GetByIdAsync(id).ConfigureAwait(false);

        // Assert
        storedPrice.Should().NotBeNull();
        storedPrice!.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenPriceDoesNotExist()
    {
        // Act
        var storedPrice = await _priceRepository.GetByIdAsync(999).ConfigureAwait(false);

        // Assert
        storedPrice.Should().BeNull();
    }

    [Fact]
    public async Task GetLatestByAssetAndFiatAsync_ShouldReturnLatestPrice()
    {
        // Arrange
        var asset = "USDT";
        var fiat = "UAH";
        await _priceRepository.AddAsync(CreateTestPrice(asset, fiat, buyPrice: 38.0m, timestamp: DateTime.UtcNow.AddMinutes(-5))).ConfigureAwait(false);
        var latestPrice = CreateTestPrice(asset, fiat, buyPrice: 38.5m, timestamp: DateTime.UtcNow);
        await _priceRepository.AddAsync(latestPrice).ConfigureAwait(false);

        // Act
        var result = await _priceRepository.GetLatestByAssetAndFiatAsync(asset, fiat).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.BuyPrice.Should().Be(latestPrice.BuyPrice);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdatePriceAndReturnTrue()
    {
        // Arrange
        var price = CreateTestPrice();
        var id = await _priceRepository.AddAsync(price).ConfigureAwait(false);
        var storedPrice = await _priceRepository.GetByIdAsync(id).ConfigureAwait(false);
        storedPrice!.BuyPrice = 39.0m;
        storedPrice.SellPrice = 39.5m;

        // Act
        var result = await _priceRepository.UpdateAsync(storedPrice).ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
        var updatedPrice = await _priceRepository.GetByIdAsync(id).ConfigureAwait(false);
        updatedPrice!.BuyPrice.Should().Be(39.0m);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeletePriceAndReturnTrue()
    {
        // Arrange
        var price = CreateTestPrice();
        var id = await _priceRepository.AddAsync(price).ConfigureAwait(false);

        // Act
        var result = await _priceRepository.DeleteAsync(id).ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
        var deletedPrice = await _priceRepository.GetByIdAsync(id).ConfigureAwait(false);
        deletedPrice.Should().BeNull();
    }

    [Fact]
    public async Task GetAveragePriceAsync_ShouldReturnAveragePrice()
    {
        // Arrange
        var asset = "USDT";
        var fiat = "UAH";
        await _priceRepository.AddAsync(CreateTestPrice(asset, fiat, buyPrice: 38.0m, sellPrice: 38.0m, timestamp: DateTime.UtcNow.AddHours(-1))).ConfigureAwait(false);
        await _priceRepository.AddAsync(CreateTestPrice(asset, fiat, buyPrice: 39.0m, sellPrice: 39.0m, timestamp: DateTime.UtcNow.AddHours(-2))).ConfigureAwait(false);
        
        // Act
        var avgPrice = await _priceRepository.GetAveragePriceAsync(asset, fiat, 24).ConfigureAwait(false);

        // Assert
        avgPrice.Should().Be(38.5m);
    }
}
