#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Integration;

/// <summary>
/// Client for sending Telegram notifications
/// </summary>
public class TelegramNotificationClient
{
    private readonly TelegramBotClient _botClient;
    private readonly string _chatId;
    private readonly ILogger<TelegramNotificationClient> _logger;
    private readonly ICache _cache;

    public TelegramNotificationClient(
        AppSettings appSettings,
        ILogger<TelegramNotificationClient> logger,
        ICache cache)
    {
        _botClient = new TelegramBotClient(appSettings.TelegramBotToken);
        _chatId = appSettings.TelegramAdminChatId;
        _logger = logger;
        _cache = cache;
    }

    /// <summary>
    /// Sends a text message via Telegram
    /// </summary>
    public async Task<bool> SendMessageAsync(string message, CancellationToken ct = default)
    {
        try
        {
            _logger.LogDebug("Sending Telegram message: {Message}", message.Truncate(100));

            var chatIdParsed = long.Parse(_chatId);
            var sentMessage = await _botClient.SendTextMessageAsync(
                chatId: chatIdParsed,
                text: message,
                parseMode: ParseMode.Html,
                cancellationToken: ct);

            _logger.LogInformation("Message sent successfully. Message ID: {MessageId}", sentMessage.MessageId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Telegram message");
            return false;
        }
    }

    /// <summary>
    /// Sends an alert with price information
    /// </summary>
    public async Task<bool> SendPriceAlertAsync(string asset, string fiat, decimal buyPrice, decimal sellPrice, string alertReason, CancellationToken ct = default)
    {
        var message = $@"
<b>⚠️ Price Alert: {asset}/{fiat}</b>

<b>Buy Price:</b> {buyPrice:F8}
<b>Sell Price:</b> {sellPrice:F8}

<b>Reason:</b> {alertReason}
<b>Time:</b> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
";

        return await SendMessageAsync(message, ct);
    }

    /// <summary>
    /// Sends a test message to verify connection
    /// </summary>
    public async Task<bool> SendTestMessageAsync(CancellationToken ct = default)
    {
        var message = $"✅ BinanceP2pMonitor is running\n⏰ {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";
        return await SendMessageAsync(message, ct);
    }

    /// <summary>
    /// Rate limits message sending (max 1 message per 5 seconds)
    /// </summary>
    public async Task<bool> SendRateLimitedAsync(string cacheKey, string message, TimeSpan rateLimitWindow, CancellationToken ct = default)
    {
        var lastSentKey = $"telegram_ratelimit_{cacheKey}";
        var exists = await _cache.ExistsAsync(lastSentKey, ct);

        if (exists)
        {
            _logger.LogWarning("Message rate limited for key: {CacheKey}", cacheKey);
            return false;
        }

        var success = await SendMessageAsync(message, ct);
        if (success)
        {
            await _cache.SetAsync(lastSentKey, DateTime.UtcNow, rateLimitWindow, ct);
        }

        return success;
    }
}
