#nullable enable

using System;

namespace BinanceP2pMonitor.Exceptions;

/// <summary>
/// Thrown when notification delivery fails
/// </summary>
public class NotificationException : BinanceP2pException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationException"/> class with a message.
    /// </summary>
    /// <param name="message">The error message describing the notification failure.</param>
    /// <param name="errorCode">An optional error code. Defaults to "NOTIFICATION_ERROR".</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
    public NotificationException(string message, string? errorCode = "NOTIFICATION_ERROR")
        : base(message, errorCode)
    {
        ArgumentNullException.ThrowIfNull(message);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationException"/> class
    /// with a message and an inner exception.
    /// </summary>
    /// <param name="message">The error message describing the notification failure.</param>
    /// <param name="innerException">The exception that caused this notification failure.</param>
    /// <param name="errorCode">An optional error code. Defaults to "NOTIFICATION_ERROR".</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> or <paramref name="innerException"/> is null.</exception>
    public NotificationException(string message, Exception innerException, string? errorCode = "NOTIFICATION_ERROR")
        : base(message, innerException, errorCode)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(innerException);
    }

    /// <summary>
    /// Gets a value indicating whether the exception is transient.
    /// </summary>
    public override bool IsTransient => true;
}

/// <summary>
/// Thrown when Telegram notification delivery fails
/// </summary>
public class TelegramNotificationException : NotificationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TelegramNotificationException"/> class
    /// with a message and an inner exception. The error code is always "TELEGRAM_ERROR".
    /// </summary>
    /// <param name="message">The error message describing the Telegram delivery failure.</param>
    /// <param name="innerException">The exception that caused this Telegram delivery failure.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> or <paramref name="innerException"/> is null.</exception>
    public TelegramNotificationException(string message, Exception innerException)
        : base(message, innerException, "TELEGRAM_ERROR")
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(innerException);
    }

    /// <summary>
    /// Gets a value indicating whether the exception is transient.
    /// </summary>
    public override bool IsTransient => true;
}

/// <summary>
/// Thrown when webhook notification delivery fails
/// </summary>
public class WebhookNotificationException : NotificationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookNotificationException"/> class
    /// with a message and an inner exception. The error code is always "WEBHOOK_ERROR".
    /// </summary>
    /// <param name="message">The error message describing the webhook delivery failure.</param>
    /// <param name="innerException">The exception that caused this webhook delivery failure.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> or <paramref name="innerException"/> is null.</exception>
    public WebhookNotificationException(string message, Exception innerException)
        : base(message, innerException, "WEBHOOK_ERROR")
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(innerException);
    }

    /// <summary>
    /// Gets a value indicating whether the exception is transient.
    /// </summary>
    public override bool IsTransient => true;
}
