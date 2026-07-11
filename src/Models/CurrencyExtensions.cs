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
    /// <exception cref="ArgumentNullException"><paramref name="currency"/> is <see langword="null"/></exception>
    public static string FormatCurrencyValue(this Currency currency, decimal value)
    {
        ArgumentNullException.ThrowIfNull(currency);

        var roundedValue = currency.RoundValue(value);
        var format = currency.GetDisplayFormat();
        var decimalPlaces = currency.DecimalPlaces;

        return !string.IsNullOrWhiteSpace(currency.Symbol)
            ? currency.Symbol + roundedValue.ToString("N" + decimalPlaces, CultureInfo.InvariantCulture)
            : format + " " + roundedValue.ToString("N" + decimalPlaces, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Gets a display string suitable for UI elements showing currency information
    /// </summary>
    /// <param name="currency">The currency instance</param>
    /// <returns>Formatted display string</returns>
    /// <exception cref="ArgumentNullException"><paramref name="currency"/> is <see langword="null"/></exception>
    public static string GetCurrencyDisplay(this Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);

        return !string.IsNullOrWhiteSpace(currency.Symbol)
            ? $"{currency.Name} ({currency.Symbol}) [{currency.Code}]"
            : $"{currency.Name} [{currency.Code}]";
    }

    /// <summary>
    /// Checks if currency is more popular than another currency
    /// </summary>
    /// <param name="currency">The currency instance</param>
    /// <param name="other">Other currency to compare with</param>
    /// <returns>True if this currency is more popular</returns>
    /// <exception cref="ArgumentNullException"><paramref name="currency"/> or <paramref name="other"/> is <see langword="null"/></exception>
    public static bool IsMorePopularThan(this Currency currency, Currency? other)
    {
        ArgumentNullException.ThrowIfNull(currency);
        ArgumentNullException.ThrowIfNull(other);

        return currency.PopularityScore > other.PopularityScore;
    }

    /// <summary>
    /// Gets a CSS class name based on popularity tier for styling
    /// </summary>
    /// <param name="currency">The currency instance</param>
    /// <returns>CSS class name</returns>
    /// <exception cref="ArgumentNullException"><paramref name="currency"/> is <see langword="null"/></exception>
    public static string GetPopularityCssClass(this Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);

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
    /// <exception cref="ArgumentNullException"><paramref name="currency"/> is <see langword="null"/></exception>
    public static string GetShortDisplayName(this Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);

        return !string.IsNullOrWhiteSpace(currency.Symbol)
            ? $"{currency.Code} {currency.Symbol}"
            : currency.Code;
    }

    /// <summary>
    /// Determines if currency should be highlighted in UI based on multiple factors
    /// </summary>
    /// <param name="currency">The currency instance</param>
    /// <returns>True if currency should be highlighted</returns>
    /// <exception cref="ArgumentNullException"><paramref name="currency"/> is <see langword="null"/></exception>
    public static bool ShouldHighlight(this Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);

        return currency.IsActive && currency.IsPopular() && currency.PopularityScore >= 80;
    }
}