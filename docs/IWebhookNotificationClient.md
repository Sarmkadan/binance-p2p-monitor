# IWebhookNotificationClient

This interface defines the contract for sending webhook notifications related to Binance P2P price alerts. It exposes properties representing the current alert context and asynchronous methods to dispatch notifications to a configured webhook endpoint.

## API

### `WebhookNotificationClient` (property)
- **Type**: `WebhookNotificationClient`
- **Description**: Gets the underlying `WebhookNotificationClient` instance associated with this notification context. This property provides access to the client configuration or state.

### `SendAlertAsync` (method)
- **Signature**: `Task<bool> SendAlertAsync()`
- **Description**: Sends an alert notification asynchronously using the current property values.
- **Returns**: `true` if the notification was successfully sent; otherwise, `false`.
- **Throws**: `InvalidOperationException` if required properties (e.g., `Event`, `Asset`, `Fiat`) are not set. `HttpRequestException` if the HTTP request fails.

### `SendPriceAlertAsync` (method)
- **Signature**: `Task<bool> SendPriceAlertAsync()`
- **Description**: Sends a price-specific alert notification asynchronously. This method may format the message differently from `SendAlertAsync`.
- **Returns**: `true` if the notification was successfully sent; otherwise, `false`.
- **Throws**: Same as `SendAlertAsync`.

### `Event` (property)
- **Type**: `string`
- **Description**: Gets or sets the event type (e.g., `"price_alert"`, `"threshold_crossed"`). This value is typically included in the webhook payload.

### `Asset` (property)
- **Type**: `string`
- **Description**: Gets or sets the cryptocurrency asset symbol (e.g., `"BTC"`, `"USDT"`).

### `Fiat` (property)
- **Type**: `string`
- **Description**: Gets or sets the fiat currency code (e.g., `"USD"`, `"EUR"`).

### `BuyPrice` (property)
- **Type**: `decimal`
- **Description**: Gets or sets the current buy price for the asset-fiat pair.

### `SellPrice` (property)
- **Type**: `decimal`
- **Description**: Gets or sets the current sell price for the asset-fiat pair.

### `AlertReason` (property)
- **Type**: `string`
- **Description**: Gets or sets a human-readable reason for the alert (e.g., `"Price dropped below threshold"`).

### `Timestamp` (property)
- **Type**: `DateTimeOffset`
- **Description**: Gets or sets the UTC timestamp when the alert was triggered.

### `CustomData` (property)
- **Type**: `string?`
- **Description**: Gets or sets optional custom data to include in the webhook payload. Can be `null`.

## Usage

### Example 1: Sending a price alert with default client

```csharp
IWebhookNotificationClient notification = new WebhookNotificationClient();
notification.Event = "price_alert";
notification.Asset = "BTC";
notification.Fiat = "USD";
notification.BuyPrice = 45000.00m;
notification.SellPrice = 44980.00m;
notification.AlertReason = "Buy price dropped below $45,000";
notification.Timestamp = DateTimeOffset.UtcNow;

bool sent = await notification.SendPriceAlertAsync();
if (sent)
{
    Console.WriteLine("Alert sent successfully.");
}
```

### Example 2: Using custom data and checking send result

```csharp
IWebhookNotificationClient notification = new WebhookNotificationClient();
notification.Event = "threshold_crossed";
notification.Asset = "ETH";
notification.Fiat = "EUR";
notification.BuyPrice = 2800.50m;
notification.SellPrice = 2799.00m;
notification.AlertReason = "Sell price crossed support level";
notification.Timestamp = DateTimeOffset.UtcNow;
notification.CustomData = "{\"strategy\":\"momentum\"}";

bool success = await notification.SendAlertAsync();
if (!success)
{
    // Log failure or retry
    Console.Error.WriteLine("Failed to send webhook notification.");
}
```

## Notes

- **Thread safety**: The interface does not enforce thread safety. Concurrent modification of properties while a send operation is in progress may lead to inconsistent payloads. Callers should synchronize access if the same instance is used from multiple threads.
- **Required properties**: Both `SendAlertAsync` and `SendPriceAlertAsync` may throw `InvalidOperationException` if `Event`, `Asset`, or `Fiat` are `null` or empty. Ensure these are set before calling.
- **Nullable `CustomData`**: The `CustomData` property is nullable. If set to `null`, it will be omitted from the webhook payload.
- **Return value**: A return value of `false` does not necessarily indicate a network error; it may indicate that the webhook endpoint returned a non-success status code. Check the implementation for specific error handling.
- **Property defaults**: Properties are initialized to their default values (e.g., `decimal` is `0`, `string` is `null`, `DateTimeOffset` is `DateTimeOffset.MinValue`). Callers should explicitly set all relevant properties before sending.
