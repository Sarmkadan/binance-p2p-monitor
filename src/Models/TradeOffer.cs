#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using BinanceP2pMonitor.Constants;

namespace BinanceP2pMonitor.Models;

/// <summary>
/// Represents a P2P trade offer scraped from Binance. Contains the offer price,
/// trading limits, trader reputation, and supported payment methods.
/// Each offer represents a single buy or sell listing on the Binance P2P marketplace.
/// </summary>
public class TradeOffer
{
    /// <summary>Database primary key.</summary>
    [Key]
    public int Id { get; set; }

    /// <summary>Binance-assigned offer identifier for deduplication.</summary>
    [Required]
    [StringLength(100)]
    public string OfferIdFromBinance { get; set; } = string.Empty;

    /// <summary>Cryptocurrency asset being traded (e.g., "USDT", "BTC").</summary>
    [Required]
    [StringLength(20)]
    public string Asset { get; set; } = string.Empty;

    /// <summary>Fiat currency for the trade (e.g., "UAH", "USD", "EUR").</summary>
    [Required]
    [StringLength(10)]
    public string Fiat { get; set; } = string.Empty;

    /// <summary>Whether this is a buy or sell offer.</summary>
    [Required]
    public TradeType TradeType { get; set; }

    /// <summary>Price per unit of asset in fiat currency.</summary>
    [Required]
    [Range(0.00000001, double.MaxValue)]
    public decimal Price { get; set; }

    /// <summary>Minimum trade amount in fiat currency.</summary>
    [Required]
    [Range(0.00000001, double.MaxValue)]
    public decimal MinAmount { get; set; }

    /// <summary>Maximum trade amount in fiat currency.</summary>
    [Required]
    [Range(0.00000001, double.MaxValue)]
    public decimal MaxAmount { get; set; }

    /// <summary>Trader's reputation score (0-100%).</summary>
    [Required]
    [Range(0, 100)]
    public decimal TraderRating { get; set; }

    /// <summary>Number of successfully completed trades by this trader.</summary>
    [Required]
    public int CompletedTrades { get; set; }

    /// <summary>Comma-separated list of accepted payment methods.</summary>
    [StringLength(500)]
    public string PaymentMethods { get; set; } = string.Empty;

    /// <summary>Whether the offer is currently active on Binance.</summary>
    [Required]
    public bool IsActive { get; set; }

    /// <summary>Timestamp when the offer was observed on Binance.</summary>
    [Required]
    public DateTime Timestamp { get; set; }

    /// <summary>UTC timestamp when this record was first created in the database.</summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC timestamp of the last update to this record.</summary>
    [Required]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Checks if offer matches specified criteria
    /// </summary>
    public bool MatchesCriteria(decimal minRating, decimal minAmount, decimal maxAmount)
    {
        return IsActive && TraderRating >= minRating &&
               MaxAmount >= minAmount && MinAmount <= maxAmount;
    }

    /// <summary>
    /// Calculates the premium/discount percentage compared to reference price
    /// </summary>
    public decimal CalculatePremium(decimal referencePrice)
    {
        if (referencePrice == 0)
            return 0;

        return ((Price - referencePrice) / referencePrice) * 100;
    }

    /// <summary>
    /// Validates the trade offer integrity
    /// </summary>
    public bool IsValid()
    {
        return Price > 0 && MinAmount > 0 && MaxAmount >= MinAmount &&
               TraderRating >= 0 && TraderRating <= 100 && CompletedTrades >= 0 &&
               !string.IsNullOrWhiteSpace(Asset) && !string.IsNullOrWhiteSpace(Fiat);
    }

    /// <summary>
    /// Calculates the available volume in this offer
    /// </summary>
    public decimal GetAvailableRange()
    {
        return MaxAmount - MinAmount;
    }

    /// <summary>
    /// Checks if a specific amount can be traded with this offer
    /// </summary>
    public bool CanTradeAmount(decimal amount)
    {
        return IsActive && amount >= MinAmount && amount <= MaxAmount;
    }
}
