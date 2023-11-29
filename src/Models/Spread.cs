#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace BinanceP2pMonitor.Models;

/// <summary>
/// Represents spread analysis data for a specific asset/fiat pair on Binance P2P.
/// Tracks the percentage difference between the best buy and sell prices,
/// with rolling statistics (average, min, max, standard deviation) over time.
/// </summary>
public class Spread
{
    /// <summary>Database primary key.</summary>
    [Key]
    public int Id { get; set; }

    /// <summary>Cryptocurrency asset (e.g., "USDT", "BTC").</summary>
    [Required]
    [StringLength(20)]
    public string Asset { get; set; } = string.Empty;

    /// <summary>Fiat currency (e.g., "UAH", "USD").</summary>
    [Required]
    [StringLength(10)]
    public string Fiat { get; set; } = string.Empty;

    /// <summary>Current spread as a percentage of the buy price.</summary>
    [Required]
    [Range(0, double.MaxValue)]
    public decimal CurrentSpreadPercent { get; set; }

    /// <summary>Running average spread percentage across all samples.</summary>
    [Required]
    [Range(0, double.MaxValue)]
    public decimal AverageSpreadPercent { get; set; }

    /// <summary>Lowest observed spread percentage.</summary>
    [Required]
    [Range(0, double.MaxValue)]
    public decimal MinSpreadPercent { get; set; }

    /// <summary>Highest observed spread percentage.</summary>
    [Required]
    [Range(0, double.MaxValue)]
    public decimal MaxSpreadPercent { get; set; }

    /// <summary>Total number of spread observations collected.</summary>
    [Required]
    [Range(1, long.MaxValue)]
    public long SampleCount { get; set; }

    /// <summary>UTC timestamp of the last spread sample.</summary>
    [Required]
    public DateTime LastUpdatedAt { get; set; }

    /// <summary>UTC timestamp when tracking started for this pair.</summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>Standard deviation of spread values for volatility analysis.</summary>
    [Range(0, double.MaxValue)]
    public decimal StandardDeviation { get; set; }

    /// <summary>Percentile rank (0-100) of the current spread relative to historical data.</summary>
    [Range(0, 100)]
    public decimal PercentileRank { get; set; }

    /// <summary>
    /// Determines if spread is unusually high
    /// </summary>
    public bool IsHighSpread(decimal threshold = 1.5m)
    {
        return CurrentSpreadPercent > threshold;
    }

    /// <summary>
    /// Determines if spread is unusually low
    /// </summary>
    public bool IsLowSpread(decimal threshold = 0.3m)
    {
        return CurrentSpreadPercent < threshold;
    }

    /// <summary>
    /// Calculates spread variance from average
    /// </summary>
    public decimal GetVarianceFromAverage()
    {
        if (AverageSpreadPercent == 0)
            return 0;

        return ((CurrentSpreadPercent - AverageSpreadPercent) / AverageSpreadPercent) * 100;
    }

    /// <summary>
    /// Checks if spread is within normal parameters
    /// </summary>
    public bool IsNormal()
    {
        return CurrentSpreadPercent >= MinSpreadPercent && CurrentSpreadPercent <= MaxSpreadPercent;
    }

    /// <summary>
    /// Validates spread analysis data
    /// </summary>
    public bool IsValid()
    {
        return CurrentSpreadPercent >= 0 && AverageSpreadPercent >= 0 &&
               MinSpreadPercent >= 0 && MaxSpreadPercent >= MinSpreadPercent &&
               SampleCount > 0 && !string.IsNullOrWhiteSpace(Asset) && !string.IsNullOrWhiteSpace(Fiat);
    }

    /// <summary>
    /// Gets the risk level based on spread magnitude
    /// </summary>
    public string GetRiskLevel()
    {
        return CurrentSpreadPercent switch
        {
            < 0.3m => "Very Low",
            < 0.6m => "Low",
            < 1.0m => "Medium",
            < 1.5m => "High",
            _ => "Very High"
        };
    }

    /// <summary>
    /// Updates spread statistics (call after new sample)
    /// </summary>
    public void UpdateStatistics(decimal newSpread)
    {
        if (newSpread < MinSpreadPercent || MinSpreadPercent == 0)
            MinSpreadPercent = newSpread;

        if (newSpread > MaxSpreadPercent)
            MaxSpreadPercent = newSpread;

        AverageSpreadPercent = (AverageSpreadPercent * SampleCount + newSpread) / (SampleCount + 1);
        CurrentSpreadPercent = newSpread;
        SampleCount++;
        LastUpdatedAt = DateTime.UtcNow;
    }
}
