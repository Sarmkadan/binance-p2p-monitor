#nullable enable
using System.ComponentModel.DataAnnotations;

namespace BinanceP2pMonitor.Models;

/// <summary>
/// Represents historical price records for trend analysis
/// </summary>
public class PriceHistory
{
    /// <summary>
    /// Gets or sets the unique identifier for the price history record.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the related price record.
    /// </summary>
    [Required]
    public int PriceId { get; set; }

    /// <summary>
    /// Gets or sets the asset symbol (e.g., USDT).
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Asset { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the fiat currency code (e.g., USD).
    /// </summary>
    [Required]
    [StringLength(10)]
    public string Fiat { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the buy price recorded for the asset.
    /// </summary>
    [Required]
    [Range(0.00000001, double.MaxValue)]
    public decimal BuyPrice { get; set; }

    /// <summary>
    /// Gets or sets the sell price recorded for the asset.
    /// </summary>
    [Required]
    [Range(0.00000001, double.MaxValue)]
    public decimal SellPrice { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp at which the price was recorded.
    /// </summary>
    [Required]
    public DateTime RecordedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp at which this record was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the spread percentage between the buy and sell prices.
    /// </summary>
    [Range(0, 100)]
    public decimal SpreadPercentage { get; set; }

    /// <summary>
    /// Gets or sets the percentage price change relative to a previous record.
    /// </summary>
    [Range(0, 100)]
    public decimal PriceChangePercent { get; set; }

    /// <summary>
    /// Gets or sets optional notes about this record.
    /// </summary>
    [StringLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the related price record.
    /// </summary>
    public Price? Price { get; set; }

    /// <summary>
    /// Calculates the mid-price between buy and sell
    /// </summary>
    /// <returns>The average price between buy and sell prices.</returns>
    public decimal GetMidPrice()
    {
        return (BuyPrice + SellPrice) / 2;
    }

    /// <summary>
    /// Calculates the spread percentage
    /// </summary>
    /// <returns>The spread percentage between buy and sell prices.</returns>
    public decimal CalculateSpread()
    {
        if (BuyPrice == 0)
            return 0;

        return ((SellPrice - BuyPrice) / BuyPrice) * 100;
    }

    /// <summary>
    /// Validates historical price data
    /// </summary>
    /// <returns>True if the historical price data is valid; otherwise, false.</returns>
    public bool IsValid()
    {
        return BuyPrice > 0 && SellPrice > 0 && SellPrice >= BuyPrice &&
        !string.IsNullOrWhiteSpace(Asset) && !string.IsNullOrWhiteSpace(Fiat) &&
        RecordedAt <= DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if this record is recent (within last hour)
    /// </summary>
    /// <returns>True if the record was created within the last hour; otherwise, false.</returns>
    public bool IsRecent()
    {
        var elapsed = DateTime.UtcNow - RecordedAt;
        return elapsed.TotalHours < 1;
    }

    /// <summary>
    /// Gets age of this record in minutes
    /// </summary>
    /// <returns>The age of the record in minutes.</returns>
    public int GetAgeInMinutes()
    {
        var elapsed = DateTime.UtcNow - RecordedAt;
        return (int)elapsed.TotalMinutes;
    }

    /// <summary>
    /// Returns a string representation of the price history record.
    /// </summary>
    /// <returns>A formatted string with key properties.</returns>
    public override string ToString()
    {
        return $"PriceHistory {{ Id={Id}, Asset={Asset}, Fiat={Fiat}, Buy={BuyPrice}, Sell={SellPrice}, Recorded={RecordedAt:yyyy-MM-dd HH:mm:ss}, Spread={SpreadPercentage}%, Change={PriceChangePercent}% }}";
    }

    /// <summary>
    /// Compares this history record with another to determine trend
    /// </summary>
    /// <param name="other">The other price history record to compare against.</param>
    /// <returns>The percentage change from the other record to this one.</returns>
    public decimal CompareTo(PriceHistory other)
    {
        if (other is null || other.BuyPrice == 0)
            return 0;

        return ((BuyPrice - other.BuyPrice) / other.BuyPrice) * 100;
    }
}
