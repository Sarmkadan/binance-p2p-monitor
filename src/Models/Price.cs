// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace BinanceP2pMonitor.Models;

/// <summary>
/// Represents a P2P price quote for a trading pair and fiat currency
/// </summary>
public class Price
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
    [Range(0.00000001, double.MaxValue)]
    public decimal BuyPrice { get; set; }

    [Required]
    [Range(0.00000001, double.MaxValue)]
    public decimal SellPrice { get; set; }

    [Required]
    [Range(0, 100)]
    public decimal BuyChangePercent { get; set; }

    [Required]
    [Range(0, 100)]
    public decimal SellChangePercent { get; set; }

    [Required]
    public DateTime Timestamp { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    [StringLength(1000)]
    public string? Metadata { get; set; }

    // Navigation properties
    public ICollection<PriceHistory> History { get; set; } = new List<PriceHistory>();

    /// <summary>
    /// Calculates the bid-ask spread in percentage
    /// </summary>
    public decimal CalculateSpread()
    {
        if (BuyPrice == 0)
            return 0;

        return ((SellPrice - BuyPrice) / BuyPrice) * 100;
    }

    /// <summary>
    /// Validates price data integrity
    /// </summary>
    public bool IsValid()
    {
        return BuyPrice > 0 && SellPrice > 0 && SellPrice >= BuyPrice &&
               !string.IsNullOrWhiteSpace(Asset) && !string.IsNullOrWhiteSpace(Fiat);
    }

    /// <summary>
    /// Checks if this price is significantly different from another
    /// </summary>
    public bool IsDifferentFrom(Price other, decimal changeThreshold = 0.5m)
    {
        if (other == null)
            return true;

        var buyDiff = Math.Abs(BuyPrice - other.BuyPrice) / other.BuyPrice * 100;
        var sellDiff = Math.Abs(SellPrice - other.SellPrice) / other.SellPrice * 100;

        return buyDiff > changeThreshold || sellDiff > changeThreshold;
    }

    /// <summary>
    /// Converts to JSON string for storage
    /// </summary>
    public string ToJson()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }

    /// <summary>
    /// Creates a Price instance from JSON string
    /// </summary>
    public static Price? FromJson(string json)
    {
        try
        {
            return JsonConvert.DeserializeObject<Price>(json);
        }
        catch
        {
            return null;
        }
    }
}
