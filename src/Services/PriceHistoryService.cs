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
/// Service for managing price history and trend analysis
/// </summary>
public class PriceHistoryService : IPriceHistoryService
{
    private readonly IHistoryRepository _historyRepository;
    private readonly AppSettings _settings;
    private readonly ILogger<PriceHistoryService> _logger;

    public PriceHistoryService(
        IHistoryRepository historyRepository,
        AppSettings settings,
        ILogger<PriceHistoryService> logger)
    {
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Records a price in history
    /// </summary>
    public async Task<int> RecordPriceAsync(Price price)
    {
        try
        {
            if (price == null || !price.IsValid())
                throw new InvalidPriceException("Price data is invalid for recording");

            var history = new PriceHistory
            {
                PriceId = price.Id,
                Asset = price.Asset,
                Fiat = price.Fiat,
                BuyPrice = price.BuyPrice,
                SellPrice = price.SellPrice,
                RecordedAt = price.Timestamp,
                CreatedAt = DateTime.UtcNow,
                SpreadPercentage = price.CalculateSpread(),
                PriceChangePercent = price.BuyChangePercent
            };

            return await _historyRepository.AddAsync(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording price history for {Asset}/{Fiat}", price?.Asset, price?.Fiat);
            throw;
        }
    }

    /// <summary>
    /// Gets price history for a trading pair
    /// </summary>
    public async Task<IEnumerable<PriceHistory>> GetHistoryAsync(string asset, string fiat, int hours = 24)
    {
        try
        {
            return await _historyRepository.GetHistoryByAssetAndFiatAsync(asset, fiat, hours);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving price history for {Asset}/{Fiat}", asset, fiat);
            throw;
        }
    }

    /// <summary>
    /// Analyzes price trend
    /// </summary>
    public async Task<decimal> GetPriceTrendAsync(string asset, string fiat, int hours)
    {
        try
        {
            var history = await GetHistoryAsync(asset, fiat, hours);
            var records = history.OrderBy(h => h.RecordedAt).ToList();

            if (records.Count < 2)
                return 0;

            var firstRecord = records.First();
            var lastRecord = records.Last();

            var firstPrice = (firstRecord.BuyPrice + firstRecord.SellPrice) / 2;
            var lastPrice = (lastRecord.BuyPrice + lastRecord.SellPrice) / 2;

            if (firstPrice == 0)
                return 0;

            return ((lastPrice - firstPrice) / firstPrice) * 100;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating price trend for {Asset}/{Fiat}", asset, fiat);
            throw;
        }
    }

    /// <summary>
    /// Gets price statistics
    /// </summary>
    public async Task<(decimal High, decimal Low, decimal Average)> GetPriceStatsAsync(string asset, string fiat, int hours)
    {
        try
        {
            var history = await GetHistoryAsync(asset, fiat, hours);
            var records = history.ToList();

            if (records.Count == 0)
                return (0, 0, 0);

            var prices = records.SelectMany(r => new[] { r.BuyPrice, r.SellPrice }).ToList();

            return (
                prices.Max(),
                prices.Min(),
                (decimal)prices.Average()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating price statistics for {Asset}/{Fiat}", asset, fiat);
            throw;
        }
    }

    /// <summary>
    /// Cleans up old history records
    /// </summary>
    public async Task<bool> CleanupOldHistoryAsync(int daysOld)
    {
        try
        {
            return await _historyRepository.DeleteOldRecordsAsync(daysOld);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old history records");
            throw;
        }
    }

    /// <summary>
    /// Gets total history count
    /// </summary>
    public async Task<long> GetHistoryCountAsync()
    {
        try
        {
            return await _historyRepository.GetTotalHistoryCountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting history count");
            throw;
        }
    }

    /// <summary>
    /// Gets detailed analysis for a trading pair
    /// </summary>
    public async Task<Dictionary<string, object>> GetDetailedAnalysisAsync(string asset, string fiat, int hours = 24)
    {
        try
        {
            var history = await GetHistoryAsync(asset, fiat, hours);
            var (high, low, average) = await GetPriceStatsAsync(asset, fiat, hours);
            var trend = await GetPriceTrendAsync(asset, fiat, hours);

            var records = history.ToList();
            var spreadValues = records.Select(r => r.SpreadPercentage ?? 0).ToList();

            return new Dictionary<string, object>
            {
                { "Asset", asset },
                { "Fiat", fiat },
                { "HighPrice", high },
                { "LowPrice", low },
                { "AveragePrice", average },
                { "Trend", trend },
                { "TrendDirection", trend > 0 ? "Up" : trend < 0 ? "Down" : "Stable" },
                { "RecordCount", records.Count },
                { "TimeSpanHours", hours },
                { "HighestSpread", spreadValues.Any() ? spreadValues.Max() : 0 },
                { "LowestSpread", spreadValues.Any() ? spreadValues.Min() : 0 },
                { "AverageSpread", spreadValues.Any() ? spreadValues.Average() : 0 }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting detailed analysis for {Asset}/{Fiat}", asset, fiat);
            throw;
        }
    }
}
