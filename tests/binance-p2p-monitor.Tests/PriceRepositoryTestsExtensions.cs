#nullable enable
using BinanceP2pMonitor.Data;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Xunit;

namespace BinanceP2pMonitor.Tests;

public static class PriceRepositoryTestsExtensions
{
    public static async Task<Price> GetByIdAsync_ShouldReturnValidPrice(this PriceRepositoryTests tests, int priceId)
    {
        // Act
        var storedPrice = await tests.GetPriceRepository().GetByIdAsync(priceId).ConfigureAwait(false);

        // Assert
        storedPrice.Should().NotBeNull();
        storedPrice!.Id.Should().Be(priceId);

        return storedPrice;
    }

    public static async Task<int> AddAsync_ShouldReturnValidIdAndPersist(this PriceRepositoryTests tests, Price price)
    {
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

    public static async Task<Price?> GetByIdAsync_ShouldReturnNull_WhenPriceDoesNotExist(this PriceRepositoryTests tests, int nonExistentId)
    {
        // Act
        var storedPrice = await tests.GetPriceRepository().GetByIdAsync(nonExistentId).ConfigureAwait(false);

        // Assert
        storedPrice.Should().BeNull();

        return storedPrice;
    }

    public static async Task<Price?> GetLatestByAssetAndFiatAsync_ShouldReturnNull_WhenNoPriceExists(this PriceRepositoryTests tests, string asset, string fiat)
    {
        // Act
        var result = await tests.GetPriceRepository().GetLatestByAssetAndFiatAsync(asset, fiat).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();

        return result;
    }

    public static async Task<bool> UpdateAsync_ShouldReturnFalse_WhenPriceDoesNotExist(this PriceRepositoryTests tests, Price nonExistentPrice)
    {
        // Set an invalid ID to simulate non-existent price
        nonExistentPrice.Id = 0;

        // Act
        var result = await tests.GetPriceRepository().UpdateAsync(nonExistentPrice).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();

        return result;
    }

    public static async Task<bool> DeleteAsync_ShouldReturnFalse_WhenPriceDoesNotExist(this PriceRepositoryTests tests, int nonExistentId)
    {
        // Act
        var result = await tests.GetPriceRepository().DeleteAsync(nonExistentId).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();

        return result;
    }

    public static async Task<decimal?> GetAveragePriceAsync_ShouldReturnNull_WhenNoPricesInTimeRange(this PriceRepositoryTests tests, string asset, string fiat, int hours)
    {
        // Act
        var avgPrice = await tests.GetPriceRepository().GetAveragePriceAsync(asset, fiat, hours).ConfigureAwait(false);

        // Assert
        avgPrice.Should().BeNull();

        return avgPrice;
    }

    public static PriceRepository GetPriceRepository(this PriceRepositoryTests tests)
    {
        // Use reflection to access the private field
        var field = typeof(PriceRepositoryTests).GetField("_priceRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field.Should().NotBeNull("The _priceRepository field should exist");

        return (PriceRepository)field!.GetValue(tests)!;
    }

    public static DatabaseContext GetDatabaseContext(this PriceRepositoryTests tests)
    {
        // Use reflection to access the private field
        var field = typeof(PriceRepositoryTests).GetField("_context", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field.Should().NotBeNull("The _context field should exist");

        return (DatabaseContext)field!.GetValue(tests)!;
    }
}