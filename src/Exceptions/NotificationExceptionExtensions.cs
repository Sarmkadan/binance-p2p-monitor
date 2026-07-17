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
}