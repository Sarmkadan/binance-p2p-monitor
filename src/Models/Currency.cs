// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace BinanceP2pMonitor.Models;

/// <summary>
/// Represents a currency for P2P trading
/// </summary>
public class Currency
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(10)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(3)]
    public string? Symbol { get; set; }

    [Required]
    public bool IsActive { get; set; }

    [Required]
    public int DecimalPlaces { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    [Range(0, 100)]
    public decimal PopularityScore { get; set; }

    [Range(1, 100)]
    public int DisplayOrder { get; set; } = 50;

    [StringLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Formats a decimal value according to currency precision
    /// </summary>
    public string FormatValue(decimal value)
    {
        return value.ToString($"F{DecimalPlaces}");
    }

    /// <summary>
    /// Gets the currency display format (code or symbol)
    /// </summary>
    public string GetDisplayFormat()
    {
        return !string.IsNullOrWhiteSpace(Symbol) ? Symbol : Code;
    }

    /// <summary>
    /// Validates currency data
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Code) && !string.IsNullOrWhiteSpace(Name) &&
               DecimalPlaces >= 0 && DecimalPlaces <= 8 &&
               DisplayOrder >= 1 && DisplayOrder <= 100;
    }

    /// <summary>
    /// Checks if currency is commonly used
    /// </summary>
    public bool IsPopular()
    {
        return PopularityScore >= 70;
    }

    /// <summary>
    /// Gets popularity tier
    /// </summary>
    public string GetPopularityTier()
    {
        return PopularityScore switch
        {
            >= 90 => "Premium",
            >= 70 => "Popular",
            >= 50 => "Standard",
            >= 30 => "Niche",
            _ => "Rare"
        };
    }

    /// <summary>
    /// Rounds a value to the appropriate decimal places
    /// </summary>
    public decimal RoundValue(decimal value)
    {
        return Math.Round(value, DecimalPlaces);
    }

    /// <summary>
    /// Gets human-readable name with code
    /// </summary>
    public string GetFullName()
    {
        var symbol = !string.IsNullOrWhiteSpace(Symbol) ? $" ({Symbol})" : string.Empty;
        return $"{Name}{symbol} [{Code}]";
    }

    /// <summary>
    /// Compares two currencies by popularity
    /// </summary>
    public int ComparePopularity(Currency other)
    {
        return PopularityScore.CompareTo(other.PopularityScore);
    }
}
