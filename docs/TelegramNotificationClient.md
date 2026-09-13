# TelegramNotificationClient

`TelegramNotificationClient` implements `ITelegramNotificationClient` by sending text messages through a Telegram bot. It supports direct messages to any numeric chat ID, formatted price alerts to the configured administrator chat, and cache-backed suppression of repeated administrator messages.

The class and its interface are declared in `src/Integration/TelegramNotificationClient.cs` in the `BinanceP2pMonitor.Integration` namespace.

## Construction

```csharp
public TelegramNotificationClient(
    AppSettings appSettings,
    ILogger<TelegramNotificationClient> logger,
    ICache cache)
```

The constructor stores the supplied settings, logger, and cache, and creates a `TelegramBotClient` from `AppSettings.TelegramBotToken`. Passing `null` for any constructor dependency throws `ArgumentNullException`. A missing or invalid bot token may be rejected when the underlying Telegram client is created.

The application registers `ITelegramNotificationClient` with `TelegramNotificationClient` as a singleton. Its configured `ICache` implementation is also a singleton.

The relevant settings are:

| Setting | Used for |
| --- | --- |
| `TelegramBotToken` | Creating the underlying Telegram bot client. |
| `TelegramAdminChatId` | Selecting the destination for price alerts and rate-limited messages. |

`TelegramAdminChatId` is stored as a string but parsed as a signed 64-bit integer when an administrator message is sent. The class does not inspect `EnableTelegramNotifications`; callers or higher-level services are responsible for deciding whether to invoke it.

## Public API

### `SendMessageAsync`

```csharp
public Task<bool> SendMessageAsync(
    long chatId,
    string message,
    CancellationToken ct = default)
```

Sends `message` to `chatId` with Telegram's HTML parse mode enabled. Before sending, it writes a debug log containing the destination and the message truncated to 100 characters. A successful request is logged with the Telegram message ID and returns `true`.

Any exception raised inside the method, including cancellation or an error while preparing the debug log, is caught and logged; the method then returns `false`. It does not rethrow Telegram API, network, formatting, or cancellation errors.

Because HTML parse mode is always used, callers are responsible for supplying valid Telegram-supported HTML and escaping dynamic text when appropriate. This method performs no explicit validation of `chatId` or `message`.

### `SendPriceAlertAsync`

```csharp
public Task<bool> SendPriceAlertAsync(
    string asset,
    string fiat,
    decimal buyPrice,
    decimal sellPrice,
    string alertReason,
    CancellationToken ct = default)
```

Builds an HTML-formatted alert and sends it to `AppSettings.TelegramAdminChatId` through `SendMessageAsync`. The generated message has this shape:

```text
<b>Price Alert: ASSET/FIAT</b>

<b>Buy:</b> 1.2345
<b>Sell:</b> 1.3456

<b>Reason:</b> reason
<b>Time:</b> yyyy-MM-dd HH:mm:ss UTC
```

Buy and sell prices use the `F4` format and therefore contain four fractional digits. The timestamp is obtained from `DateTime.UtcNow`. Interpolated values are not HTML-escaped, and number formatting follows the process's current culture.

The administrator chat ID is parsed with `long.Parse` and invariant culture before `SendMessageAsync` is entered. Consequently, a malformed or out-of-range value throws rather than producing a `false` result. Once sending begins, the return and error behavior is the same as `SendMessageAsync`.

### `SendRateLimitedAsync`

```csharp
public Task<bool> SendRateLimitedAsync(
    string cacheKey,
    string message,
    TimeSpan rateLimitWindow,
    CancellationToken ct = default)
```

Suppresses repeated sends by deriving the cache key `telegram_ratelimit_{cacheKey}` and checking it with `ICache.ExistsAsync`.

- If the derived key exists, the method logs a warning, does not contact Telegram, and returns `false`.
- If it does not exist, the method sends the message to the configured administrator chat.
- After a successful send, it caches the current UTC time under the derived key for `rateLimitWindow`.
- After an unsuccessful send, it does not create the cache entry, so a later call can retry.

The existence check and cache write are separate operations. Concurrent calls with the same key can therefore both observe no entry and both send. Rate limiting is determined by the injected cache rather than by Telegram's API limits; with the application's in-memory cache, entries are local to the running process and are lost on restart.

Unlike `SendMessageAsync`, this method has no encompassing `try`/`catch`. Exceptions from the cache, administrator chat-ID parsing, or the post-send cache write propagate to the caller. In particular, a cache-write failure can make the method throw after Telegram has already accepted the message. Cancellation passed into `SendMessageAsync` is converted there to `false`, while cancellation raised by a cache operation can propagate.

## Return-value semantics

`false` does not always mean the same thing across the API:

| Method | `false` means |
| --- | --- |
| `SendMessageAsync` | The send path raised an exception. |
| `SendPriceAlertAsync` | `SendMessageAsync` returned `false`; chat-ID parsing failures throw instead. |
| `SendRateLimitedAsync` | The message was suppressed by the cache, or the attempted send returned `false`. |

Callers that need to distinguish rate-limit suppression from delivery failure must do so outside this interface; the method exposes both as `false`.

## Example

```csharp
using BinanceP2pMonitor.Integration;

public sealed class PriceAlertPublisher
{
    private readonly ITelegramNotificationClient _telegram;

    public PriceAlertPublisher(ITelegramNotificationClient telegram)
    {
        _telegram = telegram;
    }

    public Task<bool> PublishAsync(CancellationToken cancellationToken)
    {
        return _telegram.SendPriceAlertAsync(
            asset: "USDT",
            fiat: "EUR",
            buyPrice: 0.9234m,
            sellPrice: 0.9312m,
            alertReason: "Spread threshold exceeded",
            ct: cancellationToken);
    }
}
```

For a cache-suppressed notification, choose a stable logical key and a window:

```csharp
bool sent = await telegram.SendRateLimitedAsync(
    cacheKey: "exchange-connectivity",
    message: "<b>Warning:</b> Binance connectivity is degraded.",
    rateLimitWindow: TimeSpan.FromMinutes(15),
    ct: cancellationToken);
```

Here, `false` can mean either that the key was already present or that delivery failed.
