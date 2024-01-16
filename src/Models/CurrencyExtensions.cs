#nullable enable

using System;
using System.Globalization;

namespace BinanceP2pMonitor.Models;

/// <summary>
/// Extension methods for Currency providing additional functionality
/// </summary>
public static class CurrencyExtensions
{
    /// <summary>
    /// Formats a currency value with currency symbol and proper formatting
    /// </summary>
    /// <param name="currency">The currency instance</param>
    /// <param name="value">The value to format</param>
    /// <returns>Formatted string with currency symbol</returns>
    public static string FormatCurrencyValue(this Currency currency, decimal value)
    {
        var roundedValue = currency.RoundValue(value);
        var format = currency.GetDisplayFormat();

        if (!string.IsNullOrWhiteSpace(currency.Symbol))
        {
            return currency.Symbol + roundedValue.ToString("N" + currency.DecimalPlaces);
        }

        return format + " " + roundedValue.ToString("N" + currency.DecimalPlaces);
    }

    /// <summary>
    /// Gets a display string suitable for UI elements showing currency information
    /// </summary>
    /// <param name="currency">The currency instance</param>
    /// <returns>Formatted display string</returns>
    public static string GetCurrencyDisplay(this Currency currency)
    {
        if (!string.IsNullOrWhiteSpace(currency.Symbol))
        {
            return currency.Name + " (" + currency.Symbol + ") [" + currency.Code + "]";
        }

        return currency.Name + " [" + currency.Code + "]";
    }

    /// <summary>
    /// Checks if currency is more popular than another currency
    /// </summary>
    /// <param name="currency">The currency instance</param>
    /// <param name="other">Other currency to compare with</param>
    /// <returns>True if this currency is more popular</returns>
    public static bool IsMorePopularThan(this Currency currency, Currency other)
    {
        return currency.PopularityScore > other.PopularityScore;
    }

    /// <summary>
    /// Gets a CSS class name based on popularity tier for styling
    /// </summary>
    /// <param name="currency">The currency instance</param>
    /// <returns>CSS class name</returns>
    public static string GetPopularityCssClass(this Currency currency)
    {
        return currency.GetPopularityTier().ToLowerInvariant() switch
        {
            "premium" => "currency-premium",
            "popular" => "currency-popular",
            "standard" => "currency-standard",
            "niche" => "currency-niche",
            _ => "currency-rare"
        };
    }

    /// <summary>
    /// Gets a short display name suitable for dropdowns and lists
    /// </summary>
    /// <param name="currency">The currency instance</param>
    /// <returns>Short display string</returns>
    public static string GetShortDisplayName(this Currency currency)
    {
        if (!string.IsNullOrWhiteSpace(currency.Symbol))
        {
            return currency.Code + " " + currency.Symbol;
        }

        return currency.Code;
    }

    /// <summary>
    /// Determines if currency should be highlighted in UI based on multiple factors
    /// </summary>
    /// <param name="currency">The currency instance</param>
    /// <returns>True if currency should be highlighted</returns>
    public static bool ShouldHighlight(this Currency currency)
    {
        return currency.IsActive && currency.IsPopular() && currency.PopularityScore >= 80;
    }
}