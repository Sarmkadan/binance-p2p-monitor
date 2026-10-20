#nullable enable
using System.ComponentModel.DataAnnotations;

namespace BinanceP2pMonitor.Models;

/// <summary>
/// Represents a user profile for Telegram alerts and preferences
/// </summary>
public class UserProfile
{
    /// <summary>
    /// Gets or sets the unique identifier for the user profile.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the Telegram chat identifier used for sending alerts.
    /// </summary>
    [Required]
    [Range(1, long.MaxValue)]
    public long TelegramChatId { get; set; }

    /// <summary>
    /// Gets or sets the Telegram username associated with this profile.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TelegramUsername { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address for the user.
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first name of the user.
    /// </summary>
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name of the user.
    /// </summary>
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the user profile is active.
    /// </summary>
    [Required]
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user receives real-time notifications.
    /// </summary>
    [Required]
    public bool ReceiveNotifications { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user receives a daily summary report.
    /// </summary>
    [Required]
    public bool ReceiveDailyReport { get; set; }

    /// <summary>
    /// Gets or sets the hour (1-24, UTC) at which the daily report is sent.
    /// </summary>
    [Range(1, 24)]
    public int DailyReportHourUtc { get; set; } = 9;

    /// <summary>
    /// Gets or sets the timestamp when the user profile was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the user profile was last updated.
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the binary representation of the last activity timestamp, or null if no activity has been recorded.
    /// </summary>
    [Range(0, long.MaxValue)]
    public long? LastActivityAt { get; set; }

    /// <summary>
    /// Gets or sets a JSON string containing user-specific preferences, or null if not configured.
    /// </summary>
    [StringLength(1000)]
    public string? Preferences { get; set; }

    /// <summary>
    /// Gets or sets the collection of price alerts associated with this user.
    /// </summary>
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
        if (LastActivityAt is null)
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

    /// <summary>
    /// Returns a string representation of the user profile
    /// </summary>
    /// <returns>A formatted string with key profile properties</returns>
    public override string ToString() =>
        $"UserProfile: Id={Id}, TelegramChatId={TelegramChatId}, Username={TelegramUsername}, " +
        $"Name={GetFullName()}, Email={Email}, IsActive={IsActive}, " +
        $"ReceiveNotifications={ReceiveNotifications}, ReceiveDailyReport={ReceiveDailyReport}, " +
        $"DailyReportHourUtc={DailyReportHourUtc}, ActiveAlerts={GetActiveAlertCount()}, " +
        $"CreatedAt={CreatedAt:u}, UpdatedAt={UpdatedAt:u}";
}
