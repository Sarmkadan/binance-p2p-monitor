// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using BinanceP2pMonitor.Constants;

namespace BinanceP2pMonitor.Models;

/// <summary>
/// Represents a price alert configuration for monitoring
/// </summary>
public class PriceAlert
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
    public AlertType AlertType { get; set; }

    [Required]
    [Range(0, 100)]
    public decimal Threshold { get; set; }

    [Required]
    public AlertCondition Condition { get; set; }

    [Required]
    public bool IsEnabled { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    [Range(0, long.MaxValue)]
    public long? LastTriggeredAt { get; set; }

    [Range(1, int.MaxValue)]
    public int TriggerCount { get; set; } = 0;

    [StringLength(500)]
    public string? Notes { get; set; }

    public UserProfile? User { get; set; }

    /// <summary>
    /// Determines if this alert should trigger based on current price change
    /// </summary>
    public bool ShouldTrigger(decimal currentChange)
    {
        if (!IsEnabled)
            return false;

        return Condition switch
        {
            AlertCondition.GreaterThan => currentChange > Threshold,
            AlertCondition.LessThan => currentChange < Threshold,
            AlertCondition.Equals => Math.Abs(currentChange - Threshold) < 0.01m,
            AlertCondition.GreaterThanOrEqual => currentChange >= Threshold,
            AlertCondition.LessThanOrEqual => currentChange <= Threshold,
            _ => false
        };
    }

    /// <summary>
    /// Checks if enough time has passed since last trigger (cooldown period in minutes)
    /// </summary>
    public bool IsInCooldownPeriod(int cooldownMinutes = 5)
    {
        if (LastTriggeredAt == null)
            return false;

        var lastTrigger = DateTime.FromBinary(LastTriggeredAt.Value);
        var elapsed = DateTime.UtcNow - lastTrigger;

        return elapsed.TotalMinutes < cooldownMinutes;
    }

    /// <summary>
    /// Updates the last triggered timestamp and increments counter
    /// </summary>
    public void RecordTrigger()
    {
        LastTriggeredAt = DateTime.UtcNow.ToBinary();
        TriggerCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Validates alert configuration
    /// </summary>
    public bool IsValid()
    {
        return Threshold >= 0 && Threshold <= 100 &&
               !string.IsNullOrWhiteSpace(Asset) && !string.IsNullOrWhiteSpace(Fiat) &&
               UserId > 0;
    }

    /// <summary>
    /// Gets a human-readable description of the alert
    /// </summary>
    public string GetDescription()
    {
        var conditionText = Condition switch
        {
            AlertCondition.GreaterThan => ">",
            AlertCondition.LessThan => "<",
            AlertCondition.Equals => "=",
            AlertCondition.GreaterThanOrEqual => ">=",
            AlertCondition.LessThanOrEqual => "<=",
            _ => "?"
        };

        return $"{Asset}/{Fiat}: {AlertType} alert when {conditionText} {Threshold}%";
    }

    /// <summary>
    /// Toggles the enabled state of the alert
    /// </summary>
    public void Toggle()
    {
        IsEnabled = !IsEnabled;
        UpdatedAt = DateTime.UtcNow;
    }
}
