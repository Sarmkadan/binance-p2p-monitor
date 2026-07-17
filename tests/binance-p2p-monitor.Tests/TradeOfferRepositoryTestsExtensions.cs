#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BinanceP2pMonitor.Constants;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using FluentAssertions;
using Xunit;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Extension methods for <see cref="TradeOfferRepositoryTests"/> that provide additional test utilities.
/// </summary>
public static class TradeOfferRepositoryTestsExtensions
{
    /// <summary>
    /// Retrieves the <see cref="TradeOfferRepository"/> instance from a <see cref="TradeOfferRepositoryTests"/>
    /// test class via reflection.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <returns>The underlying <see cref="TradeOfferRepository"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if the private field <c>_tradeOfferRepository</c> cannot be found.</exception>
    private static TradeOfferRepository GetTradeOfferRepository(this TradeOfferRepositoryTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        var field = typeof(TradeOfferRepositoryTests).GetField(
            "_tradeOfferRepository",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (field is null)
        {
            throw new ArgumentException("Unable to locate the private field '_tradeOfferRepository' on the test class.", nameof(tests));
        }

        var value = field.GetValue(tests);
        return (TradeOfferRepository)value!;
    }

    /// <summary>
    /// Creates and adds a test trade offer to the repository, returning the created offer.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="binanceId">Optional Binance offer ID. Defaults to <c>"TEST_OFFER_{Guid.NewGuid()}"</c>.</param>
    /// <param name="asset">Optional asset symbol. Defaults to <c>"USDT"</c>.</param>
    /// <param name="fiat">Optional fiat currency. Defaults to <c>"USD"</c>.</param>
    /// <param name="tradeType">Optional trade type. Defaults to <see cref="TradeType.Buy"/>.</param>
    /// <param name="price">Optional price in the specified currency. Defaults to <c>1.0m</c>.</param>
    /// <param name="isActive">Optional active status. Defaults to <c>true</c>.</param>
    /// <returns>The created <see cref="TradeOffer"/> with its generated identifier.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is <c>null</c>.</exception>
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
        if (asset is not null) ArgumentException.ThrowIfNullOrEmpty(asset);
        if (fiat is not null) ArgumentException.ThrowIfNullOrEmpty(fiat);

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
    /// <param name="count">Number of offers to create. Must be greater than zero.</param>
    /// <param name="asset">Optional asset symbol. Defaults to <c>"USDT"</c>.</param>
    /// <param name="fiat">Optional fiat currency. Defaults to <c>"USD"</c>.</param>
    /// <param name="priceRange">
    /// Optional price range in the specified currency. If supplied, <c>min</c> must be less than or equal to <c>max</c>.
    /// Defaults to a range from <c>1.0m</c> to <c>2.0m</c> when omitted.
    /// </param>
    /// <returns>A read‑only list of the created trade offers with populated identifiers.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="count"/> is not positive.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="priceRange"/> has an invalid interval.</exception>
    public static async Task<IReadOnlyList<TradeOffer>> CreateAndAddTestTradeOffersAsync(
        this TradeOfferRepositoryTests tests,
        int count,
        string? asset = null,
        string? fiat = null,
        (decimal min, decimal max)? priceRange = null)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(count, 0);
        if (asset is not null) ArgumentException.ThrowIfNullOrEmpty(asset);
        if (fiat is not null) ArgumentException.ThrowIfNullOrEmpty(fiat);
        if (priceRange.HasValue && priceRange.Value.min > priceRange.Value.max)
        {
            throw new ArgumentException("The minimum price must be less than or equal to the maximum price.", nameof(priceRange));
        }

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
    /// Verifies that a trade offer matches the expected values.
    /// </summary>
    /// <param name="actual">The actual trade offer.</param>
    /// <param name="expected">The expected trade offer values.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="actual"/> or <paramref name="expected"/> is <c>null</c>.</exception>
    public static void ShouldMatchExpectedValues(this TradeOffer actual, TradeOffer expected)
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
    /// <returns>The number of active trade offers.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is <c>null</c>.</exception>
    public static async Task<int> GetActiveOffersCountAsync(this TradeOfferRepositoryTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        var activeOffers = await tests.GetTradeOfferRepository().GetAllActiveAsync().ConfigureAwait(false);
        return activeOffers.Count();
    }
}
