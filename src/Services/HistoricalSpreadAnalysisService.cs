// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Events;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using Microsoft.Extensions.Logging;

namespace BinanceP2pMonitor.Services;

/// <summary>
/// Computes statistical metrics over historical spread data and raises anomaly alerts
/// </summary>
public interface IHistoricalSpreadAnalysisService
{
    /// <summary>
    /// Builds a full statistical report for the given asset/fiat pair over the specified time window
    /// </summary>
    Task<SpreadStatisticsReport?> AnalyzeHistoricalSpreadAsync(
        string asset, string fiat, int hours = 24, CancellationToken ct = default);

    /// <summary>
    /// Scans the supplied pairs, flags those whose current spread exceeds the Z-score threshold,
    /// and publishes a <see cref="SpreadAlertTriggeredEvent"/> for each anomaly detected
    /// </summary>
    Task<IEnumerable<SpreadStatisticsReport>> DetectStatisticalAlertsAsync(
        IEnumerable<(string Asset, string Fiat)> pairs,
        decimal zScoreThreshold = 2.5m,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the spread value at the requested percentile (0–100) from recent history
    /// </summary>
    Task<decimal> GetSpreadPercentileAsync(
        string asset, string fiat, decimal percentile, int hours = 24, CancellationToken ct = default);

    /// <summary>
    /// Returns a time-ordered sequence of rolling-window average spreads
    /// </summary>
    Task<IEnumerable<(DateTime WindowEnd, decimal AverageSpread)>> GetRollingWindowAveragesAsync(
        string asset, string fiat, int windowSizeMinutes = 15, int hours = 24, CancellationToken ct = default);
}

/// <summary>
/// Default implementation of <see cref="IHistoricalSpreadAnalysisService"/>
/// </summary>
public class HistoricalSpreadAnalysisService : IHistoricalSpreadAnalysisService
{
    private readonly IHistoryRepository _historyRepository;
    private readonly ISpreadAnalysisService _spreadAnalysisService;
    private readonly IEventBus _eventBus;
    private readonly AppSettings _settings;
    private readonly ILogger<HistoricalSpreadAnalysisService> _logger;

