#nullable enable

using System;
using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Models;

/// <summary>
/// Extension methods for <see cref="TradeOffer"/> providing additional functionality for trade offer analysis and formatting.
/// </summary>
public static class TradeOfferExtensions
{
    /// <summary>
    /// Determines whether this offer is better than another offer based on price and rating.
    /// </summary>
    /// <param name="offer">The current trade offer.</param>
    /// <param name="other">The other offer to compare with.</param>
    /// <returns>True if this offer is better; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="other"/> is <see langword="null"/>.</exception>
    public static bool IsBetterThan(this TradeOffer offer, TradeOffer? other)
    {
        ArgumentNullException.ThrowIfNull(offer);
        ArgumentNullException.ThrowIfNull(other);

        // Compare by price first (lower is better for buyers, higher is better for sellers)
        int priceComparison = offer.TradeType == TradeType.Buy
            ? other.Price.CompareTo(offer.Price)
            : offer.Price.CompareTo(other.Price);

        if (priceComparison != 0)
            return priceComparison < 0;

        // If prices are equal, compare by trader rating (higher is better)
        return offer.TraderRating > other.TraderRating;
    }

    /// <summary>
    /// Gets the midpoint of the available range for this offer.
    /// </summary>
    /// <param name="offer">The trade offer.</param>
    /// <returns>The midpoint amount between <see cref="TradeOffer.MinAmount"/> and <see cref="TradeOffer.MaxAmount"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="offer"/> is <see langword="null"/>.</exception>
    public static decimal GetMidpointAmount(this TradeOffer offer)
    {
        ArgumentNullException.ThrowIfNull(offer);

        return (offer.MinAmount + offer.MaxAmount) / 2;
    }

    /// <summary>
    /// Calculates the volume percentage of the available range that a specific amount represents.
    /// </summary>
    /// <param name="offer">The trade offer.</param>
    /// <param name="amount">The amount to check.</param>
    /// <returns>Percentage (0-100) of the available range that the amount represents.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="offer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="amount"/> is negative.</exception>
    public static decimal GetVolumePercentage(this TradeOffer offer, decimal amount)
    {
        ArgumentNullException.ThrowIfNull(offer);
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        if (amount < offer.MinAmount || amount > offer.MaxAmount)
            return 0;

        decimal range = offer.GetAvailableRange();
        if (range == 0)
            return 100;

        decimal position = amount - offer.MinAmount;
        return (position / range) * 100;
    }

    /// <summary>
    /// Formats the payment methods string for display purposes.
    /// </summary>
    /// <param name="offer">The trade offer.</param>
    /// <returns>Formatted payment methods string. Returns "Unknown" if payment methods are not specified.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="offer"/> is <see langword="null"/>.</exception>
    public static string FormatPaymentMethods(this TradeOffer offer)
    {
        ArgumentNullException.ThrowIfNull(offer);

        if (string.IsNullOrWhiteSpace(offer.PaymentMethods))
            return "Unknown";

        // Replace common separators with readable format
        return offer.PaymentMethods
            .Replace(";", ", ")
            .Replace("|", ", ")
            .Trim();
    }
}