#nullable enable

using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Integration;
using BinanceP2pMonitor.Caching;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using NSubstitute.ReturnsExtensions;

namespace BinanceP2pMonitor.Tests;

public class TelegramNotificationClientTests
{
    private readonly ITelegramBotClientWrapper _mockBotClientWrapper;
    private readonly ILogger<TelegramNotificationClient> _mockLogger;
    private readonly ICache _mockCache;
    private readonly AppSettings _appSettings;
    private readonly TelegramNotificationClient _telegramNotificationClient;

    public TelegramNotificationClientTests()
    {
        _mockBotClientWrapper = Substitute.For<ITelegramBotClientWrapper>();
        _mockLogger = Substitute.For<ILogger<TelegramNotificationClient>>();
        _mockCache = Substitute.For<ICache>();
        _appSettings = new AppSettings
        {
            TelegramBotToken = "test_token",
            TelegramAdminChatId = "123456789"
        };
        _telegramNotificationClient = new TelegramNotificationClient(
            _mockBotClientWrapper,
            _appSettings,
            _mockLogger,
            _mockCache);
    }

    [Fact]
    public async Task SendMessageAsync_ShouldReturnTrueAndLogSuccess_OnSuccessfulSend()
    {
        // Arrange
        var chatId = 123L;
        var messageText = "Test Message";
        var sentMessage = new Message { MessageId = 1, Text = messageText };
        _mockBotClientWrapper.SendTextMessageAsync(
            chatId,
            messageText,
            ParseMode.Html,
            null, // entities
            null, // disableWebPagePreview
            null, // disableNotification
            null, // replyToMessageId
            null, // allowSendingWithoutReply
            null, // replyMarkup
            Arg.Any<CancellationToken>())
            .Returns(sentMessage);

        // Act
        var result = await _telegramNotificationClient.SendMessageAsync(chatId, messageText);

        // Assert
        result.Should().BeTrue();
        _mockLogger.Received(1).LogDebug(
            "Sending Telegram message to {ChatId}: {Message}", chatId, messageText);
        _mockLogger.Received(1).LogInformation(
            "Message sent successfully. Message ID: {MessageId}", sentMessage.MessageId);
    }

