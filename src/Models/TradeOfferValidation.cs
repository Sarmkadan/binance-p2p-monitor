#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;

namespace BinanceP2pMonitor.Models;

/// <summary>
/// Provides validation helpers for <see cref="TradeOffer"/> instances
/// </summary>
public static class TradeOfferValidation
{
    /// <summary>
    /// Validates a <see cref="TradeOffer"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The trade offer to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if the offer is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this TradeOffer value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate string properties
        if (string.IsNullOrWhiteSpace(value.OfferIdFromBinance))
        {
            errors.Add("OfferIdFromBinance must not be null or whitespace.");
        }
        else if (value.OfferIdFromBinance.Length > 100)
        {
            errors.Add("OfferIdFromBinance must not exceed 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(value.Asset))
        {
            errors.Add("Asset must not be null or whitespace.");
        }
        else if (value.Asset.Length > 20)
        {
            errors.Add("Asset must not exceed 20 characters.");
        }

        if (string.IsNullOrWhiteSpace(value.Fiat))
        {
            errors.Add("Fiat must not be null or whitespace.");
        }
        else if (value.Fiat.Length > 10)
        {
            errors.Add("Fiat must not exceed 10 characters.");
        }

        if (string.IsNullOrWhiteSpace(value.PaymentMethods) && value.IsActive)
        {
            errors.Add("PaymentMethods must not be null or whitespace when offer is active.");
        }

        // Validate numeric properties
        if (value.Price <= 0m)
        {
            errors.Add("Price must be greater than zero.");
        }

        if (value.MinAmount <= 0m)
        {
            errors.Add("MinAmount must be greater than zero.");
        }

        if (value.MaxAmount <= 0m)
        {
            errors.Add("MaxAmount must be greater than zero.");
        }
        else if (value.MaxAmount < value.MinAmount)
        {
            errors.Add("MaxAmount must be greater than or equal to MinAmount.");
        }

        if (value.TraderRating is < 0m or > 100m)
        {
            errors.Add("TraderRating must be between 0 and 100 inclusive.");
        }

        if (value.CompletedTrades < 0)
        {
            errors.Add("CompletedTrades must be non-negative.");
        }

        // Validate date properties
        if (value.Timestamp == default)
        {
            errors.Add("Timestamp must be set to a valid DateTime.");
        }

        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt must be set to a valid DateTime.");
        }

        if (value.UpdatedAt == default)
        {
            errors.Add("UpdatedAt must be set to a valid DateTime.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="TradeOffer"/> instance is valid.
    /// </summary>
    /// <param name="value">The trade offer to check.</param>
    /// <returns>True if the offer is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this TradeOffer value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that a <see cref="TradeOffer"/> instance is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The trade offer to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the offer contains validation errors.</exception>
    public static void EnsureValid(this TradeOffer value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"TradeOffer validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}