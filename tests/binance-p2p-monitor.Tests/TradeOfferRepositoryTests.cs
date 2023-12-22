#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BinanceP2pMonitor.Constants;
using BinanceP2pMonitor.Data;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Xunit;

namespace BinanceP2pMonitor.Tests;

public class TradeOfferRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DatabaseContext _context;
    private readonly TradeOfferRepository _tradeOfferRepository;

    public TradeOfferRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new DatabaseContext(_connection);
        _tradeOfferRepository = new TradeOfferRepository(_context);

        _context.ExecuteCommand(@"
            CREATE TABLE TradeOffers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                OfferIdFromBinance TEXT NOT NULL,
                Asset TEXT NOT NULL,
                Fiat TEXT NOT NULL,
                TradeType INTEGER NOT NULL,
                Price REAL NOT NULL,
                MinAmount REAL NOT NULL,
                MaxAmount REAL NOT NULL,
                TraderRating REAL NOT NULL,
                CompletedTrades INTEGER NOT NULL,
                PaymentMethods TEXT,
                IsActive INTEGER NOT NULL,
                Timestamp TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
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

    private TradeOffer CreateTestTradeOffer(
        string binanceId = "BINANCE_ID_1",
        string asset = "USDT",
        string fiat = "UAH",
        TradeType tradeType = TradeType.Buy,
        decimal price = 38.0m,
        bool isActive = true)
    {
        return new TradeOffer
        {
            OfferIdFromBinance = binanceId,
            Asset = asset,
            Fiat = fiat,
            TradeType = tradeType,
            Price = price,
            MinAmount = 100,
            MaxAmount = 1000,
            TraderRating = 99.5m,
            CompletedTrades = 1000,
            PaymentMethods = "Bank Transfer",
            IsActive = isActive,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task AddAsync_ShouldAddTradeOfferAndReturnId()
    {
        // Arrange
        var offer = CreateTestTradeOffer();

        // Act
        var id = await _tradeOfferRepository.AddAsync(offer);

        // Assert
        id.Should().BeGreaterThan(0);
        var storedOffer = await _tradeOfferRepository.GetByIdAsync(id);
        storedOffer.Should().NotBeNull();
        storedOffer!.OfferIdFromBinance.Should().Be(offer.OfferIdFromBinance);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTradeOffer_WhenOfferExists()
    {
        // Arrange
        var offer = CreateTestTradeOffer();
        var id = await _tradeOfferRepository.AddAsync(offer);

        // Act
        var storedOffer = await _tradeOfferRepository.GetByIdAsync(id);

        // Assert
        storedOffer.Should().NotBeNull();
        storedOffer!.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenOfferDoesNotExist()
    {
        // Act
        var storedOffer = await _tradeOfferRepository.GetByIdAsync(999);

        // Assert
        storedOffer.Should().BeNull();
    }

    [Fact]
    public async Task GetByBinanceIdAsync_ShouldReturnTradeOffer_WhenOfferExists()
    {
        // Arrange
        var offer = CreateTestTradeOffer("BINANCE_XYZ");
        await _tradeOfferRepository.AddAsync(offer);

        // Act
        var storedOffer = await _tradeOfferRepository.GetByBinanceIdAsync("BINANCE_XYZ");

        // Assert
        storedOffer.Should().NotBeNull();
        storedOffer!.OfferIdFromBinance.Should().Be("BINANCE_XYZ");
    }

    [Fact]
    public async Task GetAllActiveAsync_ShouldReturnAllActiveOffers()
    {
        // Arrange
        await _tradeOfferRepository.AddAsync(CreateTestTradeOffer(binanceId: "1", isActive: true));
        await _tradeOfferRepository.AddAsync(CreateTestTradeOffer(binanceId: "2", isActive: true));
        await _tradeOfferRepository.AddAsync(CreateTestTradeOffer(binanceId: "3", isActive: false)); // Inactive offer

        // Act
        var activeOffers = await _tradeOfferRepository.GetAllActiveAsync();

        // Assert
        activeOffers.Should().HaveCount(2);
        activeOffers.Should().AllSatisfy(o => o.IsActive.Should().BeTrue());
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTradeOfferAndReturnTrue()
    {
        // Arrange
        var offer = CreateTestTradeOffer();
        var id = await _tradeOfferRepository.AddAsync(offer);
        var storedOffer = await _tradeOfferRepository.GetByIdAsync(id);
        storedOffer!.Price = 39.0m;
        storedOffer.IsActive = false;

        // Act
        var result = await _tradeOfferRepository.UpdateAsync(storedOffer);

        // Assert
        result.Should().BeTrue();
        var updatedOffer = await _tradeOfferRepository.GetByIdAsync(id);
        updatedOffer!.Price.Should().Be(39.0m);
        updatedOffer.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteTradeOfferAndReturnTrue()
    {
        // Arrange
        var offer = CreateTestTradeOffer();
        var id = await _tradeOfferRepository.AddAsync(offer);

        // Act
        var result = await _tradeOfferRepository.DeleteAsync(id);

        // Assert
        result.Should().BeTrue();
        var deletedOffer = await _tradeOfferRepository.GetByIdAsync(id);
        deletedOffer.Should().BeNull();
    }
}
