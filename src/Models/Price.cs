#nullable enable
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

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
    /// <returns>The spread percentage between buy and sell prices</returns>
    public decimal CalculateSpread()
    {
        if (BuyPrice == 0)
            return 0;

        return ((SellPrice - BuyPrice) / BuyPrice) * 100;
    }

    /// <summary>
    /// Validates price data integrity
    /// </summary>
    /// <returns>True if the price data is valid; otherwise, false.</returns>
    public bool IsValid()
    {
        return BuyPrice > 0 && SellPrice > 0 && SellPrice >= BuyPrice &&
        !string.IsNullOrWhiteSpace(Asset) && !string.IsNullOrWhiteSpace(Fiat);
    }

    /// <summary>
    /// Checks if this price is significantly different from another
    /// </summary>
    /// <param name="other">The other price to compare against.</param>
    /// <param name="changeThreshold">The percentage threshold for considering prices different (default 0.5%).</param>
    /// <returns>True if the prices differ by more than the threshold; otherwise, false.</returns>
    public bool IsDifferentFrom(Price other, decimal changeThreshold = 0.5m)
    {
        if (other is null)
            return true;

        var buyDiff = Math.Abs(BuyPrice - other.BuyPrice) / other.BuyPrice * 100;
        var sellDiff = Math.Abs(SellPrice - other.SellPrice) / other.SellPrice * 100;

        return buyDiff > changeThreshold || sellDiff > changeThreshold;
    }

    /// <summary>
    /// Converts to JSON string for storage
    /// </summary>
    /// <returns>The JSON representation of the price.</returns>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Creates a Price instance from JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A new Price instance if successful; otherwise, null.</returns>
    public static Price? FromJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<Price>(json);
        }
        catch
        {
            return null;
        }
    }
}
