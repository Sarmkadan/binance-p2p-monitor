#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BinanceP2pMonitor.Integration;

public interface ITelegramBotClientWrapper
{
    Task<Message> SendTextMessageAsync(
        long chatId,
        string text,
        ParseMode? parseMode = null,
        IEnumerable<MessageEntity>? entities = null,
        bool? disableWebPagePreview = null,
        bool? disableNotification = null,
        int? replyToMessageId = null,
        bool? allowSendingWithoutReply = null,
        IReplyMarkup? replyMarkup = null,
        CancellationToken cancellationToken = default);
}

public class TelegramBotClientWrapper : ITelegramBotClientWrapper
{
    private readonly TelegramBotClient _botClient;

    public TelegramBotClientWrapper(string token)
    {
        _botClient = new TelegramBotClient(token);
    }

    public async Task<Message> SendTextMessageAsync(
        long chatId,
        string text,
        ParseMode? parseMode = null,
        IEnumerable<MessageEntity>? entities = null,
        bool? disableWebPagePreview = null,
        bool? disableNotification = null,
        int? replyToMessageId = null,
        bool? allowSendingWithoutReply = null,
        IReplyMarkup? replyMarkup = null,
        CancellationToken cancellationToken = default)
    {
        return await _botClient.SendTextMessageAsync(
            chatId,
            text,
            parseMode,
            entities,
            disableWebPagePreview,
            disableNotification,
            replyToMessageId,
            allowSendingWithoutReply,
            replyMarkup,
            cancellationToken);
    }
}
