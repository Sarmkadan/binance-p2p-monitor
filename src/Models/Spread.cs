#nullable enable
using System.ComponentModel.DataAnnotations;

namespace BinanceP2pMonitor.Models;

/// <summary>
/// Represents spread analysis data between buy and sell prices
/// </summary>
public class Spread
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string Asset { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string Fiat { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue)]
    public decimal CurrentSpreadPercent { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal AverageSpreadPercent { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal MinSpreadPercent { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal MaxSpreadPercent { get; set; }

    [Required]
    [Range(1, long.MaxValue)]
    public long SampleCount { get; set; }

    [Required]
    public DateTime LastUpdatedAt { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Range(0, double.MaxValue)]
    public decimal StandardDeviation { get; set; }

    [Range(0, 100)]
    public decimal PercentileRank { get; set; }

    /// <summary>
    /// Determines if spread is unusually high
    /// </summary>
    /// <param name="threshold">The threshold percentage to consider as high spread (default 1.5%).</param>
    /// <returns>True if the spread is high; otherwise, false.</returns>
    public bool IsHighSpread(decimal threshold = 1.5m)
    {
        return CurrentSpreadPercent > threshold;
    }

    /// <summary>
    /// Determines if spread is unusually low
    /// </summary>
    /// <param name="threshold">The threshold percentage to consider as low spread (default 0.3%).</param>
    /// <returns>True if the spread is low; otherwise, false.</returns>
    public bool IsLowSpread(decimal threshold = 0.3m)
    {
        return CurrentSpreadPercent < threshold;
    }

    /// <summary>
    /// Calculates spread variance from average
    /// </summary>
    /// <returns>The variance percentage from the average spread.</returns>
    public decimal GetVarianceFromAverage()
    {
        if (AverageSpreadPercent == 0)
            return 0;

        return ((CurrentSpreadPercent - AverageSpreadPercent) / AverageSpreadPercent) * 100;
    }

    /// <summary>
    /// Checks if spread is within normal parameters
    /// </summary>
    /// <returns>True if the spread is within normal range; otherwise, false.</returns>
    public bool IsNormal()
    {
        return CurrentSpreadPercent >= MinSpreadPercent && CurrentSpreadPercent <= MaxSpreadPercent;
    }

    /// <summary>
    /// Validates spread analysis data
    /// </summary>
    /// <returns>True if the spread data is valid; otherwise, false.</returns>
    public bool IsValid()
    {
        return CurrentSpreadPercent >= 0 && AverageSpreadPercent >= 0 &&
        MinSpreadPercent >= 0 && MaxSpreadPercent >= MinSpreadPercent &&
        SampleCount > 0 && !string.IsNullOrWhiteSpace(Asset) && !string.IsNullOrWhiteSpace(Fiat);
    }

    /// <summary>
    /// Gets the risk level based on spread magnitude
    /// </summary>
    /// <returns>A string representing the risk level (Very Low, Low, Medium, High, Very High).</returns>
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
    /// <param name="newSpread">The new spread percentage to add to statistics.</param>
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
