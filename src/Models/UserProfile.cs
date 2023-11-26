// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace BinanceP2pMonitor.Models;

/// <summary>
/// Represents a user profile for Telegram alerts and preferences
/// </summary>
public class UserProfile
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Range(1, long.MaxValue)]
    public long TelegramChatId { get; set; }

    [Required]
    [StringLength(100)]
    public string TelegramUsername { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public bool IsActive { get; set; }

    [Required]
    public bool ReceiveNotifications { get; set; }

    [Required]
    public bool ReceiveDailyReport { get; set; }

    [Range(1, 24)]
    public int DailyReportHourUtc { get; set; } = 9;

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    [Range(0, long.MaxValue)]
    public long? LastActivityAt { get; set; }

    [StringLength(1000)]
    public string? Preferences { get; set; }

    // Navigation properties
    public ICollection<PriceAlert> Alerts { get; set; } = new List<PriceAlert>();

    /// <summary>
    /// Gets the full name of the user
    /// </summary>
    public string GetFullName()
    {
        var name = $"{FirstName} {LastName}".Trim();
        return string.IsNullOrWhiteSpace(name) ? TelegramUsername : name;
    }

    /// <summary>
    /// Updates the last activity timestamp
    /// </summary>
    public void UpdateActivity()
    {
        LastActivityAt = DateTime.UtcNow.ToBinary();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the user is active based on recent activity
    /// </summary>
    public bool IsRecentlyActive(int inactiveMinutes = 30)
    {
        if (LastActivityAt == null)
            return false;

        var lastActivity = DateTime.FromBinary(LastActivityAt.Value);
        var elapsed = DateTime.UtcNow - lastActivity;

        return elapsed.TotalMinutes < inactiveMinutes;
    }

    /// <summary>
    /// Gets the number of active alerts for this user
    /// </summary>
    public int GetActiveAlertCount()
    {
        return Alerts?.Count(a => a.IsEnabled) ?? 0;
    }

    /// <summary>
    /// Validates user profile data
    /// </summary>
    public bool IsValid()
    {
        return TelegramChatId > 0 &&
               !string.IsNullOrWhiteSpace(TelegramUsername) &&
               !string.IsNullOrWhiteSpace(Email) &&
               DailyReportHourUtc >= 1 && DailyReportHourUtc <= 24;
    }

    /// <summary>
    /// Disables all alerts for this user
    /// </summary>
    public void DisableAllAlerts()
    {
        foreach (var alert in Alerts ?? new List<PriceAlert>())
        {
            alert.IsEnabled = false;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets alerts for a specific trading pair
    /// </summary>
    public IEnumerable<PriceAlert> GetAlertsForPair(string asset, string fiat)
    {
        return Alerts?.Where(a => a.Asset == asset && a.Fiat == fiat) ?? Enumerable.Empty<PriceAlert>();
    }
}
