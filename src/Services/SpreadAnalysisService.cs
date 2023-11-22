// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Exceptions;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using Microsoft.Extensions.Logging;

namespace BinanceP2pMonitor.Services;

/// <summary>
/// Service for analyzing spreads and finding trading opportunities
/// </summary>
public class SpreadAnalysisService : ISpreadAnalysisService
{
    private readonly IPriceRepository _priceRepository;
    private readonly AppSettings _settings;
    private readonly ILogger<SpreadAnalysisService> _logger;
    private readonly Dictionary<string, Spread> _spreadCache;

    public SpreadAnalysisService(
        IPriceRepository priceRepository,
        AppSettings settings,
        ILogger<SpreadAnalysisService> logger)
    {
        _priceRepository = priceRepository ?? throw new ArgumentNullException(nameof(priceRepository));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _spreadCache = new Dictionary<string, Spread>();
    }

    /// <summary>
    /// Gets spread analysis for a trading pair
    /// </summary>
    public async Task<Spread?> GetSpreadAnalysisAsync(string asset, string fiat)
    {
        try
        {
            var key = $"{asset}/{fiat}";

            if (_spreadCache.TryGetValue(key, out var cachedSpread))
                return cachedSpread;

            var price = await _priceRepository.GetLatestByAssetAndFiatAsync(asset, fiat);
            if (price == null)
                return null;

            var spread = new Spread
            {
                Asset = asset,
                Fiat = fiat,
                CurrentSpreadPercent = price.CalculateSpread(),
                LastUpdatedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                SampleCount = 1
            };

            _spreadCache[key] = spread;
            return spread;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing spread for {Asset}/{Fiat}", asset, fiat);
            throw;
        }
    }

    /// <summary>
    /// Gets top spread opportunities
    /// </summary>
    public async Task<IEnumerable<Spread>> GetTopSpreadOpportunitiesAsync(int limit = 10)
    {
        try
        {
            var spreads = await GetAllSpreadsAsync();

            return spreads.Values
                .Where(s => s.CurrentSpreadPercent > _settings.DefaultSpreadThreshold)
                .OrderByDescending(s => s.CurrentSpreadPercent)
                .Take(limit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top spread opportunities");
            throw;
        }
    }

    /// <summary>
    /// Analyzes spread percentage
    /// </summary>
    public async Task<decimal> AnalyzeSpreadAsync(decimal buyPrice, decimal sellPrice)
    {
        try
        {
            if (buyPrice <= 0)
                throw new InvalidPriceException("Buy price must be positive");

            var spread = ((sellPrice - buyPrice) / buyPrice) * 100;
            return await Task.FromResult(Math.Round(spread, 4));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing spread");
            throw;
        }
    }

    /// <summary>
    /// Updates spread analysis
    /// </summary>
    public async Task<bool> UpdateSpreadAsync(Spread spread)
    {
        try
        {
            if (spread == null || !spread.IsValid())
                throw new InvalidPriceException("Spread data is invalid");

            var key = $"{spread.Asset}/{spread.Fiat}";
            _spreadCache[key] = spread;

            _logger.LogInformation("Updated spread for {Asset}/{Fiat}: {Spread:F4}%",
                spread.Asset, spread.Fiat, spread.CurrentSpreadPercent);

            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating spread");
            throw;
        }
    }

    /// <summary>
    /// Gets all spreads
    /// </summary>
    public async Task<Dictionary<string, Spread>> GetAllSpreadsAsync()
    {
        try
        {
            var prices = await _priceRepository.GetAllActiveAsync();
            var spreads = new Dictionary<string, Spread>();

            foreach (var price in prices)
            {
                var key = $"{price.Asset}/{price.Fiat}";
                var spread = new Spread
                {
                    Asset = price.Asset,
                    Fiat = price.Fiat,
                    CurrentSpreadPercent = price.CalculateSpread(),
                    LastUpdatedAt = price.UpdatedAt,
                    CreatedAt = price.CreatedAt,
                    SampleCount = 1
                };

                spreads[key] = spread;
            }

            return spreads;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all spreads");
            throw;
        }
    }

    /// <summary>
    /// Finds anomalous spreads (outliers)
    /// </summary>
    public async Task<IEnumerable<(string Asset, string Fiat, decimal Spread)>> FindAnomalousSpreadAsync(decimal zScoreThreshold = 2.0m)
    {
        try
        {
            var spreads = await GetAllSpreadsAsync();

            if (spreads.Count == 0)
                return Enumerable.Empty<(string, string, decimal)>();

            var spreadValues = spreads.Values.Select(s => s.CurrentSpreadPercent).ToList();
            var mean = spreadValues.Average();
            var stdDev = CalculateStandardDeviation(spreadValues, mean);

            var anomalies = new List<(string Asset, string Fiat, decimal Spread)>();

            foreach (var spread in spreads.Values)
            {
                var zScore = stdDev > 0 ? Math.Abs((spread.CurrentSpreadPercent - mean) / stdDev) : 0;

                if (zScore > zScoreThreshold)
                {
                    anomalies.Add((spread.Asset, spread.Fiat, spread.CurrentSpreadPercent));
                }
            }

            return anomalies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding anomalous spreads");
            throw;
        }
    }

    /// <summary>
    /// Calculates standard deviation
    /// </summary>
    private decimal CalculateStandardDeviation(IEnumerable<decimal> values, decimal mean)
    {
        var count = values.Count();
        if (count < 2)
            return 0;

        var variance = values.Sum(v => (v - mean) * (v - mean)) / count;
        return (decimal)Math.Sqrt((double)variance);
    }
}