    public HistoricalSpreadAnalysisService(
        IHistoryRepository historyRepository,
        ISpreadAnalysisService spreadAnalysisService,
        IEventBus eventBus,
        AppSettings settings,
        ILogger<HistoricalSpreadAnalysisService> logger)
    {
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
        _spreadAnalysisService = spreadAnalysisService ?? throw new ArgumentNullException(nameof(spreadAnalysisService));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Builds a full statistical report for the given asset/fiat pair over the specified time window
    /// </summary>
    public async Task<SpreadStatisticsReport?> AnalyzeHistoricalSpreadAsync(
        string asset, string fiat, int hours = 24, CancellationToken ct = default)
    {
        try
        {
            var history = (await _historyRepository.GetHistoryByAssetAndFiatAsync(asset, fiat, hours))
                .OrderBy(h => h.RecordedAt)
                .ToList();

            if (history.Count == 0)
            {
                _logger.LogWarning("No historical data for {Asset}/{Fiat} in the last {Hours}h", asset, fiat, hours);
                return null;
            }

            var sortedSpreads = history.Select(h => h.SpreadPercentage).OrderBy(s => s).ToList();
            var currentSpreadData = await _spreadAnalysisService.GetSpreadAnalysisAsync(asset, fiat);
            var currentSpread = currentSpreadData?.CurrentSpreadPercent ?? sortedSpreads.Last();

            var mean = sortedSpreads.Average();
            var stdDev = CalculateStandardDeviation(sortedSpreads, mean);

            var report = new SpreadStatisticsReport
            {
                Asset = asset,
                Fiat = fiat,
                TimeWindowHours = hours,
                SampleCount = sortedSpreads.Count,
                Mean = Math.Round(mean, 4),
                Median = CalculatePercentile(sortedSpreads, 50),
                StandardDeviation = Math.Round(stdDev, 4),
                Variance = Math.Round(stdDev * stdDev, 4),
                MinSpread = sortedSpreads.Min(),
                MaxSpread = sortedSpreads.Max(),
                Percentile5 = CalculatePercentile(sortedSpreads, 5),
                Percentile95 = CalculatePercentile(sortedSpreads, 95),
                CurrentSpread = currentSpread,
                ZScore = stdDev > 0 ? Math.Round((currentSpread - mean) / stdDev, 4) : 0,
                TrendSlope = CalculateTrendSlope(history),
                AnalyzedAt = DateTime.UtcNow,
                IsAnomalous = stdDev > 0 && Math.Abs((currentSpread - mean) / stdDev) >= 2.0m
            };

            _logger.LogInformation(
                "Spread analysis for {Asset}/{Fiat}: mean={Mean:F4}%, stdDev={StdDev:F4}%, zScore={ZScore:F2}, anomalous={Anomalous}",
                asset, fiat, report.Mean, report.StandardDeviation, report.ZScore, report.IsAnomalous);

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing historical spread for {Asset}/{Fiat}", asset, fiat);
            throw;
        }
    }

    /// <summary>
    /// Scans the supplied pairs, flags those whose current spread exceeds the Z-score threshold,
    /// and publishes a <see cref="SpreadAlertTriggeredEvent"/> for each anomaly detected
    /// </summary>
    public async Task<IEnumerable<SpreadStatisticsReport>> DetectStatisticalAlertsAsync(
        IEnumerable<(string Asset, string Fiat)> pairs,
        decimal zScoreThreshold = 2.5m,
        CancellationToken ct = default)
    {
        try
        {
            var analysisTasks = pairs.Select(p => AnalyzeHistoricalSpreadAsync(p.Asset, p.Fiat, ct: ct));
            var results = await Task.WhenAll(analysisTasks);

            var anomalies = results
                .Where(r => r != null && Math.Abs(r.ZScore) >= zScoreThreshold)
                .Select(r => r!)
                .ToList();

            var alertTasks = anomalies.Select(report => _eventBus.PublishAsync(new SpreadAlertTriggeredEvent
            {
                Asset = report.Asset,
                Fiat = report.Fiat,
                SpreadPercentage = report.CurrentSpread,
                Threshold = _settings.DefaultSpreadThreshold
            }, ct));

            await Task.WhenAll(alertTasks);

            if (anomalies.Count > 0)
                _logger.LogWarning(
                    "Detected {Count} anomalous spread(s) with |Z-score| >= {Threshold}",
                    anomalies.Count, zScoreThreshold);

            return anomalies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting statistical spread alerts");
            throw;
        }
    }

    /// <summary>
    /// Returns the spread value at the requested percentile (0–100) from recent history
    /// </summary>
    public async Task<decimal> GetSpreadPercentileAsync(
        string asset, string fiat, decimal percentile, int hours = 24, CancellationToken ct = default)
    {
        try
        {
            if (percentile is < 0 or > 100)
                throw new ArgumentOutOfRangeException(nameof(percentile), "Percentile must be between 0 and 100");

            var history = (await _historyRepository.GetHistoryByAssetAndFiatAsync(asset, fiat, hours)).ToList();

            if (history.Count == 0)
                return 0;

            var sorted = history.Select(h => h.SpreadPercentage).OrderBy(s => s).ToList();
            return CalculatePercentile(sorted, percentile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error computing {Percentile}th percentile spread for {Asset}/{Fiat}", percentile, asset, fiat);
            throw;
        }
    }

    /// <summary>
    /// Returns a time-ordered sequence of rolling-window average spreads
    /// </summary>
    public async Task<IEnumerable<(DateTime WindowEnd, decimal AverageSpread)>> GetRollingWindowAveragesAsync(
        string asset, string fiat, int windowSizeMinutes = 15, int hours = 24, CancellationToken ct = default)
    {
        try
        {
            var history = (await _historyRepository.GetHistoryByAssetAndFiatAsync(asset, fiat, hours))
                .OrderBy(h => h.RecordedAt)
                .ToList();

            if (history.Count == 0)
                return Enumerable.Empty<(DateTime, decimal)>();

            var windowSpan = TimeSpan.FromMinutes(windowSizeMinutes);
            var results = new List<(DateTime WindowEnd, decimal AverageSpread)>();
            var windowEnd = history[0].RecordedAt.Add(windowSpan);
            var cutoff = history[^1].RecordedAt.Add(windowSpan);

            while (windowEnd <= cutoff)
            {
                var start = windowEnd.Subtract(windowSpan);
                var bucket = history
                    .Where(h => h.RecordedAt > start && h.RecordedAt <= windowEnd)
                    .Select(h => h.SpreadPercentage)
                    .ToList();

                if (bucket.Count > 0)
                    results.Add((windowEnd, Math.Round(bucket.Average(), 4)));

                windowEnd = windowEnd.Add(windowSpan);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error computing rolling window averages for {Asset}/{Fiat}", asset, fiat);
            throw;
        }
    }

    private static decimal CalculateStandardDeviation(IList<decimal> values, decimal mean)
    {
        if (values.Count < 2)
            return 0;

        var variance = values.Sum(v => (v - mean) * (v - mean)) / values.Count;
        return (decimal)Math.Sqrt((double)variance);
    }

    private static decimal CalculatePercentile(IList<decimal> sortedValues, decimal percentile)
    {
        if (sortedValues.Count == 0)
            return 0;

        var index = (double)(percentile / 100) * (sortedValues.Count - 1);
        var lower = (int)Math.Floor(index);
        var upper = Math.Min(lower + 1, sortedValues.Count - 1);
        var fraction = (decimal)(index - lower);

        return sortedValues[lower] + fraction * (sortedValues[upper] - sortedValues[lower]);
    }

    // Uses ordinary least-squares linear regression; slope is in spread-percentage-points per minute
    private static decimal CalculateTrendSlope(IList<PriceHistory> orderedHistory)
    {
        if (orderedHistory.Count < 2)
            return 0;

        var baseline = orderedHistory[0].RecordedAt;
        var xValues = orderedHistory.Select(h => (decimal)(h.RecordedAt - baseline).TotalMinutes).ToList();
        var yValues = orderedHistory.Select(h => h.SpreadPercentage).ToList();

        var xMean = xValues.Average();
        var yMean = yValues.Average();

        var numerator = xValues.Zip(yValues, (x, y) => (x - xMean) * (y - yMean)).Sum();
        var denominator = xValues.Sum(x => (x - xMean) * (x - xMean));

        return denominator == 0 ? 0 : Math.Round(numerator / denominator, 6);
    }
}
