#nullable enable

using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using BinanceP2pMonitor.Caching;
using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Utilities;
using Microsoft.Extensions.Logging;
using BinanceP2pMonitor.Configuration; // added using directive

namespace BinanceP2pMonitor.Integration;

/// <summary>
/// Interface for sending notifications via Telegram.
/// </summary>
public interface ITelegramNotificationClient
{
    /// <summary>
    /// Sends a message to the specified chat ID.
    /// </summary>
    /// <param name="chatId">The ID of the chat to send the message to.</param>
    /// <param name="message">The message to send.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>True if the message was sent successfully, false otherwise.</returns>
    Task<bool> SendMessageAsync(long chatId, string message, CancellationToken ct = default);

    /// <summary>
    /// Sends a price alert message to the Telegram admin chat.
    /// </summary>
    /// <param name="asset">The asset being alerted.</param>
    /// <param name="fiat">The fiat currency being alerted.</param>
    /// <param name="buyPrice">The buy price of the asset.</param>
    /// <param name="sellPrice">The sell price of the asset.</param>
    /// <param name="alertReason">The reason for the alert.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>True if the message was sent successfully, false otherwise.</returns>
    Task<bool> SendPriceAlertAsync(string asset, string fiat, decimal buyPrice, decimal sellPrice, string alertReason, CancellationToken ct = default);

    /// <summary>
    /// Sends a rate-limited message to the Telegram admin chat.
    /// </summary>
    /// <param name="cacheKey">The cache key to check for rate limiting.</param>
    /// <param name="message">The message to send.</param>
    /// <param name="rateLimitWindow">The rate limit window.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>True if the message was sent successfully, false otherwise.</returns>
    Task<bool> SendRateLimitedAsync(string cacheKey, string message, TimeSpan rateLimitWindow, CancellationToken ct = default);
}

/// <summary>
/// Implementation of the ITelegramNotificationClient interface.
/// </summary>
public class TelegramNotificationClient : ITelegramNotificationClient
{
    private readonly TelegramBotClient _botClient;
    private readonly ILogger<TelegramNotificationClient> _logger;
    private readonly ICache _cache;
    private readonly AppSettings _appSettings;

    /// <summary>
    /// Initializes a new instance of the TelegramNotificationClient class.
    /// </summary>
    /// <param name="appSettings">The application settings.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cache">The cache.</param>
    public TelegramNotificationClient(
        AppSettings appSettings,
        ILogger<TelegramNotificationClient> logger,
        ICache cache)
    {
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _botClient = new TelegramBotClient(_appSettings.TelegramBotToken);
    }

    /// <summary>
    /// Sends a message to the specified chat ID.
    /// </summary>
    /// <param name="chatId">The ID of the chat to send the message to.</param>
    /// <param name="message">The message to send.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>True if the message was sent successfully, false otherwise.</returns>
    public async Task<bool> SendMessageAsync(long chatId, string message, CancellationToken ct = default)
    {
        try
        {
            _logger.LogDebug("Sending Telegram message to {ChatId}: {Message}", chatId, message.Truncate(100));
            var sentMessage = await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: message,
                parseMode: ParseMode.Html,
                cancellationToken: ct);

            _logger.LogInformation("Message sent. MessageId: {MessageId}", sentMessage.MessageId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Telegram message to {ChatId}", chatId);
            return false;
        }
    }

    /// <summary>
    /// Sends a price alert message to the Telegram admin chat.
    /// </summary>
    /// <param name="asset">The asset being alerted.</param>
    /// <param name="fiat">The fiat currency being alerted.</param>
    /// <param name="buyPrice">The buy price of the asset.</param>
    /// <param name="sellPrice">The sell price of the asset.</param>
    /// <param name="alertReason">The reason for the alert.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>True if the message was sent successfully, false otherwise.</returns>
    public async Task<bool> SendPriceAlertAsync(string asset, string fiat, decimal buyPrice, decimal sellPrice, string alertReason, CancellationToken ct = default)
    {
        var message = $"<b>Price Alert: {asset}/{fiat}</b>\n\n" +
                      $"<b>Buy:</b> {buyPrice:F4}\n" +
                      $"<b>Sell:</b> {sellPrice:F4}\n\n" +
                      $"<b>Reason:</b> {alertReason}\n" +
                      $"<b>Time:</b> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";
        return await SendMessageAsync(long.Parse(_appSettings.TelegramAdminChatId), message, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a rate-limited message to the Telegram admin chat.
    /// </summary>
    /// <param name="cacheKey">The cache key to check for rate limiting.</param>
    /// <param name="message">The message to send.</param>
    /// <param name="rateLimitWindow">The rate limit window.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>True if the message was sent successfully, false otherwise.</returns>
    public async Task<bool> SendRateLimitedAsync(string cacheKey, string message, TimeSpan rateLimitWindow, CancellationToken ct = default)
    {
        var lastSentKey = $"telegram_ratelimit_{cacheKey}";
        var exists = await _cache.ExistsAsync(lastSentKey, ct).ConfigureAwait(false);
        if (exists)
        {
            _logger.LogWarning("Message rate limited for key: {CacheKey}", cacheKey);
            return false;
        }

        var success = await SendMessageAsync(long.Parse(_appSettings.TelegramAdminChatId), message, ct).ConfigureAwait(false);
        if (success)
            await _cache.SetAsync(lastSentKey, DateTime.UtcNow, rateLimitWindow, ct).ConfigureAwait(false);

        return success;
    }
}
