#nullable enable
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using BinanceP2pMonitor.Exceptions;
using Microsoft.Extensions.Logging;

namespace BinanceP2pMonitor.Services;

/// <summary>
/// Service for managing WebSocket connections to real-time price feeds
/// </summary>
public class WebSocketService : IWebSocketService, IDisposable
{
    private static readonly TimeSpan KeepaliveInterval = TimeSpan.FromMinutes(20);
    private static readonly TimeSpan ReconnectBaseDelay = TimeSpan.FromSeconds(5);
    private const int MaxReconnectAttempts = 10;

    private readonly ILogger<WebSocketService> _logger;
    private ClientWebSocket? _webSocket;
    private bool _isConnected;
    private readonly HashSet<string> _subscribedPairs;
    private CancellationTokenSource? _cancellationTokenSource;
    private Timer? _keepaliveTimer;

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

            _webSocket?.Dispose();
            _webSocket = new ClientWebSocket();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            // Connect to Binance WebSocket endpoint
            var uri = new Uri("wss://stream.binance.com:9443/ws");
            await _webSocket.ConnectAsync(uri, _cancellationTokenSource.Token).ConfigureAwait(false);

            _isConnected = true;
            _logger.LogInformation("WebSocket connected successfully");

            // Re-subscribe to all previously subscribed pairs after reconnection
            foreach (var pairKey in _subscribedPairs.ToList())
            {
                var parsedPair = ParsePairKey(pairKey);
                if (parsedPair.HasValue)
                {
                    var (asset, fiat) = parsedPair.Value;
                    _logger.LogInformation("Re-subscribing to {Asset}/{Fiat} after reconnection", asset, fiat);
                    var subscriptionMessage = new
                    {
                        method = "SUBSCRIBE",
                        @params = new[] { $"{pairKey}@ticker" },
                        id = DateTime.UtcNow.Ticks
                    };
                    var json = System.Text.Json.JsonSerializer.Serialize(subscriptionMessage);
                    await SendMessageAsync(json).ConfigureAwait(false);
                }
            }

            // Start keepalive pings to prevent server-side timeout (~30 min)
            StartKeepaliveTimer();

