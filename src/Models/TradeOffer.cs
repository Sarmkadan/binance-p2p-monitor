#nullable enable
using System.ComponentModel.DataAnnotations;
using BinanceP2pMonitor.Constants;

namespace BinanceP2pMonitor.Models;

/// <summary>
/// Represents a P2P trade offer from Binance
/// </summary>
public class TradeOffer
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string OfferIdFromBinance { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Asset { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string Fiat { get; set; } = string.Empty;

    [Required]
    public TradeType TradeType { get; set; }

    [Required]
    [Range(0.00000001, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    [Range(0.00000001, double.MaxValue)]
    public decimal MinAmount { get; set; }

    [Required]
    [Range(0.00000001, double.MaxValue)]
    public decimal MaxAmount { get; set; }

    [Required]
    [Range(0, 100)]
    public decimal TraderRating { get; set; }

    [Required]
    public int CompletedTrades { get; set; }

    [StringLength(500)]
    public string PaymentMethods { get; set; } = string.Empty;

    [Required]
    public bool IsActive { get; set; }

    [Required]
    public DateTime Timestamp { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Checks if offer matches specified criteria
    /// </summary>
    /// <param name="minRating">The minimum trader rating required.</param>
    /// <param name="minAmount">The minimum amount that can be traded.</param>
    /// <param name="maxAmount">The maximum amount that can be traded.</param>
    /// <returns>True if the offer matches all criteria; otherwise, false.</returns>
    public bool MatchesCriteria(decimal minRating, decimal minAmount, decimal maxAmount)
    {
        return IsActive && TraderRating >= minRating &&
        MaxAmount >= minAmount && MinAmount <= maxAmount;
    }

    /// <summary>
    /// Calculates the premium/discount percentage compared to reference price
    /// </summary>
    /// <param name="referencePrice">The reference price to compare against.</param>
    /// <returns>The premium or discount percentage.</returns>
    public decimal CalculatePremium(decimal referencePrice)
    {
        if (referencePrice == 0)
            return 0;

        return ((Price - referencePrice) / referencePrice) * 100;
    }

    /// <summary>
    /// Validates the trade offer integrity
    /// </summary>
    /// <returns>True if the trade offer data is valid; otherwise, false.</returns>
    public bool IsValid()
    {
        return Price > 0 && MinAmount > 0 && MaxAmount >= MinAmount &&
        TraderRating >= 0 && TraderRating <= 100 && CompletedTrades >= 0 &&
        !string.IsNullOrWhiteSpace(Asset) && !string.IsNullOrWhiteSpace(Fiat);
    }

    /// <summary>
    /// Calculates the available volume in this offer
    /// </summary>
    /// <returns>The available trading range (MaxAmount - MinAmount).</returns>
    public decimal GetAvailableRange()
    {
        return MaxAmount - MinAmount;
    }

    /// <summary>
    /// Checks if a specific amount can be traded with this offer
    /// </summary>
    /// <param name="amount">The amount to check.</param>
    /// <returns>True if the amount can be traded with this offer; otherwise, false.</returns>
    public bool CanTradeAmount(decimal amount)
    {
        return IsActive && amount >= MinAmount && amount <= MaxAmount;
    }
}
