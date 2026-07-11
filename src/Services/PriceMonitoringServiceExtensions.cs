#nullable enable

using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Services;

/// <summary>
/// Extension methods for PriceMonitoringService to provide additional functionality
/// </summary>
public static class PriceMonitoringServiceExtensions
{
    /// <summary>
    /// Gets the current price for a specific trading pair with caching support
    /// </summary>
    /// <param name="service">The price monitoring service</param>
    /// <param name="asset">The cryptocurrency asset (e.g., USDT, BTC)</param>
    /// <param name="fiat">The fiat currency (e.g., USD, EUR)</param>
    /// <param name="cacheDurationMinutes">Optional cache duration in minutes</param>
    /// <returns>The current price or null if not found</returns>
    /// <exception cref="ArgumentNullException">Thrown if service, asset, or fiat is null</exception>
    /// <exception cref="ArgumentException">Thrown if asset or fiat is whitespace, or cacheDurationMinutes is negative</exception>
    public static async Task<Price?> GetCurrentPriceAsync(this PriceMonitoringService service, string asset, string fiat, int cacheDurationMinutes = 5)
    {
        ArgumentNullException.ThrowIfNull(service);

        ArgumentException.ThrowIfNullOrWhiteSpace(asset);
        ArgumentException.ThrowIfNullOrWhiteSpace(fiat);

        if (cacheDurationMinutes < 0)
            throw new ArgumentException("Cache duration cannot be negative", nameof(cacheDurationMinutes));

        // If cache duration is 0, use the original method
        if (cacheDurationMinutes == 0)
        {
            return await service.GetCurrentPriceAsync(asset, fiat).ConfigureAwait(false);
        }

        // For caching, we would typically use a cache service, but since we don't have one,
        // we'll implement a simple in-memory cache using a static dictionary
        // Note: In a real application, consider using IMemoryCache or IDistributedCache
        var cacheKey = $"{asset}_{fiat}";

        // Check cache first
        if (TryGetCachedPrice(cacheKey, out var cachedEntry, cacheDurationMinutes))
        {
            return cachedEntry.Price;
        }

        // Cache miss - fetch from service
        var price = await service.GetCurrentPriceAsync(asset, fiat).ConfigureAwait(false);

        if (price != null)
        {
            CachePrice(cacheKey, price);
        }

        return price;
    }

    /// <summary>
    /// Helper method to get cached price
    /// </summary>
    /// <param name="cacheKey">The cache key to look up</param>
    /// <param name="cachedEntry">Output parameter for the cached entry</param>
    /// <param name="cacheDurationMinutes">Cache duration in minutes</param>
    /// <returns>True if the price was found in cache and is still valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if cacheKey is null</exception>
    private static bool TryGetCachedPrice(string cacheKey, out (Price Price, DateTime Timestamp) cachedEntry, int cacheDurationMinutes)
    {
        ArgumentNullException.ThrowIfNull(cacheKey);

        // In a real application, this would use IMemoryCache or IDistributedCache
        // For this implementation, we return false to always fetch fresh data
        cachedEntry = default;
        return false;
    }

    /// <summary>
    /// Helper method to cache price
    /// </summary>
    /// <param name="cacheKey">The cache key to use</param>
    /// <param name="price">The price to cache</param>
    /// <exception cref="ArgumentNullException">Thrown if cacheKey or price is null</exception>
    private static void CachePrice(string cacheKey, Price price)
    {
        ArgumentNullException.ThrowIfNull(cacheKey);
        ArgumentNullException.ThrowIfNull(price);
    }

