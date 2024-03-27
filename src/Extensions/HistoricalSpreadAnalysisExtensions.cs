#nullable enable
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BinanceP2pMonitor.Extensions;

/// <summary>
/// Extension methods for registering historical spread analysis with the DI container
/// and working with spread statistics reports.
/// </summary>
public static class HistoricalSpreadAnalysisExtensions
{
    /// <summary>
    /// Registers <see cref="IHistoricalSpreadAnalysisService"/> as a scoped service.
    /// Requires <see cref="IHistoryRepository"/>, <see cref="ISpreadAnalysisService"/>,
    /// <see cref="IEventBus"/>, and <see cref="AppSettings"/> to be registered beforehand.
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    /// <returns>The same <paramref name="services"/> instance for fluent chaining</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/></exception>
    public static IServiceCollection AddHistoricalSpreadAnalysis(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IHistoricalSpreadAnalysisService, HistoricalSpreadAnalysisService>();
        return services;
    }

    /// <summary>
    /// Formats a <see cref="SpreadStatisticsReport"/> as a multi-line human-readable summary
    /// suitable for console output or structured logging.
    /// </summary>
    /// <param name="report">The spread statistics report to format</param>
    /// <returns>A formatted string representation of the key statistics</returns>
    /// <exception cref="ArgumentNullException"><paramref name="report"/> is <see langword="null"/></exception>
    public static string ToSummaryString(this SpreadStatisticsReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var sb = new System.Text.StringBuilder();

        sb.AppendLine($"┌─ Spread Analysis: {report.Asset}/{report.Fiat} ──────────────────────────────────");
        sb.AppendLine($"│ Window : Last {report.TimeWindowHours}h (analyzed at {report.AnalyzedAt:yyyy-MM-dd HH:mm:ss} UTC)");
        sb.AppendLine($"│ Samples : {report.SampleCount:N0}");
        sb.AppendLine($"│ Current Spread : {report.CurrentSpread:F4}%");
        sb.AppendLine($"│ Mean : {report.Mean:F4}% ± {report.StandardDeviation:F4}%");
        sb.AppendLine($"│ Median : {report.Median:F4}% (50th percentile)");
        sb.AppendLine($"│ Range : [{report.MinSpread:F4}%, {report.MaxSpread:F4}%]");
        sb.AppendLine($"│ IQR : [{report.Percentile5:F4}%, {report.Percentile95:F4}%] (volatility range)");
        sb.AppendLine($"│ Z-Score : {report.ZScore:F2} ({(report.IsAnomalous ? "ANOMALOUS" : "normal")})");
        sb.AppendLine($"│ Trend : {report.GetTrendLabel()} ({report.TrendSlope:+0.000000;-0.000000} %/min)");
        sb.Append("└──────────────────────────────────────────────────────────────────────");

        return sb.ToString();
    }

    /// <summary>
    /// Determines whether the spread statistics report indicates a critical anomaly.
    /// </summary>
    /// <param name="report">The spread statistics report to check</param>
    /// <param name="criticalZScore">The critical Z-score threshold (default 3.0)</param>
    /// <returns>True if the spread is critically anomalous; otherwise, false</returns>
    /// <exception cref="ArgumentNullException"><paramref name="report"/> is <see langword="null"/></exception>
    public static bool IsCritical(this SpreadStatisticsReport report, decimal criticalZScore = 3.0m)
    {
        ArgumentNullException.ThrowIfNull(report);
        return report.IsCritical(criticalZScore);
    }

    /// <summary>
    /// Determines whether the current spread exceeds the historical mean.
    /// </summary>
    /// <param name="report">The spread statistics report to check</param>
    /// <returns>True if the current spread is above average; otherwise, false</returns>
    /// <exception cref="ArgumentNullException"><paramref name="report"/> is <see langword="null"/></exception>
    public static bool IsAboveAverage(this SpreadStatisticsReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        return report.IsAboveAverage();
    }

    /// <summary>
    /// Returns the interquartile-range width as a measure of spread volatility.
    /// </summary>
    /// <param name="report">The spread statistics report to analyze</param>
    /// <returns>The volatility range as a decimal value</returns>
    /// <exception cref="ArgumentNullException"><paramref name="report"/> is <see langword="null"/></exception>
    public static decimal GetVolatilityRange(this SpreadStatisticsReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        return report.GetVolatilityRange();
    }
}
