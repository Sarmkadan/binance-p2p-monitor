namespace BinanceP2pMonitor.Exceptions;

/// <summary>
/// Provides extension methods for <see cref="NotificationException"/>.
/// </summary>
public static class NotificationExceptionExtensions
{
    /// <summary>
    /// Gets a user-friendly message for the notification exception.
    /// </summary>
    /// <param name="exception">The notification exception.</param>
    /// <returns>A user-friendly message.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    public static string GetUserFriendlyMessage(this NotificationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            TelegramNotificationException => "Failed to send notification via Telegram.",
            WebhookNotificationException => "Failed to send notification via webhook.",
            _ => "An error occurred while sending a notification.",
        };
    }

    /// <summary>
    /// Determines if the notification exception is due to a network issue.
    /// </summary>
    /// <param name="exception">The notification exception.</param>
    /// <returns><c>true</c> if the exception is due to a network issue; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    public static bool IsNetworkRelated(this NotificationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception is WebhookNotificationException;
    }

    /// <summary>
    /// Determines if the notification exception is transient (e.g. can be retried).
    /// Notification delivery failures are considered transient with a cap.
    /// </summary>
    /// <param name="exception">The notification exception.</param>
    /// <returns><c>true</c> as all notification exceptions are transient.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    public static bool IsTransient(this NotificationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return true;
    }

    /// <summary>
    /// Determines if the notification exception is fatal (should not be retried).
    /// Notification delivery failures are transient, not fatal.
    /// </summary>
    /// <param name="exception">The notification exception.</param>
    /// <returns><c>false</c> as notification exceptions are not fatal.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    public static bool IsFatal(this NotificationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return false;
    }
}