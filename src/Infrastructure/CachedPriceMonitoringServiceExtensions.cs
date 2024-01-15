#nullable enable

using BinanceP2pMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinanceP2pMonitor.Infrastructure;

/// <summary>
/// Extension methods for CachedPriceMonitoringService providing additional functionality
/// </summary>
public static class CachedPriceMonitoringServiceExtensions
{
    /// <summary>
    /// Gets the current price for a specific asset-fiat pair with automatic retry logic
    /// </summary>
    /// <param name="service">The cached price monitoring service</param>
    /// <param name="asset">The cryptocurrency asset (e.g., "USDT", "BTC")</param>
    /// <param name="fiat">The fiat currency (e.g., "USD", "EUR")</param>
    /// <param name="maxRetries">Maximum number of retry attempts (default: 3)</param>
    /// <returns>The current price or null if not available</returns>
    public static async Task<Price?> GetCurrentPriceAsyncWithRetry(
        this CachedPriceMonitoringService service,
        string asset,
        string fiat,
        int maxRetries = 3)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));
        if (maxRetries < 0)
            throw new ArgumentException("Max retries cannot be negative", nameof(maxRetries));

        Exception? lastException = null;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await service.GetCurrentPriceAsync(asset, fiat).ConfigureAwait(false);
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                lastException = ex;
                // Wait before retrying (exponential backoff)
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), default(CancellationToken)).ConfigureAwait(false);
            }
        }

        throw new InvalidOperationException(
            $"Failed to get price after {maxRetries} retries for {asset}/{fiat}",
            lastException);
    }

    /// <summary>
    /// Gets all current prices and filters by minimum buy price threshold
    /// </summary>
    /// <param name="service">The cached price monitoring service</param>
    /// <param name="minBuyPrice">Minimum buy price threshold (default: 1000)</param>
    /// <returns>Filtered collection of prices with sufficient buy price</returns>
    public static async Task<IEnumerable<Price>> GetPricesWithMinBuyPriceAsync(
        this CachedPriceMonitoringService service,
        decimal minBuyPrice = 1000m)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));
        if (minBuyPrice < 0)
            throw new ArgumentException("Minimum buy price cannot be negative", nameof(minBuyPrice));

        var allPrices = await service.GetAllCurrentPricesAsync().ConfigureAwait(false);
        return allPrices.Where(p => p.BuyPrice >= minBuyPrice)
                       .OrderByDescending(p => p.BuyPrice);
    }

    /// <summary>
    /// Gets spread analysis with automatic threshold evaluation
    /// </summary>
    /// <param name="service">The cached price monitoring service</param>
    /// <param name="asset">The cryptocurrency asset</param>
    /// <param name="fiat">The fiat currency</param>
    /// <param name="isHighThreshold">Threshold for considering spread as high (default: 1.5%)</param>
    /// <param name="isLowThreshold">Threshold for considering spread as low (default: 0.3%)</param>
    /// <returns>Spread analysis with risk assessment</returns>
    public static async Task<(Spread Spread, bool IsHigh, bool IsLow, string RiskLevel)> GetSpreadAnalysisWithRiskAsync(
        this CachedPriceMonitoringService service,
        string asset,
        string fiat,
        decimal isHighThreshold = 1.5m,
        decimal isLowThreshold = 0.3m)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));
        if (isHighThreshold <= 0 || isLowThreshold <= 0)
            throw new ArgumentException("Thresholds must be positive");
        if (isHighThreshold <= isLowThreshold)
            throw new ArgumentException("High threshold must be greater than low threshold");

        var spread = await service.GetSpreadAnalysisAsync(asset, fiat).ConfigureAwait(false);

        if (spread is null)
            throw new InvalidOperationException("Spread analysis returned null");

        bool isHigh = spread.CurrentSpreadPercent > isHighThreshold;
        bool isLow = spread.CurrentSpreadPercent < isLowThreshold;
        string riskLevel = spread.GetRiskLevel();

        return (spread, isHigh, isLow, riskLevel);
    }

    /// <summary>
    /// Gets prices with significant change compared to average over a time period
    /// </summary>
    /// <param name="service">The cached price monitoring service</param>
    /// <param name="changePercentThreshold">The percentage change threshold (e.g., 2.0 for 2%)</param>
    /// <param name="hours">Time period in hours to compare against</param>
    /// <returns>Collection of prices with significant changes</returns>
    public static async Task<IEnumerable<Price>> GetPricesWithSignificantChangeAsync(
        this CachedPriceMonitoringService service,
        decimal changePercentThreshold,
        int hours)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));
        if (changePercentThreshold < 0)
            throw new ArgumentException("Threshold cannot be negative", nameof(changePercentThreshold));
        if (hours <= 0)
            throw new ArgumentException("Hours must be positive", nameof(hours));

        var currentPrices = await service.GetAllCurrentPricesAsync().ConfigureAwait(false);

        // Calculate percentage changes and filter
        var results = await Task.WhenAll(currentPrices.Select(async current =>
        {
            var avgPrice = await service.GetAveragePriceAsync(current.Asset, current.Fiat, hours)
                .ConfigureAwait(false);

            if (!avgPrice.HasValue || avgPrice.Value == 0)
                return null;

            // Calculate average price change percentage
            decimal changePercent = ((current.BuyPrice - avgPrice.Value) / avgPrice.Value) * 100;

            return new
            {
                Price = current,
                ChangePercent = changePercent,
                IsSignificant = Math.Abs(changePercent) >= changePercentThreshold
            };
        })).ConfigureAwait(false);

        return results.Where(x => x?.IsSignificant == true).Select(x => x!.Price);
    }
}