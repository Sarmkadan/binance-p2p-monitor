// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace BinanceP2pMonitor.Models;

/// <summary>
/// Represents historical price records for trend analysis
/// </summary>
public class PriceHistory
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PriceId { get; set; }

    [Required]
    [StringLength(20)]
    public string Asset { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string Fiat { get; set; } = string.Empty;

    [Required]
    [Range(0.00000001, double.MaxValue)]
    public decimal BuyPrice { get; set; }

    [Required]
    [Range(0.00000001, double.MaxValue)]
    public decimal SellPrice { get; set; }

    [Required]
    public DateTime RecordedAt { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Range(0, 100)]
    public decimal SpreadPercentage { get; set; }

    [Range(0, 100)]
    public decimal PriceChangePercent { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public Price? Price { get; set; }

    /// <summary>
    /// Calculates the mid-price between buy and sell
    /// </summary>
    public decimal GetMidPrice()
    {
        return (BuyPrice + SellPrice) / 2;
    }

    /// <summary>
    /// Calculates the spread percentage
    /// </summary>
    public decimal CalculateSpread()
    {
        if (BuyPrice == 0)
            return 0;

        return ((SellPrice - BuyPrice) / BuyPrice) * 100;
    }

    /// <summary>
    /// Validates historical price data
    /// </summary>
    public bool IsValid()
    {
        return BuyPrice > 0 && SellPrice > 0 && SellPrice >= BuyPrice &&
               !string.IsNullOrWhiteSpace(Asset) && !string.IsNullOrWhiteSpace(Fiat) &&
               RecordedAt <= DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if this record is recent (within last hour)
    /// </summary>
    public bool IsRecent()
    {
        var elapsed = DateTime.UtcNow - RecordedAt;
        return elapsed.TotalHours < 1;
    }

    /// <summary>
    /// Gets age of this record in minutes
    /// </summary>
    public int GetAgeInMinutes()
    {
        var elapsed = DateTime.UtcNow - RecordedAt;
        return (int)elapsed.TotalMinutes;
    }

    /// <summary>
    /// Compares this history record with another to determine trend
    /// </summary>
    public decimal CompareTo(PriceHistory other)
    {
        if (other == null || other.BuyPrice == 0)
            return 0;

        return ((BuyPrice - other.BuyPrice) / other.BuyPrice) * 100;
    }
}
