#nullable enable

using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using BinanceP2pMonitor.Caching;
using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Utilities;
using Microsoft.Extensions.Logging;

namespace BinanceP2pMonitor.Integration;

public interface ITelegramNotificationClient
{
    Task<bool> SendMessageAsync(long chatId, string message, CancellationToken ct = default);
    Task<bool> SendPriceAlertAsync(string asset, string fiat, decimal buyPrice, decimal sellPrice, string alertReason, CancellationToken ct = default);
    Task<bool> SendRateLimitedAsync(string cacheKey, string message, TimeSpan rateLimitWindow, CancellationToken ct = default);
}

public class TelegramNotificationClient : ITelegramNotificationClient
{
    private readonly TelegramBotClient _botClient;
    private readonly ILogger<TelegramNotificationClient> _logger;
    private readonly ICache _cache;
    private readonly AppSettings _appSettings;

    public TelegramNotificationClient(
        AppSettings appSettings,
        ILogger<TelegramNotificationClient> logger,
        ICache cache)
    {
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _botClient = new TelegramBotClient(appSettings.TelegramBotToken);
    }

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

    public async Task<bool> SendPriceAlertAsync(string asset, string fiat, decimal buyPrice, decimal sellPrice, string alertReason, CancellationToken ct = default)
    {
        var message = $"<b>Price Alert: {asset}/{fiat}</b>\n\n" +
                      $"<b>Buy:</b> {buyPrice:F4}\n" +
                      $"<b>Sell:</b> {sellPrice:F4}\n\n" +
                      $"<b>Reason:</b> {alertReason}\n" +
                      $"<b>Time:</b> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";
        return await SendMessageAsync(long.Parse(_appSettings.TelegramAdminChatId), message, ct).ConfigureAwait(false);
    }

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
