# IWebSocketService
The `IWebSocketService` type is designed to provide real-time market data for a specific asset and fiat currency pair through a WebSocket connection. It exposes properties that allow consumers to access the current buy and sell prices, as well as the last update time, enabling applications to make informed decisions based on up-to-date market information.

## API
* `public string Asset`: Gets the asset symbol (e.g., BTC, ETH) associated with the WebSocket service.
* `public string Fiat`: Gets the fiat currency (e.g., USDT, EUR) associated with the WebSocket service.
* `public decimal BuyPrice`: Gets the current buy price of the asset in the specified fiat currency.
* `public decimal SellPrice`: Gets the current sell price of the asset in the specified fiat currency.
* `public DateTime UpdateTime`: Gets the timestamp of the last update to the market data.

## Usage
The following examples demonstrate how to use the `IWebSocketService` type in a C# application:
```csharp
// Example 1: Accessing market data
IWebSocketService service = new WebSocketService("BTC", "USDT");
Console.WriteLine($"Buy Price: {service.BuyPrice}, Sell Price: {service.SellPrice}, Last Update: {service.UpdateTime}");

// Example 2: Monitoring price changes
IWebSocketService service2 = new WebSocketService("ETH", "EUR");
decimal previousBuyPrice = service2.BuyPrice;
while (true)
{
    if (service2.BuyPrice != previousBuyPrice)
    {
        Console.WriteLine($"Buy price changed: {previousBuyPrice} -> {service2.BuyPrice}");
        previousBuyPrice = service2.BuyPrice;
    }
    // Implement a delay or other logic to control the monitoring frequency
}
```

## Notes
When using the `IWebSocketService` type, consider the following:
* The `BuyPrice` and `SellPrice` properties may not always reflect the exact market prices due to potential delays in updating the data.
* The `UpdateTime` property can be used to determine the freshness of the market data.
* This type is designed for use in a multithreaded environment, but it is the responsibility of the consumer to ensure thread safety when accessing the properties.
* In cases where the WebSocket connection is lost or the service is unable to retrieve market data, the properties may return default or stale values; it is the responsibility of the consumer to handle such scenarios accordingly.
