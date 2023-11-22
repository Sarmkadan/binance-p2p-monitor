#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace BinanceP2pMonitor.Models;

/// <summary>
/// Immutable statistical report produced by historical spread analysis over a defined time window
/// </summary>
public class SpreadStatisticsReport
{
    /// <summary>Gets or sets the asset symbol (e.g., BTC, ETH)</summary>
    [Required]
    [StringLength(20)]
    public string Asset { get; set; } = string.Empty;

    /// <summary>Gets or sets the fiat currency (e.g., USD, EUR)</summary>
    [Required]
    [StringLength(10)]
    public string Fiat { get; set; } = string.Empty;

    /// <summary>Gets or sets the analysis time window in hours</summary>
    [Range(1, int.MaxValue)]
    public int TimeWindowHours { get; set; }

    /// <summary>Gets or sets the number of spread samples used in analysis</summary>
    [Range(0, long.MaxValue)]
    public long SampleCount { get; set; }

    /// <summary>Gets or sets the arithmetic mean of historical spread percentages</summary>
    [Range(0, double.MaxValue)]
    public decimal Mean { get; set; }

    /// <summary>Gets or sets the median (50th percentile) of historical spread percentages</summary>
    [Range(0, double.MaxValue)]
    public decimal Median { get; set; }

    /// <summary>Gets or sets the population standard deviation of historical spread values</summary>
    [Range(0, double.MaxValue)]
    public decimal StandardDeviation { get; set; }

    /// <summary>Gets or sets the population variance of historical spread values</summary>
    [Range(0, double.MaxValue)]
    public decimal Variance { get; set; }

    /// <summary>Gets or sets the minimum spread percentage observed in the window</summary>
    [Range(0, double.MaxValue)]
    public decimal MinSpread { get; set; }

    /// <summary>Gets or sets the maximum spread percentage observed in the window</summary>
    [Range(0, double.MaxValue)]
    public decimal MaxSpread { get; set; }

    /// <summary>Gets or sets the 5th percentile of historical spread values</summary>
    [Range(0, double.MaxValue)]
    public decimal Percentile5 { get; set; }

    /// <summary>Gets or sets the 95th percentile of historical spread values</summary>
    [Range(0, double.MaxValue)]
    public decimal Percentile95 { get; set; }

    /// <summary>Gets or sets the most recently observed spread percentage</summary>
    public decimal CurrentSpread { get; set; }

    /// <summary>
    /// Gets or sets the Z-score of the current spread relative to the historical distribution.
    /// Positive values indicate the spread is above the mean; negative values indicate below.
    /// </summary>
    public decimal ZScore { get; set; }

    /// <summary>
    /// Gets or sets the linear regression slope of spread values over the analysis window,
    /// expressed as percentage-points per minute
    /// </summary>
    public decimal TrendSlope { get; set; }

    /// <summary>Gets or sets when this report was generated (UTC)</summary>
    [Required]
    public DateTime AnalyzedAt { get; set; }

    /// <summary>
    /// Gets or sets whether the current spread is flagged as statistically anomalous
    /// (Z-score magnitude exceeded the configured threshold during analysis)
    /// </summary>
    public bool IsAnomalous { get; set; }

    /// <summary>
    /// Returns a human-readable description of the spread trend direction and strength
    /// </summary>
    public string GetTrendLabel() => TrendSlope switch
    {
        > 0.5m  => "Strong Uptrend",
        > 0.1m  => "Mild Uptrend",
        < -0.5m => "Strong Downtrend",
        < -0.1m => "Mild Downtrend",
        _       => "Stable"
    };

    /// <summary>
    /// Determines whether the current spread is critically anomalous, i.e., its Z-score
    /// magnitude exceeds <paramref name="criticalZScore"/>
    /// </summary>
    public bool IsCritical(decimal criticalZScore = 3.0m) => Math.Abs(ZScore) > criticalZScore;

    /// <summary>
    /// Determines whether the current spread exceeds the historical mean
    /// </summary>
    public bool IsAboveAverage() => CurrentSpread > Mean;

    /// <summary>
    /// Returns the interquartile-range width (Percentile95 − Percentile5) as a measure of spread volatility
    /// </summary>
    public decimal GetVolatilityRange() => Percentile95 - Percentile5;

    /// <summary>
    /// Validates that the report contains consistent, non-default data
    /// </summary>
    public bool IsValid()
        => !string.IsNullOrWhiteSpace(Asset) && !string.IsNullOrWhiteSpace(Fiat)
           && SampleCount >= 0 && TimeWindowHours > 0;
}
