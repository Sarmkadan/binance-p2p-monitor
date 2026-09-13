# WebSocketService

`WebSocketService` implements `IWebSocketService` and `IDisposable` for Binance ticker updates. It connects to `wss://stream.binance.com:9443/ws`, maintains a set of subscribed symbols, receives ticker messages, and publishes parsed prices through `OnPriceUpdate`.

The type is in the `BinanceP2pMonitor.Services` namespace.

## Construction

```csharp
public WebSocketService(ILogger<WebSocketService> logger)
```

The constructor requires an `ILogger<WebSocketService>`. Passing `null` throws `ArgumentNullException`. The application registers the implementation as a scoped service for `IWebSocketService`.

## Public API

### `IsConnected`

```csharp
public bool IsConnected { get; }
```

Returns `true` only when the service's connection flag is set and the underlying `ClientWebSocket` is in the `Open` state. It is `false` before connection, after a detected disconnect, and after `DisconnectAsync`.

### `OnPriceUpdate`

```csharp
public event EventHandler<PriceUpdateEventArgs>? OnPriceUpdate;
```

Raised for each valid ticker message whose symbol can be split into an asset and quote currency. The sender is the `WebSocketService` instance.

`PriceUpdateEventArgs` provides:

| Property | Type | Value |
| --- | --- | --- |
| `Asset` | `string` | Uppercase base-asset symbol parsed from the ticker symbol. |
| `Fiat` | `string` | Uppercase quote symbol parsed from the ticker symbol. |
| `BuyPrice` | `decimal` | Binance ticker field `b`, the best bid price. |
| `SellPrice` | `decimal` | Binance ticker field `a`, the best ask price. |
| `UpdateTime` | `DateTime` | Binance event time `E`, converted from Unix milliseconds to UTC. |

Known quote suffixes are checked in this order: `USDT`, `BUSD`, `DAI`, `EUR`, `RUB`, and `GBP`. For other symbols longer than three characters, the final three characters are treated as the quote and the preceding characters as the asset. Malformed JSON, blank symbols, and symbols that cannot be split are logged and do not raise the event.

Event handlers run synchronously on the receive-loop call path. Exceptions thrown by a handler are caught by message processing and logged rather than propagated to the caller that initiated the connection.

### `ConnectAsync`

```csharp
public Task ConnectAsync()
```

Connects to the Binance WebSocket endpoint. If already connected, it returns without doing anything. A successful connection:

1. Creates a new cancellation source and `ClientWebSocket`, disposing the previous instances.
2. Re-subscribes to every pair retained in the service's subscription set.
3. Starts a keepalive timer that sends a JSON ping every 20 minutes.
4. Starts the background receive loop.

Connection failures are logged and wrapped in an `ApiException` with message `Failed to connect to WebSocket` and error code `WEBSOCKET_CONNECT_FAILED`. An `ApiException` already raised while re-subscribing is allowed to propagate unchanged.

The method has no cancellation-token parameter. Its internal token is controlled by the service and is cancelled by `Dispose`; `DisconnectAsync` does not cancel it.

### `DisconnectAsync`

```csharp
public Task DisconnectAsync()
```

Stops the keepalive timer and, when the socket is open, sends a normal WebSocket close with description `Closing`. It then marks the service disconnected. Errors are logged and rethrown unchanged.

Calling this method when the socket is absent or not open still marks the service disconnected and completes normally. The subscribed-pair set is retained, so a later `ConnectAsync` attempts to restore those subscriptions.

### `SubscribeToPairAsync`

```csharp
public Task SubscribeToPairAsync(string asset, string fiat)
```

Subscribes to the lowercase stream `<asset><fiat>@ticker` by sending a Binance `SUBSCRIBE` message. If the service is disconnected, it first calls `ConnectAsync`. A pair already present in the subscription set is ignored.

The pair is added to the set only after the subscription message is sent successfully. Errors are logged and rethrown; send failures are represented by `ApiException`. The method performs no explicit null, empty-string, or symbol-format validation.

### `UnsubscribeFromPairAsync`

```csharp
public Task UnsubscribeFromPairAsync(string asset, string fiat)
```

Sends an `UNSUBSCRIBE` message for the lowercase stream `<asset><fiat>@ticker`, then removes the pair from the subscription set. If the pair is not currently recorded, the method returns without sending a message.

This method does not connect automatically. If a recorded pair is unsubscribed while no connection is open, sending fails with `ApiException` and the pair remains in the set. Other errors are logged and rethrown. As with subscription, there is no explicit argument validation.

### `Dispose`

```csharp
public void Dispose()
```

Disposes the keepalive timer, cancels the receive-loop token, waits up to two seconds for the receive loop, and disposes the socket and cancellation source. It suppresses finalization and tolerates repeated calls.

`Dispose` does not send a normal close frame and does not explicitly reset the internal connection flag. Because the socket is removed, `IsConnected` nevertheless returns `false` after disposal. The class does not expose a separate disposed-state check; using it again after disposal is not documented as a supported lifecycle.

## Extensibility API

```csharp
protected virtual void OnPriceUpdateRaised(PriceUpdateEventArgs args)
```

Derived classes can override this method to customize event dispatch. The base implementation invokes `OnPriceUpdate` with the current service as sender and the supplied event arguments.

## Reconnection behavior

When the server sends a close frame, the service marks itself disconnected and starts reconnection in the background unless cancellation was requested. Other unexpected WebSocket errors also trigger reconnection, except premature-close and invalid-state errors, which stop the receive loop without starting a reconnect attempt.

Reconnection uses up to ten attempts with exponential delays beginning at five seconds (5, 10, 20 seconds, and so on). On a successful connection, all retained subscriptions are sent again. Reconnection is background work and is not directly awaitable through the public API.

## Example

```csharp
using BinanceP2pMonitor.Services;
using Microsoft.Extensions.Logging;

using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole());

using var service = new WebSocketService(
    loggerFactory.CreateLogger<WebSocketService>());

service.OnPriceUpdate += (_, update) =>
{
    Console.WriteLine(
        $"{update.Asset}/{update.Fiat}: bid {update.BuyPrice}, " +
        $"ask {update.SellPrice} at {update.UpdateTime:O}");
};

await service.ConnectAsync();
await service.SubscribeToPairAsync("BTC", "USDT");

// Keep the application alive while updates are required.

await service.UnsubscribeFromPairAsync("BTC", "USDT");
await service.DisconnectAsync();
```

The service itself does not buffer updates for consumers. Subscribe to `OnPriceUpdate` before requesting a pair if the first received update must not be missed.
