namespace BinanceP2pMonitor.Models;

/// <summary>
/// Provides extension methods for <see cref="UserProfile"/>.
/// </summary>
public static class UserProfileExtensions
{
    /// <summary>
    /// Gets a formatted full name for the user, handling cases where first or last name might be null or empty.
    /// </summary>
    /// <param name="profile">The <see cref="UserProfile"/> instance.</param>
    /// <returns>A formatted full name.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="profile"/> is <c>null</c>.</exception>
    public static string GetFormattedFullName(this UserProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return profile.GetFullName();
    }

    /// <summary>
    /// Determines if the user profile is eligible for daily reports based on their settings and activity status.
    /// </summary>
    /// <param name="profile">The <see cref="UserProfile"/> instance.</param>
    /// <returns><c>true</c> if eligible; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="profile"/> is <c>null</c>.</exception>
    public static bool IsEligibleForDailyReport(this UserProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return profile.IsActive && profile.ReceiveDailyReport && profile.IsRecentlyActive();
    }

    /// <summary>
    /// Gets the total number of alerts and active alerts for the user.
    /// </summary>
    /// <param name="profile">The <see cref="UserProfile"/> instance.</param>
    /// <returns>An object containing total and active alert counts.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="profile"/> is <c>null</c>.</exception>
    public static (int TotalAlerts, int ActiveAlerts) GetAlertCounts(this UserProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var totalAlerts = profile.Alerts?.Count ?? 0;
        var activeAlerts = profile.GetActiveAlertCount();

        return (totalAlerts, activeAlerts);
    }
}