            // Start listening for messages
            _ = ListenForMessagesAsync(_cancellationTokenSource.Token);
        }
        catch (Exception ex) when (ex is not ApiException)
        {
            _isConnected = false;
            _logger.LogError(ex, "Failed to connect to WebSocket");
            throw new ApiException("Failed to connect to WebSocket", ex, "WEBSOCKET_CONNECT_FAILED");
        }
    }

    /// <summary>
    /// Starts a periodic keepalive timer that sends pings to prevent server-side timeout
    /// </summary>
    private void StartKeepaliveTimer()
    {
        _keepaliveTimer?.Dispose();
        _keepaliveTimer = new Timer(async _ =>
        {
            if (!IsConnected)
                return;
            try
            {
                var pingMessage = System.Text.Json.JsonSerializer.Serialize(new { ping = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() });
                await SendMessageAsync(pingMessage).ConfigureAwait(false);
                _logger.LogDebug("WebSocket keepalive ping sent");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "WebSocket keepalive ping failed");
            }
        }, null, KeepaliveInterval, KeepaliveInterval);
    }

    /// <summary>
    /// Attempts to reconnect with exponential backoff after an unexpected disconnect
    /// </summary>
    private async Task ReconnectAsync(CancellationToken cancellationToken)
    {
        for (int attempt = 1; attempt <= MaxReconnectAttempts; attempt++)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            var delay = TimeSpan.FromSeconds(ReconnectBaseDelay.TotalSeconds * Math.Pow(2, attempt - 1));
            _logger.LogWarning("WebSocket reconnect attempt {Attempt}/{Max} in {Delay}s...",
                attempt, MaxReconnectAttempts, (int)delay.TotalSeconds);

            try
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                await ConnectAsync().ConfigureAwait(false);
                _logger.LogInformation("WebSocket reconnected successfully on attempt {Attempt}", attempt);
                return;
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WebSocket reconnect attempt {Attempt} failed", attempt);
            }
        }

        _logger.LogError("WebSocket failed to reconnect after {Max} attempts", MaxReconnectAttempts);
    }

    /// <summary>
    /// Disconnects from the WebSocket server
    /// </summary>
    public async Task DisconnectAsync()
    {
        try
        {
            _keepaliveTimer?.Dispose();
            _keepaliveTimer = null;

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
                await ConnectAsync().ConfigureAwait(false);

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
            await SendMessageAsync(json).ConfigureAwait(false);

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
            await SendMessageAsync(json).ConfigureAwait(false);

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
                    else if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        _logger.LogDebug("Received binary WebSocket frame, ignoring");
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _isConnected = false;
                        _logger.LogWarning("WebSocket server closed the connection (status: {Status}, description: {Description}). Reconnecting...",
                            result.CloseStatus, result.CloseStatusDescription);

                        _keepaliveTimer?.Dispose();
                        _keepaliveTimer = null;

                        if (!cancellationToken.IsCancellationRequested)
                            _ = ReconnectAsync(cancellationToken);

                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (WebSocketException wsEx) when (wsEx is not ApiException)
                {
                    _isConnected = false;
                    _logger.LogError(wsEx, "WebSocket connection lost unexpectedly. Reconnecting...");

                    _keepaliveTimer?.Dispose();
                    _keepaliveTimer = null;

                    if (!cancellationToken.IsCancellationRequested)
                        _ = ReconnectAsync(cancellationToken);

                    break;
                }
            }
        }
        catch (Exception ex) when (ex is not ApiException)
        {
            _isConnected = false;
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
            var message = JsonSerializer.Deserialize<BinanceTickerMessage>(json);

            if (message == null || string.IsNullOrWhiteSpace(message.s))
            {
                _logger.LogWarning("Received malformed WebSocket message: {Message}", json);
                return;
            }

            // Extract Asset and Fiat from the symbol
            string asset;
            string fiat;
            var symbol = message.s.ToUpper();

            // Common fiat currencies are 3 or 4 characters long
            if (symbol.EndsWith("USDT"))
            {
                fiat = "USDT";
                asset = symbol.Replace("USDT", "");
            }
            else if (symbol.EndsWith("BUSD"))
            {
                fiat = "BUSD";
                asset = symbol.Replace("BUSD", "");
            }
            else if (symbol.EndsWith("DAI"))
            {
                fiat = "DAI";
                asset = symbol.Replace("DAI", "");
            }
            else if (symbol.EndsWith("EUR"))
            {
                fiat = "EUR";
                asset = symbol.Replace("EUR", "");
            }
            else if (symbol.EndsWith("RUB"))
            {
                fiat = "RUB";
                asset = symbol.Replace("RUB", "");
            }
            else if (symbol.EndsWith("GBP"))
            {
                fiat = "GBP";
                asset = symbol.Replace("GBP", "");
            }
            else if (symbol.Length > 3) // Assume last 3 chars are fiat if not matched above
            {
                fiat = symbol.Substring(symbol.Length - 3);
                asset = symbol.Substring(0, symbol.Length - 3);
            }
            else
            {
                _logger.LogWarning("Could not parse asset and fiat from symbol: {Symbol}", symbol);
                return;
            }


            var eventArgs = new PriceUpdateEventArgs
            {
                Asset = asset,
                Fiat = fiat,
                BuyPrice = message.b,
                SellPrice = message.a,
                UpdateTime = DateTimeOffset.FromUnixTimeMilliseconds(message.E).UtcDateTime
            };

            OnPriceUpdateRaised(eventArgs);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error parsing WebSocket JSON message: {Message}", json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing WebSocket message: {Message}", json);
        }
    }

    /// <summary>
    /// Represents a simplified Binance ticker message for deserialization.
    /// </summary>
    private class BinanceTickerMessage
    {
        public string s { get; set; } = string.Empty; // Symbol
        public decimal b { get; set; } // Best bid price
        public decimal a { get; set; } // Best ask price
        public long E { get; set; } // Event time in milliseconds
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
        catch (Exception ex) when (ex is not ApiException)
        {
            _logger.LogError(ex, "Error sending WebSocket message");
            throw new ApiException("Failed to send WebSocket message", ex);
        }
    }

    /// <summary>
    /// Raises the price update event
    /// </summary>
    protected virtual void OnPriceUpdateRaised(PriceUpdateEventArgs args)
    {
        OnPriceUpdate?.Invoke(this, args);
    }

    private (string Asset, string Fiat)? ParsePairKey(string pairKey)
    {
        var symbol = pairKey.ToUpper();

        // Common fiat currencies, ordered by length to prioritize longer matches
        if (symbol.EndsWith("USDT"))
            return (symbol.Replace("USDT", ""), "USDT");
        if (symbol.EndsWith("BUSD"))
            return (symbol.Replace("BUSD", ""), "BUSD");
        if (symbol.EndsWith("DAI"))
            return (symbol.Replace("DAI", ""), "DAI");
        if (symbol.EndsWith("EUR"))
            return (symbol.Replace("EUR", ""), "EUR");
        if (symbol.EndsWith("RUB"))
            return (symbol.Replace("RUB", ""), "RUB");
        if (symbol.EndsWith("GBP"))
            return (symbol.Replace("GBP", ""), "GBP");
        
        // Fallback for 3-character fiats if not matched above and symbol is long enough
        if (symbol.Length > 3)
        {
            var potentialFiat = symbol.Substring(symbol.Length - 3);
            // This is a more generalized assumption, might need to be refined based on actual data
            // For now, assume if it's 3 chars and not a known asset prefix, it's fiat
            // A more robust solution might involve a predefined list of fiats
            return (symbol.Substring(0, symbol.Length - 3), potentialFiat);
        }

        _logger.LogWarning("Could not parse asset and fiat from pair key: {PairKey}", pairKey);
        return null;
    }

    public void Dispose()
    {
        _keepaliveTimer?.Dispose();
        _webSocket?.Dispose();
        _cancellationTokenSource?.Dispose();
        GC.SuppressFinalize(this);
    }
}
