# ITelegramNotificationClient
The `ITelegramNotificationClient` type is designed to facilitate sending notifications to Telegram users, providing a simple and efficient way to communicate with users of the binance-p2p-monitor project. It offers methods for sending various types of messages, allowing for flexible notification handling.

## API
### TelegramNotificationClient
The `TelegramNotificationClient` constructor is used to create an instance of the `ITelegramNotificationClient` type, initializing it for subsequent use.

### SendMessageAsync
Sends a generic message to a Telegram user. 
- Parameters: Not specified in the provided information.
- Return Value: `bool` indicating whether the operation was successful.
- Exceptions: Not specified, but it is expected to throw exceptions in case of network errors, authentication issues, or other failures.

### SendPriceAlertAsync
Sends a price alert message to a Telegram user. 
- Parameters: Not specified in the provided information.
- Return Value: `bool` indicating whether the operation was successful.
- Exceptions: Similar to `SendMessageAsync`, expected to throw exceptions for network errors, authentication issues, or other failures.

### SendRateLimitedAsync
Sends a rate-limited message to a Telegram user, presumably handling rate limits imposed by Telegram.
- Parameters: Not specified in the provided information.
- Return Value: `bool` indicating whether the operation was successful.
- Exceptions: Expected to throw exceptions for network errors, authentication issues, rate limit breaches, or other failures.

## Usage
The following examples demonstrate how to use the `ITelegramNotificationClient` type:
```csharp
// Example 1: Sending a generic message
var client = new TelegramNotificationClient();
bool success = await client.SendMessageAsync();
if (success)
{
    Console.WriteLine("Message sent successfully.");
}
else
{
    Console.WriteLine("Failed to send message.");
}

// Example 2: Sending a price alert
var client2 = new TelegramNotificationClient();
bool priceAlertSuccess = await client2.SendPriceAlertAsync();
if (priceAlertSuccess)
{
    Console.WriteLine("Price alert sent successfully.");
}
else
{
    Console.WriteLine("Failed to send price alert.");
}
```

## Notes
- **Thread Safety**: The `ITelegramNotificationClient` type and its methods are expected to be thread-safe, allowing for concurrent access and message sending without fear of data corruption or other threading issues.
- **Rate Limiting**: The `SendRateLimitedAsync` method is designed to handle Telegram's rate limits, preventing the application from being banned or restricted due to excessive messaging.
- **Error Handling**: It is crucial to handle exceptions and errors properly when using the `ITelegramNotificationClient` type, ensuring that the application remains stable and functional even in the face of network errors or other issues.
- **Parameter Specification**: While the provided information does not specify the parameters for the `SendMessageAsync`, `SendPriceAlertAsync`, and `SendRateLimitedAsync` methods, it is essential to consult the actual method signatures or documentation for accurate parameter information.
