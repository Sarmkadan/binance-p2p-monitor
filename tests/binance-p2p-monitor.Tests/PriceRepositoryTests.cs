#nullable enable
using BinanceP2pMonitor.Data;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Xunit;

/// <summary>
/// Tests for the PriceRepository class.
/// </summary>
public class PriceRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DatabaseContext _context;
    private readonly PriceRepository _priceRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="PriceRepositoryTests"/> class.
    /// </summary>
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

    /// <summary>
    /// Releases unmanaged resources and performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }

    /// <summary>
    /// Creates a test price with the specified properties.
    /// </summary>
    /// <param name="asset">The asset of the price.</param>
    /// <param name="fiat">The fiat of the price.</param>
    /// <param name="buyPrice">The buy price of the price.</param>
    /// <param name="sellPrice">The sell price of the price.</param>
    /// <param name="timestamp">The timestamp of the price.</param>
    /// <returns>A test price with the specified properties.</returns>
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

    /// <summary>
    /// Tests that adding a price and retrieving it by ID returns the correct price.
    /// </summary>
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

    /// <summary>
    /// Tests that retrieving a price by ID returns the correct price when the price exists.
    /// </summary>
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

    /// <summary>
    /// Tests that retrieving a price by ID returns null when the price does not exist.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenPriceDoesNotExist()
    {
        // Act
        var storedPrice = await _priceRepository.GetByIdAsync(999).ConfigureAwait(false);

        // Assert
        storedPrice.Should().BeNull();
    }

    /// <summary>
    /// Tests that retrieving the latest price by asset and fiat returns the correct price.
    /// </summary>
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

    /// <summary>
    /// Tests that updating a price and retrieving it returns the updated price.
    /// </summary>
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

    /// <summary>
    /// Tests that deleting a price and retrieving it returns null.
    /// </summary>
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

    /// <summary>
    /// Tests that retrieving the average price by asset and fiat returns the correct average price.
    /// </summary>
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
