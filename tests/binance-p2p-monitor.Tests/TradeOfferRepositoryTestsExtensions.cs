#nullable enable

using BinanceP2pMonitor.Constants;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Extension methods for <see cref="TradeOfferRepositoryTests"/> to provide additional test utilities.
/// </summary>
public static class TradeOfferRepositoryTestsExtensions
{
    private static TradeOfferRepository GetTradeOfferRepository(this TradeOfferRepositoryTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        var field = typeof(TradeOfferRepositoryTests).GetField(
            "_tradeOfferRepository",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (TradeOfferRepository)field!.GetValue(tests)!;
    }

    /// <summary>
    /// Creates and adds a test trade offer to the repository, returning the created offer.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="binanceId">Optional Binance offer ID. Defaults to "TEST_OFFER_{Guid.NewGuid()}".</param>
    /// <param name="asset">Optional asset symbol. Defaults to "USDT".</param>
    /// <param name="fiat">Optional fiat currency. Defaults to "USD".</param>
    /// <param name="tradeType">Optional trade type. Defaults to <see cref="TradeType.Buy"/>.</param>
    /// <param name="price">Optional price. Defaults to 1.0m.</param>
    /// <param name="isActive">Optional active status. Defaults to true.</param>
    /// <returns>The created trade offer with populated ID.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
    public static async Task<TradeOffer> CreateAndAddTestTradeOfferAsync(
        this TradeOfferRepositoryTests tests,
        string? binanceId = null,
        string? asset = null,
        string? fiat = null,
        TradeType tradeType = TradeType.Buy,
        decimal price = 1.0m,
        bool isActive = true)
    {
        ArgumentNullException.ThrowIfNull(tests);

        var offer = new TradeOffer
        {
            OfferIdFromBinance = binanceId ?? $"TEST_OFFER_{Guid.NewGuid()}",
            Asset = asset ?? "USDT",
            Fiat = fiat ?? "USD",
            TradeType = tradeType,
            Price = price,
            MinAmount = 10,
            MaxAmount = 1000,
            TraderRating = 95.0m,
            CompletedTrades = 50,
            PaymentMethods = "Test Payment Method",
            IsActive = isActive,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var id = await tests.GetTradeOfferRepository().AddAsync(offer).ConfigureAwait(false);
        offer.Id = id;

        return offer;
    }

    /// <summary>
    /// Creates and adds multiple test trade offers to the repository.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="count">Number of offers to create. Must be positive.</param>
    /// <param name="asset">Optional asset symbol. Defaults to "USDT".</param>
    /// <param name="fiat">Optional fiat currency. Defaults to "USD".</param>
    /// <param name="priceRange">Optional price range. Defaults to 1.0m to 2.0m.</param>
    /// <returns>Collection of created trade offers with populated IDs.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="count"/> is not positive.</exception>
    public static async Task<IReadOnlyList<TradeOffer>> CreateAndAddTestTradeOffersAsync(
        this TradeOfferRepositoryTests tests,
        int count,
        string? asset = null,
        string? fiat = null,
        (decimal min, decimal max)? priceRange = null)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(count, 0);

        var offers = new List<TradeOffer>(count);
        var random = new Random();

        for (var i = 0; i < count; i++)
        {
            var tradeType = i % 2 == 0 ? TradeType.Buy : TradeType.Sell;
            var isActive = i % 2 == 0;

            var offer = new TradeOffer
            {
                OfferIdFromBinance = $"TEST_OFFER_{Guid.NewGuid()}",
                Asset = asset ?? "USDT",
                Fiat = fiat ?? "USD",
                TradeType = tradeType,
                Price = priceRange.HasValue
                    ? priceRange.Value.min + (decimal)random.NextDouble() * (priceRange.Value.max - priceRange.Value.min)
                    : 1.0m + i * 0.5m,
                MinAmount = 10 + i * 10,
                MaxAmount = 100 + i * 100,
                TraderRating = 85.0m + i * 2.5m,
                CompletedTrades = 10 + i * 20,
                PaymentMethods = i % 3 == 0 ? "Bank Transfer" : i % 3 == 1 ? "Credit Card" : "PayPal",
                IsActive = isActive,
                Timestamp = DateTime.UtcNow.AddHours(-i),
                CreatedAt = DateTime.UtcNow.AddHours(-i),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-i)
            };

            var id = await tests.GetTradeOfferRepository().AddAsync(offer).ConfigureAwait(false);
            offer.Id = id;
            offers.Add(offer);
        }

        return offers.AsReadOnly();
    }

    /// <summary>
    /// Verifies that a trade offer matches expected values.
    /// </summary>
    /// <param name="actual">The actual trade offer.</param>
    /// <param name="expected">The expected trade offer values.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="actual"/> or <paramref name="expected"/> is null.</exception>
    public static void ShouldMatchExpectedValues(
        this TradeOffer actual,
        TradeOffer expected)
    {
        ArgumentNullException.ThrowIfNull(actual);
        ArgumentNullException.ThrowIfNull(expected);

        actual.Id.Should().Be(expected.Id);
        actual.OfferIdFromBinance.Should().Be(expected.OfferIdFromBinance);
        actual.Asset.Should().Be(expected.Asset);
        actual.Fiat.Should().Be(expected.Fiat);
        actual.TradeType.Should().Be(expected.TradeType);
        actual.Price.Should().Be(expected.Price);
        actual.MinAmount.Should().Be(expected.MinAmount);
        actual.MaxAmount.Should().Be(expected.MaxAmount);
        actual.TraderRating.Should().Be(expected.TraderRating);
        actual.CompletedTrades.Should().Be(expected.CompletedTrades);
        actual.PaymentMethods.Should().Be(expected.PaymentMethods);
        actual.IsActive.Should().Be(expected.IsActive);
        actual.Timestamp.Should().BeCloseTo(expected.Timestamp, TimeSpan.FromSeconds(1));
        actual.CreatedAt.Should().BeCloseTo(expected.CreatedAt, TimeSpan.FromSeconds(1));
        actual.UpdatedAt.Should().BeCloseTo(expected.UpdatedAt, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Gets the count of active trade offers in the repository.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <returns>The count of active trade offers.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
    public static async Task<int> GetActiveOffersCountAsync(this TradeOfferRepositoryTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        var activeOffers = await tests.GetTradeOfferRepository().GetAllActiveAsync().ConfigureAwait(false);
        return activeOffers.Count();
    }
}