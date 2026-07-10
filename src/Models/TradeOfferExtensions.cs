#nullable enable

using System;
using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Models;

/// <summary>
/// Extension methods for TradeOffer providing additional functionality
/// </summary>
public static class TradeOfferExtensions
{
    /// <summary>
    /// Determines whether this offer is better than another offer based on price and rating
    /// </summary>
    /// <param name="offer">The current offer</param>
    /// <param name="other">The other offer to compare with</param>
    /// <returns>True if this offer is better; otherwise, false.</returns>
    public static bool IsBetterThan(this TradeOffer offer, TradeOffer other)
    {
        if (other == null)
            return true;

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
    /// Gets the midpoint of the available range for this offer
    /// </summary>
    /// <param name="offer">The trade offer</param>
    /// <returns>The midpoint amount between MinAmount and MaxAmount</returns>
    public static decimal GetMidpointAmount(this TradeOffer offer)
    {
        return (offer.MinAmount + offer.MaxAmount) / 2;
    }

    /// <summary>
    /// Calculates the volume percentage of the available range that a specific amount represents
    /// </summary>
    /// <param name="offer">The trade offer</param>
    /// <param name="amount">The amount to check</param>
    /// <returns>Percentage (0-100) of the available range that the amount represents</returns>
    public static decimal GetVolumePercentage(this TradeOffer offer, decimal amount)
    {
        if (amount < offer.MinAmount || amount > offer.MaxAmount)
            return 0;

        decimal range = offer.GetAvailableRange();
        if (range == 0)
            return 100;

        decimal position = amount - offer.MinAmount;
        return (position / range) * 100;
    }

    /// <summary>
    /// Formats the payment methods string for display purposes
    /// </summary>
    /// <param name="offer">The trade offer</param>
    /// <returns>Formatted payment methods string</returns>
    public static string FormatPaymentMethods(this TradeOffer offer)
    {
        if (string.IsNullOrWhiteSpace(offer.PaymentMethods))
            return "Unknown";

        // Replace common separators with readable format
        return offer.PaymentMethods
            .Replace(";", ", ")
            .Replace("|", ", ")
            .Trim();
    }
}