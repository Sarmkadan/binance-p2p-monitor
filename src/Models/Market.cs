#nullable enable
using System.ComponentModel.DataAnnotations;

namespace BinanceP2pMonitor.Models;

/// <summary>
/// Represents a trading market (asset/fiat pair)
/// </summary>
public class Market
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
    public bool IsActive { get; set; }

    [Required]
    public bool IsMonitored { get; set; }

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.00000001, double.MaxValue)]
    public decimal LastBuyPrice { get; set; }

    [Required]
    [Range(0.00000001, double.MaxValue)]
    public decimal LastSellPrice { get; set; }

    [Required]
    public long TotalOffers { get; set; }

    [Required]
    [Range(0, long.MaxValue)]
    public long DailyVolume { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    [Range(0, long.MaxValue)]
    public long? LastPriceUpdateAt { get; set; }

    [Range(1, 100)]
    public int MonitoringPriority { get; set; } = 50;

    /// <summary>
    /// Gets the market pair identifier
    /// </summary>
    public string GetPairId()
    {
        return $"{Asset}/{Fiat}";
    }

    /// <summary>
    /// Calculates the current spread percentage
    /// </summary>
    public decimal CalculateSpread()
    {
        if (LastBuyPrice == 0)
            return 0;

        return ((LastSellPrice - LastBuyPrice) / LastBuyPrice) * 100;
    }

    /// <summary>
    /// Updates last price data
    /// </summary>
    public void UpdatePrices(decimal buyPrice, decimal sellPrice)
    {
        LastBuyPrice = buyPrice;
        LastSellPrice = sellPrice;
        LastPriceUpdateAt = DateTime.UtcNow.ToBinary();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if prices are stale (not updated recently)
    /// </summary>
    public bool IsPriceStale(int maxAgeMinutes = 5)
    {
        if (LastPriceUpdateAt is null)
            return true;

        var lastUpdate = DateTime.FromBinary(LastPriceUpdateAt.Value);
        var elapsed = DateTime.UtcNow - lastUpdate;

        return elapsed.TotalMinutes > maxAgeMinutes;
    }

    /// <summary>
    /// Validates market data
    /// </summary>
    public bool IsValid()
    {
        return LastBuyPrice > 0 && LastSellPrice > 0 && LastSellPrice >= LastBuyPrice &&
               !string.IsNullOrWhiteSpace(Asset) && !string.IsNullOrWhiteSpace(Fiat) &&
               MonitoringPriority >= 1 && MonitoringPriority <= 100;
    }

    /// <summary>
    /// Gets market activity level based on volume
    /// </summary>
    public string GetActivityLevel()
    {
        return DailyVolume switch
        {
            > 1_000_000 => "Very High",
            > 500_000 => "High",
            > 100_000 => "Medium",
            > 10_000 => "Low",
            _ => "Very Low"
        };
    }

    /// <summary>
    /// Toggles monitoring for this market
    /// </summary>
    public void ToggleMonitoring()
    {
        IsMonitored = !IsMonitored;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the monitoring priority
    /// </summary>
    public void SetPriority(int priority)
    {
        if (priority >= 1 && priority <= 100)
        {
            MonitoringPriority = priority;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
