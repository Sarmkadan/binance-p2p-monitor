# WebSocketService
The `WebSocketService` class is designed to establish and manage WebSocket connections, enabling real-time communication for monitoring and tracking purposes, specifically within the context of the Binance P2P monitor project. It provides methods for connecting, disconnecting, subscribing to, and unsubscribing from specific pairs, as well as properties to access current state information.

## API
- `public WebSocketService`: The constructor for the `WebSocketService` class, used to create a new instance.
- `public async Task ConnectAsync`: Establishes a WebSocket connection asynchronously. This method does not take any parameters and does not return a value. It may throw exceptions if the connection attempt fails.
- `public async Task DisconnectAsync`: Closes the existing WebSocket connection asynchronously. Like `ConnectAsync`, it does not take parameters or return a value and may throw if the disconnection attempt fails.
- `public async Task SubscribeToPairAsync`: Subscribes to a specific pair for real-time updates. The method parameters and exact behavior are not specified, but it is expected to throw if the subscription attempt fails.
- `public async Task UnsubscribeFromPairAsync`: Unsubscribes from a previously subscribed pair. Similar to `SubscribeToPairAsync`, the parameters are not detailed, but it may throw exceptions on failure.
- `public string s`: A property that presumably holds a string value related to the service's state or configuration.
- `public decimal b`: A property that holds a decimal value, potentially related to the best price or another financial metric.
- `public decimal a`: Another decimal property, possibly representing the ask price or a similar metric.
- `public long E`: A property with a long value, which could represent an event number, timestamp, or another significant long integer value.
- `public void Dispose`: Disposes of the `WebSocketService` instance, releasing any held resources. This method does not return a value and does not throw exceptions as part of its normal operation.

## Usage
```csharp
// Example 1: Basic Connection and Disconnection
var service = new WebSocketService();
await service.ConnectAsync();
// Perform operations
await service.DisconnectAsync();
service.Dispose();

// Example 2: Subscribing to a Pair
var pairService = new WebSocketService();
await pairService.ConnectAsync();
await pairService.SubscribeToPairAsync(); // Assuming SubscribeToPairAsync has parameters in a real implementation
// Monitor the pair
await pairService.UnsubscribeFromPairAsync(); // Assuming UnsubscribeFromPairAsync has parameters
await pairService.DisconnectAsync();
pairService.Dispose();
```

## Notes
The `WebSocketService` class seems designed for use in a multi-threaded environment, given the asynchronous nature of its connection and disconnection methods. However, the thread-safety of accessing its properties (`s`, `b`, `a`, `E`) is not explicitly stated and should be assumed to require synchronization unless documented otherwise. Edge cases, such as attempting to subscribe to a pair without an active connection or unsubscribing from a pair that was never subscribed to, may result in exceptions or undefined behavior. The `Dispose` method is crucial for releasing resources, especially in environments where the service instance may be long-lived or created frequently.
