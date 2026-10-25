#nullable enable
using System.ComponentModel.DataAnnotations;

namespace BinanceP2pMonitor.Models;

/// <summary>
/// Represents a currency for P2P trading
/// </summary>
public class Currency
{
    /// <summary>
    /// Gets or sets the primary key
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the currency code (e.g. USD, EUR)
    /// </summary>
    [Required]
    [StringLength(10)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full currency name
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional currency symbol (e.g. $, €)
    /// </summary>
    [StringLength(3)]
    public string? Symbol { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the currency is active
    /// </summary>
    [Required]
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the number of decimal places used for values in this currency
    /// </summary>
    [Required]
    public int DecimalPlaces { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the popularity score in the range 0-100
    /// </summary>
    [Range(0, 100)]
    public decimal PopularityScore { get; set; }

    /// <summary>
    /// Gets or sets the display order in the range 1-100 (default 50)
    /// </summary>
    [Range(1, 100)]
    public int DisplayOrder { get; set; } = 50;

    /// <summary>
    /// Gets or sets optional free-form notes
    /// </summary>
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
    /// <exception cref="ArgumentNullException"><paramref name="other"/> is <see langword="null"/>.</exception>
    public int ComparePopularity(Currency other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return PopularityScore.CompareTo(other.PopularityScore);
    }

    /// <summary>
    /// Returns a readable summary of the currency
    /// </summary>
    public override string ToString() =>
        $"Currency: Id={Id}, Code={Code}, Name={Name}, Symbol={Symbol}, IsActive={IsActive}, " +
        $"DecimalPlaces={DecimalPlaces}, DisplayOrder={DisplayOrder}, PopularityScore={PopularityScore}";
}
