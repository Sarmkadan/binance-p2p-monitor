#nullable enable
namespace BinanceP2pMonitor.Events;

/// <summary>
/// Interface for publishing domain events
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes an event to all registered subscribers
    /// </summary>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : IEvent;

    /// <summary>
    /// Publishes multiple events
    /// </summary>
    Task PublishManyAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken ct = default) where TEvent : IEvent;
}

/// <summary>
/// Interface for subscribing to domain events
/// </summary>
public interface IEventSubscriber
{
    /// <summary>
    /// Subscribes to an event type
    /// </summary>
    void Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler) where TEvent : IEvent;

    /// <summary>
    /// Unsubscribes from an event type
    /// </summary>
    void Unsubscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler) where TEvent : IEvent;
}

/// <summary>
/// Combined event publisher and subscriber
/// </summary>
public interface IEventBus : IEventPublisher, IEventSubscriber
{
}
