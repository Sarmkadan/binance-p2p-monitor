#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BinanceP2pMonitor.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Xunit;

namespace BinanceP2pMonitor.Tests;

public class WebSocketServiceTests
{
    private readonly ILogger<WebSocketService> _mockLogger;
    private readonly WebSocketService _webSocketService;

    public WebSocketServiceTests()
    {
        _mockLogger = Substitute.For<ILogger<WebSocketService>>();
        // It's tricky to mock ClientWebSocket directly, so we'll test the logic around it.
        // For a true integration test, a real WebSocket server would be needed.
        _webSocketService = new WebSocketService(_mockLogger);
    }

    [Fact]
    public async Task ConnectAsync_ShouldConnectSuccessfully()
    {
        // Arrange

        // Act
        await _webSocketService.ConnectAsync().ConfigureAwait(false);

        // Assert
        _webSocketService.IsConnected.Should().BeTrue();
        _mockLogger.Received(1).LogInformation(Arg.Any<string>(), "WebSocket connected successfully");
    }

    [Fact]
    public async Task DisconnectAsync_ShouldDisconnectSuccessfully()
    {
        // Arrange
        await _webSocketService.ConnectAsync().ConfigureAwait(false);

        // Act
        await _webSocketService.DisconnectAsync().ConfigureAwait(false);

        // Assert
        _webSocketService.IsConnected.Should().BeFalse();
        _mockLogger.Received(1).LogInformation(Arg.Any<string>(), "WebSocket disconnected");
    }

    [Fact]
    public async Task SubscribeToPairAsync_ShouldSubscribeAndAddPair()
    {
        // Arrange
        await _webSocketService.ConnectAsync().ConfigureAwait(false);
        var asset = "BTC";
        var fiat = "USDT";

        // Act
        await _webSocketService.SubscribeToPairAsync(asset, fiat).ConfigureAwait(false);

        // Assert
        // We can't directly verify SendMessageAsync as it's private.
        // We'll rely on the fact that the public SubscribeToPairAsync calls it
        // and that the _subscribedPairs internal state is correctly managed.
        _mockLogger.Received(1).LogInformation(Arg.Any<string>(), "Subscribed to {Asset}/{Fiat}", asset, fiat);
        // Additional assertion to indirectly check subscription:
        // Attempt to subscribe again, it should return without sending a new message
        await _webSocketService.SubscribeToPairAsync(asset, fiat).ConfigureAwait(false);
        _mockLogger.Received(1).LogInformation(Arg.Any<string>(), "Subscribed to {Asset}/{Fiat}", asset, fiat); // Should still be 1 call
    }

    [Fact]
    public async Task Reconnection_ShouldRestoreSubscriptions()
    {
        // Arrange
        var asset1 = "BTC";
        var fiat1 = "USDT";
        var asset2 = "ETH";
        var fiat2 = "BUSD";

        await _webSocketService.ConnectAsync().ConfigureAwait(false);
        await _webSocketService.SubscribeToPairAsync(asset1, fiat1).ConfigureAwait(false);
        await _webSocketService.SubscribeToPairAsync(asset2, fiat2).ConfigureAwait(false);

        // Simulate disconnection by making the service think it's disconnected
        // This is a workaround as we can't directly mock ClientWebSocket's state changes easily.
        // In a real scenario, ClientWebSocket's state would change due to network issues.
        await _webSocketService.DisconnectAsync().ConfigureAwait(false);

        // Act - Reconnect
        await _webSocketService.ConnectAsync().ConfigureAwait(false); // This should trigger re-subscription logic

        // Assert
        _webSocketService.IsConnected.Should().BeTrue();
        // Verify that re-subscription logs were made for each pair
        _mockLogger.Received(1).LogInformation(Arg.Any<string>(), "Re-subscribing to {Asset}/{Fiat} after reconnection", asset1.ToUpper(), fiat1.ToUpper());
        _mockLogger.Received(1).LogInformation(Arg.Any<string>(), "Re-subscribing to {Asset}/{Fiat} after reconnection", asset2.ToUpper(), fiat2.ToUpper());
    }
}
