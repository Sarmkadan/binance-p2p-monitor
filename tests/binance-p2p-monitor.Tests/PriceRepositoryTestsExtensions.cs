#nullable enable
using BinanceP2pMonitor.Data;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Xunit;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Extension methods for <see cref="PriceRepositoryTests"/> to provide reusable test assertions.
/// </summary>
public static class PriceRepositoryTestsExtensions
{
    /// <summary>
    /// Verifies that retrieving a price by ID returns a valid price with the expected ID.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="priceId">The price ID to retrieve.</param>
    /// <returns>The retrieved price.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static async Task<Price> GetByIdAsync_ShouldReturnValidPrice(this PriceRepositoryTests tests, int priceId)
    {
        ArgumentNullException.ThrowIfNull(tests);

        // Act
        var storedPrice = await tests.GetPriceRepository().GetByIdAsync(priceId).ConfigureAwait(false);

        // Assert
        storedPrice.Should().NotBeNull();
        storedPrice!.Id.Should().Be(priceId);

        return storedPrice;
    }

    /// <summary>
    /// Verifies that adding a price returns a valid ID and persists the price correctly.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="price">The price to add.</param>
    /// <returns>The ID of the added price.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> or <paramref name="price"/> is <see langword="null"/>.</exception>
    public static async Task<int> AddAsync_ShouldReturnValidIdAndPersist(this PriceRepositoryTests tests, Price price)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(price);

        // Act
        var id = await tests.GetPriceRepository().AddAsync(price).ConfigureAwait(false);

        // Assert
        id.Should().BeGreaterThan(0);

        var storedPrice = await tests.GetPriceRepository().GetByIdAsync(id).ConfigureAwait(false);
        storedPrice.Should().NotBeNull();
        storedPrice!.Asset.Should().Be(price.Asset);
        storedPrice.Fiat.Should().Be(price.Fiat);
        storedPrice.BuyPrice.Should().Be(price.BuyPrice);
        storedPrice.SellPrice.Should().Be(price.SellPrice);

        return id;
    }

    /// <summary>
    /// Verifies that retrieving a price by ID returns null when the price does not exist.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="nonExistentId">The non-existent price ID.</param>
    /// <returns>The null result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static async Task<Price?> GetByIdAsync_ShouldReturnNull_WhenPriceDoesNotExist(this PriceRepositoryTests tests, int nonExistentId)
    {
        ArgumentNullException.ThrowIfNull(tests);

        // Act
        var storedPrice = await tests.GetPriceRepository().GetByIdAsync(nonExistentId).ConfigureAwait(false);

        // Assert
        storedPrice.Should().BeNull();

        return storedPrice;
    }

    /// <summary>
    /// Verifies that retrieving the latest price by asset and fiat returns null when no price exists.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="asset">The asset code.</param>
    /// <param name="fiat">The fiat currency.</param>
    /// <returns>The null result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="asset"/> or <paramref name="fiat"/> is null or empty.</exception>
    public static async Task<Price?> GetLatestByAssetAndFiatAsync_ShouldReturnNull_WhenNoPriceExists(this PriceRepositoryTests tests, string asset, string fiat)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentException.ThrowIfNullOrEmpty(asset);
        ArgumentException.ThrowIfNullOrEmpty(fiat);

        // Act
        var result = await tests.GetPriceRepository().GetLatestByAssetAndFiatAsync(asset, fiat).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();

        return result;
    }

    /// <summary>
    /// Verifies that updating a non-existent price returns false.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="nonExistentPrice">The price with an invalid ID.</param>
    /// <returns>The update result (false).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> or <paramref name="nonExistentPrice"/> is <see langword="null"/>.</exception>
    public static async Task<bool> UpdateAsync_ShouldReturnFalse_WhenPriceDoesNotExist(this PriceRepositoryTests tests, Price nonExistentPrice)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(nonExistentPrice);

        // Set an invalid ID to simulate non-existent price
        nonExistentPrice.Id = 0;

        // Act
        var result = await tests.GetPriceRepository().UpdateAsync(nonExistentPrice).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();

        return result;
    }

    /// <summary>
    /// Verifies that deleting a non-existent price returns false.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="nonExistentId">The non-existent price ID.</param>
    /// <returns>The delete result (false).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static async Task<bool> DeleteAsync_ShouldReturnFalse_WhenPriceDoesNotExist(this PriceRepositoryTests tests, int nonExistentId)
    {
        ArgumentNullException.ThrowIfNull(tests);

        // Act
        var result = await tests.GetPriceRepository().DeleteAsync(nonExistentId).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();

        return result;
    }

    /// <summary>
    /// Verifies that retrieving the average price returns null when no prices exist in the time range.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="asset">The asset code.</param>
    /// <param name="fiat">The fiat currency.</param>
    /// <param name="hours">The time range in hours.</param>
    /// <returns>The null average price.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="asset"/> or <paramref name="fiat"/> is null or empty.</exception>
    public static async Task<decimal?> GetAveragePriceAsync_ShouldReturnNull_WhenNoPricesInTimeRange(this PriceRepositoryTests tests, string asset, string fiat, int hours)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentException.ThrowIfNullOrEmpty(asset);
        ArgumentException.ThrowIfNullOrEmpty(fiat);
        if (hours <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(hours), "Hours must be positive");
        }

        // Act
        var avgPrice = await tests.GetPriceRepository().GetAveragePriceAsync(asset, fiat, hours).ConfigureAwait(false);

        // Assert
        avgPrice.Should().BeNull();

        return avgPrice;
    }

    /// <summary>
    /// Gets the <see cref="PriceRepository"/> instance from the test.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <returns>The price repository.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static PriceRepository GetPriceRepository(this PriceRepositoryTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        // Use reflection to access the private field
        var field = typeof(PriceRepositoryTests).GetField("_priceRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field.Should().NotBeNull("The _priceRepository field should exist");

        return (PriceRepository)field!.GetValue(tests)!;
    }

    /// <summary>
    /// Gets the <see cref="DatabaseContext"/> instance from the test.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <returns>The database context.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static DatabaseContext GetDatabaseContext(this PriceRepositoryTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        // Use reflection to access the private field
        var field = typeof(PriceRepositoryTests).GetField("_context", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field.Should().NotBeNull("The _context field should exist");

        return (DatabaseContext)field!.GetValue(tests)!;
    }
}