#nullable enable

using System;

namespace BinanceP2pMonitor.Exceptions;

/// <summary>
/// Thrown when notification delivery fails
/// </summary>
public class NotificationException : BinanceP2pException
{
    public NotificationException(string message, string? errorCode = "NOTIFICATION_ERROR")
        : base(message, errorCode)
    {
        ArgumentNullException.ThrowIfNull(message);
    }

    public NotificationException(string message, Exception innerException, string? errorCode = "NOTIFICATION_ERROR")
        : base(message, innerException, errorCode)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(innerException);
    }
}

/// <summary>
/// Thrown when Telegram notification delivery fails
/// </summary>
public class TelegramNotificationException : NotificationException
{
    public TelegramNotificationException(string message, Exception innerException)
        : base(message, innerException, "TELEGRAM_ERROR")
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(innerException);
    }
}

/// <summary>
/// Thrown when webhook notification delivery fails
/// </summary>
public class WebhookNotificationException : NotificationException
{
    public WebhookNotificationException(string message, Exception innerException)
        : base(message, innerException, "WEBHOOK_ERROR")
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(innerException);
    }
}
