#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Buffers;
using System.Collections.Frozen;
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
    // FrozenDictionary provides O(1) lookup with lower overhead than Dictionary
    // for this read-only table of per-pair spread alert thresholds.
    private static readonly FrozenDictionary<string, decimal> _pairThresholdOverrides =
        new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["BTC/USD"] = 0.5m,
            ["ETH/USD"] = 0.8m,
            ["BNB/USD"] = 1.0m,
            ["BTC/EUR"] = 0.6m,
            ["ETH/EUR"] = 0.9m,
            ["USDT/USD"] = 0.2m,
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    private readonly IPriceRepository _priceRepository;
    private readonly IPriceHistoryService _historyService;
    private readonly AppSettings _settings;
    private readonly ILogger<SpreadAnalysisService> _logger;
    private readonly Dictionary<string, Spread> _spreadCache;

    public SpreadAnalysisService(
        IPriceRepository priceRepository,
        IPriceHistoryService historyService,
        AppSettings settings,
        ILogger<SpreadAnalysisService> logger)
    {
        _priceRepository = priceRepository ?? throw new ArgumentNullException(nameof(priceRepository));
        _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
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
            if (!_spreadCache.TryGetValue(key, out var spread))
            {
                // Initialize spread with historical data if available
                var historicalPrices = await _historyService.GetHistoryAsync(asset, fiat, _settings.SpreadAnalysisHistoryHours).ConfigureAwait(false);
                if (historicalPrices != null && historicalPrices.Any())
                {
                    var historicalSpreads = historicalPrices.Select(p => p.CalculateSpread()).ToList();

                    spread = new Spread
                    {
                        Asset = asset,
                        Fiat = fiat,
                        MinSpreadPercent = historicalSpreads.Min(),
                        MaxSpreadPercent = historicalSpreads.Max(),
                        AverageSpreadPercent = historicalSpreads.Average(),
                        CurrentSpreadPercent = historicalSpreads.Last(), // Or calculate from latest price
                        SampleCount = historicalSpreads.Count,
                        LastUpdatedAt = historicalPrices.Max(p => p.Timestamp),
                        CreatedAt = historicalPrices.Min(p => p.Timestamp),
                        StandardDeviation = CalculateStandardDeviation(historicalSpreads)
                    };
                }
                else
                {
                    spread = new Spread
                    {
                        Asset = asset,
                        Fiat = fiat,
                        LastUpdatedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        SampleCount = 0
                    };
                }
                _spreadCache[key] = spread;
            }

            var latestPrice = await _priceRepository.GetLatestByAssetAndFiatAsync(asset, fiat).ConfigureAwait(false);
            if (latestPrice is null)
                return spread.SampleCount > 0 ? spread : null; // Return existing if any, else null

            var currentSpreadPercent = latestPrice.CalculateSpread();
            spread.UpdateStatistics(currentSpreadPercent);
            
            return spread;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing spread for {Asset}/{Fiat}", asset, fiat);
            throw;
        }
    }

    /// <summary>
    /// Gets top spread opportunities, applying per-pair threshold overrides where available.
    /// </summary>
    public async Task<IEnumerable<Spread>> GetTopSpreadOpportunitiesAsync(int limit = 10)
    {
        try
        {
            var spreads = await GetAllSpreadsAsync().ConfigureAwait(false);

            return spreads.Values
                .Where(s =>
                {
                    var key = $"{s.Asset}/{s.Fiat}";
                    var threshold = _pairThresholdOverrides.TryGetValue(key, out var t)
                        ? t
                        : _settings.DefaultSpreadThreshold;
                    return s.CurrentSpreadPercent > threshold;
                })
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
    /// Analyzes spread percentage. Returns ValueTask to avoid Task allocation for this synchronous path.
    /// </summary>
    public ValueTask<decimal> AnalyzeSpreadAsync(decimal buyPrice, decimal sellPrice)
    {
        try
        {
            if (buyPrice <= 0)
                throw new InvalidPriceException("Buy price must be positive");

            var spread = ((sellPrice - buyPrice) / buyPrice) * 100;
            return new ValueTask<decimal>(Math.Round(spread, 4));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing spread");
            throw;
        }
    }

    /// <summary>
    /// Updates spread analysis. Returns ValueTask to avoid Task allocation for this synchronous path.
    /// </summary>
    public ValueTask<bool> UpdateSpreadAsync(Spread spread)
    {
        try
        {
            if (spread is null || !spread.IsValid())
                throw new InvalidPriceException("Spread data is invalid");

            var key = $"{spread.Asset}/{spread.Fiat}";
            _spreadCache[key] = spread;

            _logger.LogInformation("Updated spread for {Asset}/{Fiat}: {Spread:F4}%",
                spread.Asset, spread.Fiat, spread.CurrentSpreadPercent);

            return new ValueTask<bool>(true);
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
            var prices = await _priceRepository.GetAllActiveAsync().ConfigureAwait(false);
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
    /// Calculates the population standard deviation for a list of decimal values.
    /// </summary>
    private static decimal CalculateStandardDeviation(List<decimal> values)
    {
        if (values == null || !values.Any())
            return 0;

        var mean = values.Average();
        var sumOfSquaresOfDifferences = values.Sum(v => (v - mean) * (v - mean));
        return (decimal)Math.Sqrt((double)(sumOfSquaresOfDifferences / values.Count));
    }
    public async Task<IEnumerable<(string Asset, string Fiat, decimal Spread)>> FindAnomalousSpreadAsync(decimal zScoreThreshold = 2.0m)
    {
        try
        {
            var spreads = await GetAllSpreadsAsync().ConfigureAwait(false);
            int count = spreads.Count;

            if (count == 0)
                return [];

            var pool = ArrayPool<decimal>.Shared;
            decimal[] buffer = pool.Rent(count);

            try
            {
                int idx = 0;
                foreach (var s in spreads.Values)
                    buffer[idx++] = s.CurrentSpreadPercent;

                var values = new ReadOnlySpan<decimal>(buffer, 0, count);

                decimal sum = 0;
                for (int i = 0; i < count; i++) sum += values[i];
                decimal mean = sum / count;

                decimal varianceSum = 0;
                for (int i = 0; i < count; i++)
                {
                    decimal diff = values[i] - mean;
                    varianceSum += diff * diff;
                }
                decimal stdDev = (decimal)Math.Sqrt((double)(varianceSum / count));

                var anomalies = new List<(string Asset, string Fiat, decimal Spread)>(count / 4 + 1);

                foreach (var spread in spreads.Values)
                {
                    decimal zScore = stdDev > 0
                        ? Math.Abs((spread.CurrentSpreadPercent - mean) / stdDev)
                        : 0;

                    if (zScore > zScoreThreshold)
                        anomalies.Add((spread.Asset, spread.Fiat, spread.CurrentSpreadPercent));
                }

                return anomalies;
            }
            finally
            {
                pool.Return(buffer);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding anomalous spreads");
            throw;
        }
    }
}
