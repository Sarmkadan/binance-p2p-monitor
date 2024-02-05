# EventBus

A lightweight in-memory event bus for decoupled communication between components in the Binance P2P Monitor project. It supports asynchronous event publishing and synchronous subscription handling.

## API

### `public EventBus()`

Initializes a new instance of the `EventBus` class.

### `public async Task PublishAsync<TEvent>(TEvent @event)`

Publishes an event of type `TEvent` to all registered subscribers.

- **@event**: The event instance to publish.
- **Return value**: A `Task` representing the asynchronous operation.
- **Exceptions**: Throws `ArgumentNullException` if `@event` is `null`.

### `public async Task PublishManyAsync<TEvent>(IEnumerable<TEvent> events)`

Publishes multiple events of type `TEvent` to all registered subscribers.

- **events**: A collection of event instances to publish.
- **Return value**: A `Task` representing the asynchronous operation.
- **Exceptions**: Throws `ArgumentNullException` if `events` is `null`.

### `public void Subscribe<TEvent>(Action<TEvent> handler)`

Registers a handler for events of type `TEvent`.

- **handler**: The delegate to invoke when an event of type `TEvent` is published.
- **Exceptions**: Throws `ArgumentNullException` if `handler` is `null`.

### `public void Unsubscribe<TEvent>(Action<TEvent> handler)`

Removes a previously registered handler for events of type `TEvent`.

- **handler**: The delegate to remove from the subscription list.
- **Exceptions**: Throws `ArgumentNullException` if `handler` is `null`.

## Usage

### Example 1: Basic Event Publishing and Subscription