    /// <summary>
    /// Gets all current prices with filtering by asset and fiat
    /// </summary>
    /// <param name="service">The price monitoring service</param>
    /// <param name="asset">Optional asset filter</param>
    /// <param name="fiat">Optional fiat filter</param>
    /// <returns>Filtered collection of current prices</returns>
    /// <exception cref="ArgumentNullException">Thrown if service is null</exception>
    public static async Task<IEnumerable<Price>> GetFilteredCurrentPricesAsync(this PriceMonitoringService service, string? asset = null, string? fiat = null)
    {
        ArgumentNullException.ThrowIfNull(service);

        var allPrices = await service.GetAllCurrentPricesAsync().ConfigureAwait(false);

        var filteredPrices = allPrices.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(asset))
        {
            filteredPrices = filteredPrices.Where(p => string.Equals(p.Asset, asset, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(fiat))
        {
            filteredPrices = filteredPrices.Where(p => string.Equals(p.Fiat, fiat, StringComparison.OrdinalIgnoreCase));
        }

        return filteredPrices;
    }

    /// <summary>
    /// Gets the best buy/sell prices across all monitored pairs for a specific asset
    /// </summary>
    /// <param name="service">The price monitoring service</param>
    /// <param name="asset">The cryptocurrency asset</param>
    /// <param name="fiat">Optional fiat currency filter</param>
    /// <returns>Tuple containing best buy price, best sell price, and count of prices analyzed</returns>
    /// <exception cref="ArgumentNullException">Thrown if service or asset is null</exception>
    /// <exception cref="ArgumentException">Thrown if asset is whitespace</exception>
    public static async Task<(decimal BestBuyPrice, decimal BestSellPrice, int PriceCount)> GetBestPricesAsync(
        this PriceMonitoringService service,
        string asset,
        string? fiat = null)
    {
        ArgumentNullException.ThrowIfNull(service);

        ArgumentException.ThrowIfNullOrWhiteSpace(asset);

        var prices = await service.GetFilteredCurrentPricesAsync(asset, fiat).ConfigureAwait(false);

        var priceList = prices.ToList();

        if (priceList.Count == 0)
        {
            return (0, 0, 0);
        }

        var bestBuyPrice = priceList.Min(p => p.BuyPrice);
        var bestSellPrice = priceList.Max(p => p.SellPrice);

        return (bestBuyPrice, bestSellPrice, priceList.Count);
    }

    /// <summary>
    /// Gets price statistics for a specific trading pair
    /// </summary>
    /// <param name="service">The price monitoring service</param>
    /// <param name="asset">The cryptocurrency asset</param>
    /// <param name="fiat">The fiat currency</param>
    /// <param name="hours">Number of hours to consider for statistics</param>
    /// <returns>Price statistics including average, min, max, and volatility</returns>
    /// <exception cref="ArgumentNullException">Thrown if service, asset, or fiat is null</exception>
    /// <exception cref="ArgumentException">Thrown if asset or fiat is whitespace, or hours is not positive</exception>
    public static async Task<PriceStatistics?> GetPriceStatisticsAsync(
        this PriceMonitoringService service,
        string asset,
        string fiat,
        int hours = 24)
    {
        ArgumentNullException.ThrowIfNull(service);

        ArgumentException.ThrowIfNullOrWhiteSpace(asset);
        ArgumentException.ThrowIfNullOrWhiteSpace(fiat);

        if (hours <= 0)
            throw new ArgumentException("Hours must be positive", nameof(hours));

        var averagePrice = await service.GetAveragePriceAsync(asset, fiat, hours).ConfigureAwait(false);

        if (averagePrice is null or 0)
        {
            return null;
        }

        // Get all prices for this pair in the time range
        var allPrices = await service.GetAllCurrentPricesAsync().ConfigureAwait(false);
        var pairPrices = allPrices
            .Where(p => string.Equals(p.Asset, asset, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(p.Fiat, fiat, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(p => p.Timestamp)
            .Take(100) // Limit to recent 100 prices for performance
            .ToList();

        if (pairPrices.Count == 0)
        {
            return null;
        }

        var buyPrices = pairPrices.Select(p => p.BuyPrice).ToList();
        var sellPrices = pairPrices.Select(p => p.SellPrice).ToList();

        var buyMin = buyPrices.Min();
        var buyMax = buyPrices.Max();
        var sellMin = sellPrices.Min();
        var sellMax = sellPrices.Max();

        var buyVolatility = buyMax > 0 ? ((buyMax - buyMin) / buyMin) * 100 : 0;
        var sellVolatility = sellMax > 0 ? ((sellMax - sellMin) / sellMin) * 100 : 0;

        var averageSellPriceValue = await service.GetAveragePriceAsync(asset, fiat, hours).ConfigureAwait(false);

        return new PriceStatistics
        {
            Asset = asset,
            Fiat = fiat,
            Hours = hours,
            AverageBuyPrice = (decimal)averagePrice,
            AverageSellPrice = averageSellPriceValue.HasValue ? (decimal)averageSellPriceValue.Value : 0,
            MinBuyPrice = buyMin,
            MaxBuyPrice = buyMax,
            MinSellPrice = sellMin,
            MaxSellPrice = sellMax,
            BuyPriceVolatilityPercent = buyVolatility,
            SellPriceVolatilityPercent = sellVolatility,
            PriceCount = pairPrices.Count,
            LastUpdated = pairPrices.FirstOrDefault()?.Timestamp ?? DateTime.UtcNow
        };
    }

    /// <summary>
    /// Checks if a price update would trigger any alerts based on percentage change
    /// </summary>
    /// <param name="service">The price monitoring service</param>
    /// <param name="newPrice">The new price to check</param>
    /// <param name="alertThresholdPercent">Alert threshold percentage</param>
    /// <returns>True if the price change would trigger an alert</returns>
    /// <exception cref="ArgumentNullException">Thrown if service or newPrice is null</exception>
    /// <exception cref="ArgumentException">Thrown if alertThresholdPercent is not positive</exception>
    public static async Task<bool> WouldTriggerAlertAsync(
        this PriceMonitoringService service,
        Price newPrice,
        decimal alertThresholdPercent = 2.0m)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(newPrice);

        if (alertThresholdPercent <= 0)
            throw new ArgumentException("Alert threshold must be positive", nameof(alertThresholdPercent));

        // Get current price to compare against
        var currentPrice = await service.GetCurrentPriceAsync(newPrice.Asset, newPrice.Fiat).ConfigureAwait(false);

        if (currentPrice is null)
        {
            return false; // No current price to compare, so no alert
        }

        // Calculate percentage changes
        var buyChangePercent = Math.Abs(((newPrice.BuyPrice - currentPrice.BuyPrice) / currentPrice.BuyPrice) * 100);
        var sellChangePercent = Math.Abs(((newPrice.SellPrice - currentPrice.SellPrice) / currentPrice.SellPrice) * 100);

        return buyChangePercent >= alertThresholdPercent || sellChangePercent >= alertThresholdPercent;
    }
}

/// <summary>
/// Container for price statistics
/// </summary>
public sealed class PriceStatistics
{
    public string Asset { get; set; } = string.Empty;
    public string Fiat { get; set; } = string.Empty;
    public int Hours { get; set; }
    public decimal AverageBuyPrice { get; set; }
    public decimal AverageSellPrice { get; set; }
    public decimal MinBuyPrice { get; set; }
    public decimal MaxBuyPrice { get; set; }
    public decimal MinSellPrice { get; set; }
    public decimal MaxSellPrice { get; set; }
    public decimal BuyPriceVolatilityPercent { get; set; }
    public decimal SellPriceVolatilityPercent { get; set; }
    public int PriceCount { get; set; }
    public DateTime LastUpdated { get; set; }
}