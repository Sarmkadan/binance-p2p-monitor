// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net.WebSockets;
using System.Text;
using BinanceP2pMonitor.Exceptions;
using Microsoft.Extensions.Logging;

namespace BinanceP2pMonitor.Services;

/// <summary>
/// Service for managing WebSocket connections to real-time price feeds
/// </summary>
public class WebSocketService : IWebSocketService, IDisposable
{
    private readonly ILogger<WebSocketService> _logger;
    private ClientWebSocket? _webSocket;
    private bool _isConnected;
    private readonly HashSet<string> _subscribedPairs;
    private CancellationTokenSource? _cancellationTokenSource;

    public event EventHandler<PriceUpdateEventArgs>? OnPriceUpdate;

    public bool IsConnected => _isConnected && _webSocket?.State == WebSocketState.Open;

    public WebSocketService(ILogger<WebSocketService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _subscribedPairs = new HashSet<string>();
    }

    /// <summary>
    /// Connects to the WebSocket server
    /// </summary>
    public async Task ConnectAsync()
    {
        try
        {
            if (IsConnected)
                return;

            _webSocket = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();

            // Connect to Binance WebSocket endpoint
            var uri = new Uri("wss://stream.binance.com:9443/ws");
            await _webSocket.ConnectAsync(uri, _cancellationTokenSource.Token);

            _isConnected = true;
            _logger.LogInformation("WebSocket connected successfully");

            // Start listening for messages
            _ = ListenForMessagesAsync(_cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            _isConnected = false;
            throw new ApiException("Failed to connect to WebSocket", null, "WEBSOCKET_CONNECT_FAILED");
        }
    }

    /// <summary>
    /// Disconnects from the WebSocket server
    /// </summary>
    public async Task DisconnectAsync()
    {
        try
        {
            if (_webSocket?.State == WebSocketState.Open)
            {
                _cancellationTokenSource?.Cancel();
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                    "Closing", CancellationToken.None);
            }

            _isConnected = false;
            _logger.LogInformation("WebSocket disconnected");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting WebSocket");
            throw;
        }
    }

    /// <summary>
    /// Subscribes to price updates for a trading pair
    /// </summary>
    public async Task SubscribeToPairAsync(string asset, string fiat)
    {
        try
        {
            if (!IsConnected)
                await ConnectAsync();

            var pairKey = $"{asset.ToLower()}{fiat.ToLower()}";

            if (_subscribedPairs.Contains(pairKey))
                return;

            var subscriptionMessage = new
            {
                method = "SUBSCRIBE",
                @params = new[] { $"{pairKey}@ticker" },
                id = DateTime.UtcNow.Ticks
            };

            var json = System.Text.Json.JsonSerializer.Serialize(subscriptionMessage);
            await SendMessageAsync(json);

            _subscribedPairs.Add(pairKey);
            _logger.LogInformation("Subscribed to {Asset}/{Fiat}", asset, fiat);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to {Asset}/{Fiat}", asset, fiat);
            throw;
        }
    }

    /// <summary>
    /// Unsubscribes from a trading pair
    /// </summary>
    public async Task UnsubscribeFromPairAsync(string asset, string fiat)
    {
        try
        {
            var pairKey = $"{asset.ToLower()}{fiat.ToLower()}";

            if (!_subscribedPairs.Contains(pairKey))
                return;

            var unsubscriptionMessage = new
            {
                method = "UNSUBSCRIBE",
                @params = new[] { $"{pairKey}@ticker" },
                id = DateTime.UtcNow.Ticks
            };

            var json = System.Text.Json.JsonSerializer.Serialize(unsubscriptionMessage);
            await SendMessageAsync(json);

            _subscribedPairs.Remove(pairKey);
            _logger.LogInformation("Unsubscribed from {Asset}/{Fiat}", asset, fiat);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from {Asset}/{Fiat}", asset, fiat);
            throw;
        }
    }

    /// <summary>
    /// Listens for incoming WebSocket messages
    /// </summary>
    private async Task ListenForMessagesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var buffer = new byte[4096];

            while (!cancellationToken.IsCancellationRequested && IsConnected)
            {
                try
                {
                    var result = await _webSocket!.ReceiveAsync(
                        new ArraySegment<byte>(buffer), cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        ProcessMessage(json);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await DisconnectAsync();
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listening to WebSocket messages");
        }
    }

    /// <summary>
    /// Processes incoming WebSocket message
    /// </summary>
    private void ProcessMessage(string json)
    {
        try
        {
            // TODO: Parse JSON and extract price data
            // For now, this is a placeholder
            _logger.LogDebug("Received WebSocket message: {Message}", json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing WebSocket message");
        }
    }

    /// <summary>
    /// Sends a message through the WebSocket
    /// </summary>
    private async Task SendMessageAsync(string message)
    {
        try
        {
            if (!IsConnected)
                throw new InvalidOperationException("WebSocket is not connected");

            var bytes = Encoding.UTF8.GetBytes(message);
            await _webSocket!.SendAsync(
                new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true,
                _cancellationTokenSource!.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending WebSocket message");
            throw;
        }
    }

    /// <summary>
    /// Raises the price update event
    /// </summary>
    protected virtual void OnPriceUpdateRaised(PriceUpdateEventArgs args)
    {
        OnPriceUpdate?.Invoke(this, args);
    }

    public void Dispose()
    {
        _webSocket?.Dispose();
        _cancellationTokenSource?.Dispose();
        GC.SuppressFinalize(this);
    }
}
