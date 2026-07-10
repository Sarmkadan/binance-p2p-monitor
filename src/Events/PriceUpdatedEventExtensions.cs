#nullable enable

using System.Globalization;

namespace BinanceP2pMonitor.Events;

/// <summary>
/// Extension methods for <see cref="PriceUpdatedEvent"/> that provide additional functionality
/// for working with price update events.
/// </summary>
public static class PriceUpdatedEventExtensions
{
    /// <summary>
    /// Calculates the price change percentage between the current and previous buy prices.
    /// </summary>
    /// <param name="event">The price updated event.</param>
    /// <returns>The percentage change from previous buy price to current buy price, or 0 if previous price is 0.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
    public static decimal GetBuyPriceChangePercentage(this PriceUpdatedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (@event.PreviousBuyPrice == 0)
        {
            return 0;
        }

        return ((@event.BuyPrice - @event.PreviousBuyPrice) / @event.PreviousBuyPrice) * 100;
    }

    /// <summary>
    /// Calculates the price change percentage between the current and previous sell prices.
    /// </summary>
    /// <param name="event">The price updated event.</param>
    /// <returns>The percentage change from previous sell price to current sell price, or 0 if previous price is 0.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
    public static decimal GetSellPriceChangePercentage(this PriceUpdatedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (@event.PreviousSellPrice == 0)
        {
            return 0;
        }

        return ((@event.SellPrice - @event.PreviousSellPrice) / @event.PreviousSellPrice) * 100;
    }

    /// <summary>
    /// Determines whether the buy price has increased compared to the previous buy price.
    /// </summary>
    /// <param name="event">The price updated event.</param>
    /// <returns>True if the buy price has increased; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
    public static bool HasBuyPriceIncreased(this PriceUpdatedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return @event.BuyPrice > @event.PreviousBuyPrice;
    }

    /// <summary>
    /// Determines whether the sell price has increased compared to the previous sell price.
    /// </summary>
    /// <param name="event">The price updated event.</param>
    /// <returns>True if the sell price has increased; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
    public static bool HasSellPriceIncreased(this PriceUpdatedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return @event.SellPrice > @event.PreviousSellPrice;
    }

    /// <summary>
    /// Gets a formatted string representing the asset and fiat currency pair.
    /// </summary>
    /// <param name="event">The price updated event.</param>
    /// <returns>A string in format "{Asset}/{Fiat}".</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
    public static string GetCurrencyPair(this PriceUpdatedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return $"{@event.Asset}/{@event.Fiat}";
    }

    /// <summary>
    /// Calculates the price spread (difference) between buy and sell prices.
    /// </summary>
    /// <param name="event">The price updated event.</param>
    /// <returns>The absolute difference between sell and buy prices.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
    public static decimal GetPriceSpread(this PriceUpdatedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return @event.SellPrice - @event.BuyPrice;
    }

    /// <summary>
    /// Determines whether the price spread exceeds a specified threshold.
    /// </summary>
    /// <param name="event">The price updated event.</param>
    /// <param name="threshold">The maximum allowed spread threshold.</param>
    /// <returns>True if the spread exceeds the threshold; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
    public static bool HasSpreadExceededThreshold(this PriceUpdatedEvent @event, decimal threshold)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return @event.GetPriceSpread() > threshold;
    }

    /// <summary>
    /// Gets a value indicating whether there are active buy offers.
    /// </summary>
    /// <param name="event">The price updated event.</param>
    /// <returns>True if there are buy offers available; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
    public static bool HasActiveBuyOffers(this PriceUpdatedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return @event.BuyOfferCount > 0;
    }

    /// <summary>
    /// Gets a value indicating whether there are active sell offers.
    /// </summary>
    /// <param name="event">The price updated event.</param>
    /// <returns>True if there are sell offers available; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
    public static bool HasActiveSellOffers(this PriceUpdatedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return @event.SellOfferCount > 0;
    }

    /// <summary>
    /// Gets a formatted string showing the offer counts for both buy and sell sides.
    /// </summary>
    /// <param name="event">The price updated event.</param>
    /// <returns>A string in format "Buy: {BuyOfferCount} | Sell: {SellOfferCount}".</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
    public static string GetOfferCountsSummary(this PriceUpdatedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return $"Buy: {@event.BuyOfferCount} | Sell: {@event.SellOfferCount}";
    }

    /// <summary>
    /// Creates a deep copy of the price updated event.
    /// </summary>
    /// <param name="event">The price updated event to copy.</param>
    /// <returns>A new <see cref="PriceUpdatedEvent"/> instance with the same property values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
    public static PriceUpdatedEvent DeepCopy(this PriceUpdatedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return new PriceUpdatedEvent
        {
            Asset = @event.Asset,
            Fiat = @event.Fiat,
            BuyPrice = @event.BuyPrice,
            SellPrice = @event.SellPrice,
            PreviousBuyPrice = @event.PreviousBuyPrice,
            PreviousSellPrice = @event.PreviousSellPrice,
            BuyOfferCount = @event.BuyOfferCount,
            SellOfferCount = @event.SellOfferCount
        };
    }

    /// <summary>
    /// Determines whether the price update indicates significant market movement.
    /// </summary>
    /// <param name="event">The price updated event.</param>
    /// <param name="thresholdPercentage">The minimum percentage change to consider significant (e.g., 5 for 5%).</param>
    /// <returns>True if either buy or sell price changed by at least the threshold percentage; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
    public static bool HasSignificantPriceMovement(this PriceUpdatedEvent @event, decimal thresholdPercentage)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var buyChange = Math.Abs(@event.GetBuyPriceChangePercentage());
        var sellChange = Math.Abs(@event.GetSellPriceChangePercentage());

        return buyChange >= thresholdPercentage || sellChange >= thresholdPercentage;
    }
}