    [Fact]
    public async Task SendMessageAsync_ShouldReturnFalseAndLogError_OnFailure()
    {
        // Arrange
        var chatId = 123L;
        var messageText = "Test Message";
        var exception = new Exception("Telegram API error");
        _mockBotClientWrapper.SendTextMessageAsync(
            chatId,
            messageText,
            ParseMode.Html,
            null, // entities
            null, // disableWebPagePreview
            null, // disableNotification
            null, // replyToMessageId
            null, // allowSendingWithoutReply
            null, // replyMarkup
            Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        // Act
        var result = await _telegramNotificationClient.SendMessageAsync(chatId, messageText);

        // Assert
        result.Should().BeFalse();
        _mockLogger.Received(1).LogError(
            exception,
            "Failed to send Telegram message to {ChatId}", chatId);
    }

    [Fact]
    public async Task SendPriceAlertAsync_ShouldCallSendMessageAsyncWithFormattedMessage()
    {
        // Arrange
        var asset = "BTC";
        var fiat = "USD";
        var buyPrice = 1000m;
        var sellPrice = 1010m;
        var alertReason = "High Spread";
        var expectedMessage = $@"
<b>⚠️ Price Alert: {asset}/{fiat}</b>

<b>Buy Price:</b> {buyPrice:F8}
<b>Sell Price:</b> {sellPrice:F8}

<b>Reason:</b> {alertReason}
<b>Time:</b> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
";
        // Mock SendMessageAsync within the client to return true
        _mockBotClientWrapper.SendTextMessageAsync(
            long.Parse(_appSettings.TelegramAdminChatId),
            Arg.Is<string>(s => s.Contains("High Spread")), // Use Arg.Is to check content without exact timestamp
            ParseMode.Html,
            null, null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(new Message { MessageId = 1 });

        // Act
        var result = await _telegramNotificationClient.SendPriceAlertAsync(asset, fiat, buyPrice, sellPrice, alertReason);

        // Assert
        result.Should().BeTrue();
        // Verify that SendMessageAsync was called with the correct chatId and a message containing parts of the expected message
        await _mockBotClientWrapper.Received(1).SendTextMessageAsync(
            long.Parse(_appSettings.TelegramAdminChatId),
            Arg.Is<string>(s => s.Contains(asset) && s.Contains(fiat) && s.Contains(alertReason)),
            ParseMode.Html,
            Arg.Any<IEnumerable<MessageEntity>>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<int?>(),
            Arg.Any<bool?>(),
            Arg.Any<IReplyMarkup>(),
            Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task SendTestMessageAsync_ShouldCallSendMessageAsyncWithTestMessage()
    {
        // Arrange
        // Mock SendMessageAsync within the client to return true
        _mockBotClientWrapper.SendTextMessageAsync(
            long.Parse(_appSettings.TelegramAdminChatId),
            Arg.Is<string>(s => s.Contains("BinanceP2pMonitor is running")), // Check for part of the message
            ParseMode.Html,
            null, null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(new Message { MessageId = 1 });

        // Act
        var result = await _telegramNotificationClient.SendTestMessageAsync();

        // Assert
        result.Should().BeTrue();
        // Verify that SendMessageAsync was called with the correct chatId and a message containing parts of the expected message
        await _mockBotClientWrapper.Received(1).SendTextMessageAsync(
            long.Parse(_appSettings.TelegramAdminChatId),
            Arg.Is<string>(s => s.Contains("BinanceP2pMonitor is running")),
            ParseMode.Html,
            Arg.Any<IEnumerable<MessageEntity>>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<int?>(),
            Arg.Any<bool?>(),
            Arg.Any<IReplyMarkup>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendRateLimitedAsync_ShouldSendMessageAndSetCache_WhenNotRateLimited()
    {
        // Arrange
        var cacheKey = "some_key";
        var message = "Rate limited message";
        var rateLimitWindow = TimeSpan.FromMinutes(1);
        _mockCache.ExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _mockBotClientWrapper.SendTextMessageAsync(
            Arg.Any<long>(),
            Arg.Any<string>(),
            Arg.Any<ParseMode>(),
            null, null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(new Message { MessageId = 1 });

        // Act
        var result = await _telegramNotificationClient.SendRateLimitedAsync(cacheKey, message, rateLimitWindow);

        // Assert
        result.Should().BeTrue();
        await _mockBotClientWrapper.Received(1).SendTextMessageAsync(
            long.Parse(_appSettings.TelegramAdminChatId), message, ParseMode.Html,
            Arg.Any<IEnumerable<MessageEntity>>(), Arg.Any<bool?>(), Arg.Any<bool?>(), Arg.Any<int?>(), Arg.Any<bool?>(), Arg.Any<IReplyMarkup>(), Arg.Any<CancellationToken>());
        await _mockCache.Received(1).SetAsync(
            $"telegram_ratelimit_{cacheKey}",
            Arg.Any<DateTime>(),
            rateLimitWindow,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendRateLimitedAsync_ShouldNotSendMessageAndNotSetCache_WhenRateLimited()
    {
        // Arrange
        var cacheKey = "some_key";
        var message = "Rate limited message";
        var rateLimitWindow = TimeSpan.FromMinutes(1);
        _mockCache.ExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _telegramNotificationClient.SendRateLimitedAsync(cacheKey, message, rateLimitWindow);

        // Assert
        result.Should().BeFalse();
        _mockLogger.Received(1).LogWarning("Message rate limited for key: {CacheKey}", cacheKey);
        await _mockBotClientWrapper.DidNotReceive().SendTextMessageAsync(
            Arg.Any<long>(), Arg.Any<string>(), Arg.Any<ParseMode>(),
            Arg.Any<IEnumerable<MessageEntity>>(), Arg.Any<bool?>(), Arg.Any<bool?>(), Arg.Any<int?>(), Arg.Any<bool?>(), Arg.Any<IReplyMarkup>(), Arg.Any<CancellationToken>());
        await _mockCache.DidNotReceive().SetAsync(
            Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendRateLimitedAsync_ShouldNotSetCache_WhenSendMessageFails()
    {
        // Arrange
        var cacheKey = "some_key";
        var message = "Rate limited message";
        var rateLimitWindow = TimeSpan.FromMinutes(1);
        _mockCache.ExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _mockBotClientWrapper.SendTextMessageAsync(
            Arg.Any<long>(),
            Arg.Any<string>(),
            Arg.Any<ParseMode>(),
            null, null, null, null, null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Send failed"));

        // Act
        var result = await _telegramNotificationClient.SendRateLimitedAsync(cacheKey, message, rateLimitWindow);

        // Assert
        result.Should().BeFalse();
        await _mockCache.DidNotReceive().SetAsync(
            Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
        _mockLogger.Received(1).LogError(
            Arg.Any<Exception>(),
            "Failed to send Telegram message to {ChatId}", long.Parse(_appSettings.TelegramAdminChatId));
    }
}
