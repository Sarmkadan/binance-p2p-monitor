#nullable enable
using BinanceP2pMonitor.Constants;
using BinanceP2pMonitor.Data;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Xunit;

/// <summary>
/// Tests for the TradeOfferRepository class.
/// </summary>
public class TradeOfferRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DatabaseContext _context;
    private readonly TradeOfferRepository _tradeOfferRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="TradeOfferRepositoryTests"/> class.
    /// </summary>
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

    /// <summary>
    /// Releases unmanaged resources and performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }

    /// <summary>
    /// Creates a test trade offer with the specified properties.
    /// </summary>
    /// <param name="binanceId">The Binance ID of the trade offer.</param>
    /// <param name="asset">The asset of the trade offer.</param>
    /// <param name="fiat">The fiat of the trade offer.</param>
    /// <param name="tradeType">The trade type of the trade offer.</param>
    /// <param name="price">The price of the trade offer.</param>
    /// <param name="isActive">Whether the trade offer is active.</param>
    /// <returns>A test trade offer with the specified properties.</returns>
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

    /// <summary>
    /// Tests that adding a trade offer and returning the ID works correctly.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task AddAsync_ShouldAddTradeOfferAndReturnId()
    {
        // Arrange
        var offer = CreateTestTradeOffer();

        // Act
        var id = await _tradeOfferRepository.AddAsync(offer).ConfigureAwait(false);

        // Assert
        id.Should().BeGreaterThan(0);
        var storedOffer = await _tradeOfferRepository.GetByIdAsync(id).ConfigureAwait(false);
        storedOffer.Should().NotBeNull();
        storedOffer!.OfferIdFromBinance.Should().Be(offer.OfferIdFromBinance);
    }

    /// <summary>
    /// Tests that getting a trade offer by ID works correctly when the offer exists.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GetByIdAsync_ShouldReturnTradeOffer_WhenOfferExists()
    {
        // Arrange
        var offer = CreateTestTradeOffer();
        var id = await _tradeOfferRepository.AddAsync(offer).ConfigureAwait(false);

        // Act
        var storedOffer = await _tradeOfferRepository.GetByIdAsync(id).ConfigureAwait(false);

        // Assert
        storedOffer.Should().NotBeNull();
        storedOffer!.Id.Should().Be(id);
    }

    /// <summary>
    /// Tests that getting a trade offer by ID returns null when the offer does not exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenOfferDoesNotExist()
    {
        // Act
        var storedOffer = await _tradeOfferRepository.GetByIdAsync(999).ConfigureAwait(false);

        // Assert
        storedOffer.Should().BeNull();
    }

    /// <summary>
    /// Tests that getting a trade offer by Binance ID works correctly when the offer exists.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GetByBinanceIdAsync_ShouldReturnTradeOffer_WhenOfferExists()
    {
        // Arrange
        var offer = CreateTestTradeOffer("BINANCE_XYZ");
        await _tradeOfferRepository.AddAsync(offer).ConfigureAwait(false);

        // Act
        var storedOffer = await _tradeOfferRepository.GetByBinanceIdAsync("BINANCE_XYZ").ConfigureAwait(false);

        // Assert
        storedOffer.Should().NotBeNull();
        storedOffer!.OfferIdFromBinance.Should().Be("BINANCE_XYZ");
    }

    /// <summary>
    /// Tests that getting all active trade offers works correctly.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GetAllActiveAsync_ShouldReturnAllActiveOffers()
    {
        // Arrange
        await _tradeOfferRepository.AddAsync(CreateTestTradeOffer(binanceId: "1", isActive: true)).ConfigureAwait(false);
        await _tradeOfferRepository.AddAsync(CreateTestTradeOffer(binanceId: "2", isActive: true)).ConfigureAwait(false);
        await _tradeOfferRepository.AddAsync(CreateTestTradeOffer(binanceId: "3", isActive: false)).ConfigureAwait(false); // Inactive offer

        // Act
        var activeOffers = await _tradeOfferRepository.GetAllActiveAsync().ConfigureAwait(false);

        // Assert
        activeOffers.Should().HaveCount(2);
        activeOffers.Should().AllSatisfy(o => o.IsActive.Should().BeTrue());
    }

    /// <summary>
    /// Tests that updating a trade offer works correctly.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateAsync_ShouldUpdateTradeOfferAndReturnTrue()
    {
        // Arrange
        var offer = CreateTestTradeOffer();
        var id = await _tradeOfferRepository.AddAsync(offer).ConfigureAwait(false);
        var storedOffer = await _tradeOfferRepository.GetByIdAsync(id).ConfigureAwait(false);
        storedOffer!.Price = 39.0m;
        storedOffer.IsActive = false;

        // Act
        var result = await _tradeOfferRepository.UpdateAsync(storedOffer).ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
        var updatedOffer = await _tradeOfferRepository.GetByIdAsync(id).ConfigureAwait(false);
        updatedOffer!.Price.Should().Be(39.0m);
        updatedOffer.IsActive.Should().BeFalse();
    }

    /// <summary>
    /// Tests that deleting a trade offer works correctly.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task DeleteAsync_ShouldDeleteTradeOfferAndReturnTrue()
    {
        // Arrange
        var offer = CreateTestTradeOffer();
        var id = await _tradeOfferRepository.AddAsync(offer).ConfigureAwait(false);

        // Act
        var result = await _tradeOfferRepository.DeleteAsync(id).ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
        var deletedOffer = await _tradeOfferRepository.GetByIdAsync(id).ConfigureAwait(false);
        deletedOffer.Should().BeNull();
    }
}
