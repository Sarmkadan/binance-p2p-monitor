# WebhookNotificationClient

`WebhookNotificationClient` implements `IWebhookNotificationClient` by posting JSON alert payloads to the URL in `AppSettings.WebhookUrl`. It supports caller-supplied `WebhookPayload` instances and a convenience method that constructs price-alert payloads.

The client, its interface, and `WebhookPayload` are declared in `src/Integration/WebhookNotificationClient.cs` in the `BinanceP2pMonitor.Integration` namespace.

## Construction

```csharp
public WebhookNotificationClient(
    IHttpClientFactory httpClientFactory,
    AppSettings appSettings,
    ILogger<WebhookNotificationClient> logger)
```

The constructor obtains an `HttpClient` named `WebhookNotificationClient` from `IHttpClientFactory` and retains it for later requests. It stores the supplied settings and logger; passing `null` for either of those dependencies throws `ArgumentNullException`.

The constructor does not explicitly check `httpClientFactory` for `null`. Because `CreateClient` is called immediately, a `null` factory results in `NullReferenceException` before the other arguments are checked.

The application registers `IWebhookNotificationClient` with `WebhookNotificationClient` as a singleton after calling `AddHttpClient`. No endpoint-specific named-client configuration is applied by the repository.

## Configuration

`WebhookUrl` is the only setting read directly by this class. If it is null, empty, or whitespace, `SendAlertAsync` logs that delivery was skipped and returns `false` without serializing the payload or making an HTTP request.

The client does not inspect `EnableWebhookNotifications`. The application's `AlertService` checks both that flag and `WebhookUrl` before invoking `SendPriceAlertAsync`, but direct callers are responsible for applying any enablement policy themselves.

## Public API

### `SendAlertAsync`

```csharp
public Task<bool> SendAlertAsync(
    WebhookPayload payload,
    CancellationToken ct = default)
```

Serializes `payload` with `System.Text.Json` and sends it in an HTTP `POST` to the current value of `AppSettings.WebhookUrl`. The request body uses UTF-8 and the media type `application/json`.

Serialization uses camel-case property names and compact output. Default-valued properties are not omitted, so a null `CustomData` value is written as JSON `null`. The method performs no explicit payload validation; at runtime, even a null payload is serialized as the JSON literal `null`.

The method returns:

- `true` for any HTTP success status, as determined by `HttpResponseMessage.IsSuccessStatusCode` (a 2xx response).
- `false` when the URL is not configured, the endpoint returns a non-success status, or the send path raises an exception.

Exceptions from serialization, URI processing, HTTP transport, and cancellation are caught, logged, and converted to `false`. Callers should therefore use both the result and logs when diagnosing failures; cancellation is not rethrown as `OperationCanceledException`.

No retry, timeout, authentication, signature, custom-header, or response-body handling is implemented by this class. Any timeout or default headers supplied through the created `HttpClient` still apply.

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

Creates a new `WebhookPayload` with:

| Property | Assigned value |
| --- | --- |
| `Event` | `"price_alert"` |
| `Asset` | `asset` |
| `Fiat` | `fiat` |
| `BuyPrice` | `buyPrice` |
| `SellPrice` | `sellPrice` |
| `AlertReason` | `alertReason` |
| `Timestamp` | `DateTimeOffset.UtcNow` at payload creation |
| `CustomData` | `null` |

It then returns the task from `SendAlertAsync`, forwarding the cancellation token. It does not validate or normalize the supplied symbols, prices, or reason.

## `WebhookPayload`

```csharp
public sealed class WebhookPayload
{
    public string Event { get; set; } = "alert";
    public string Asset { get; set; } = string.Empty;
    public string Fiat { get; set; } = string.Empty;
    public decimal BuyPrice { get; set; }
    public decimal SellPrice { get; set; }
    public string AlertReason { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public string? CustomData { get; set; }
}
```

With the client's serializer settings, a default payload has this general JSON shape:

```json
{
  "event": "alert",
  "asset": "",
  "fiat": "",
  "buyPrice": 0,
  "sellPrice": 0,
  "alertReason": "",
  "timestamp": "2026-09-13T12:34:56.789+00:00",
  "customData": null
}
```

The timestamp shown is illustrative. `Timestamp` defaults to the UTC instant at which the payload instance is created. `CustomData` is a nullable string; content that looks like JSON remains a JSON string and is escaped rather than embedded as an object.

## Logging and delivery semantics

The client logs:

- a debug message when no URL is configured;
- a debug message before an HTTP request;
- an information message with the numeric status code for successful delivery;
- a warning with the numeric status code for a non-success response; and
- an error with the exception and URL when the send path fails.

A `true` result means only that the endpoint returned a 2xx status. The client does not verify that the receiver processed the event. A `false` result combines disabled/unconfigured delivery, endpoint rejection, cancellation, serialization failure, invalid URL, and transport failure.

## Example

```csharp
using BinanceP2pMonitor.Integration;

public sealed class PriceAlertPublisher
{
    private readonly IWebhookNotificationClient _webhook;

    public PriceAlertPublisher(IWebhookNotificationClient webhook)
    {
        _webhook = webhook;
    }

    public Task<bool> PublishAsync(CancellationToken cancellationToken)
    {
        return _webhook.SendPriceAlertAsync(
            asset: "USDT",
            fiat: "EUR",
            buyPrice: 0.9234m,
            sellPrice: 0.9312m,
            alertReason: "Spread threshold exceeded",
            ct: cancellationToken);
    }
}
```

For a custom event, construct the payload explicitly:

```csharp
bool delivered = await webhook.SendAlertAsync(
    new WebhookPayload
    {
        Event = "monitoring_degraded",
        Asset = "BTC",
        Fiat = "USD",
        AlertReason = "Price feed is stale",
        CustomData = "source=binance-p2p"
    },
    cancellationToken);
```
