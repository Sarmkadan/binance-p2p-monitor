#nullable enable
namespace BinanceP2pMonitor.Events;

/// <summary>
/// In-memory event bus implementation with pub-sub pattern
/// </summary>
public class EventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _subscribers = new();
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly ILogger<EventBus> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventBus"/> class
    /// </summary>
    /// <param name="logger">Logger used for subscription, publishing and handler error diagnostics</param>
    public EventBus(ILogger<EventBus> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Publishes an event to all handlers currently subscribed to <typeparamref name="TEvent"/>
    /// and waits for all of them to complete
    /// </summary>
    /// <typeparam name="TEvent">Type of the event being published</typeparam>
    /// <param name="event">Event to publish</param>
    /// <param name="ct">Cancellation token passed to every handler</param>
    /// <returns>A task that completes when all handlers have completed</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null</exception>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : IEvent
    {
        if (@event is null)
            throw new ArgumentNullException(nameof(@event), $"Event of type {typeof(TEvent).Name} cannot be null");

        var eventType = typeof(TEvent);
        List<Delegate> handlersSnapshot;

        _lock.EnterReadLock();
        try
        {
            if (!_subscribers.TryGetValue(eventType, out var handlers))
            {
                _logger.LogDebug("No subscribers for event type: {EventType}", eventType.Name);
                return;
            }

            // Fix: Copy handlers to avoid holding the read lock during async execution
            handlersSnapshot = handlers.ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }

        _logger.LogInformation("Publishing event: {EventType} ({EventId})", @event.EventType, @event.EventId);

        var tasks = new List<Task>();
        foreach (var handler in handlersSnapshot.Cast<Func<TEvent, CancellationToken, Task>>())
        {
            try
            {
                tasks.Add(handler(@event, ct));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing event handler for {EventType}", eventType.Name);
            }
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    /// <summary>
    /// Publishes multiple events concurrently, each through <see cref="PublishAsync{TEvent}"/>
    /// </summary>
    /// <typeparam name="TEvent">Type of the events being published</typeparam>
    /// <param name="events">Events to publish</param>
    /// <param name="ct">Cancellation token passed to every handler</param>
    /// <returns>A task that completes when all events have been published</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="events"/> is null</exception>
    public async Task PublishManyAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken ct = default) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(events);

        var eventList = events.ToList();
        _logger.LogInformation("Publishing {Count} events", eventList.Count);

        var tasks = eventList.Select(@event => PublishAsync(@event, ct));
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    /// <summary>
    /// Registers a handler for events of type <typeparamref name="TEvent"/>
    /// </summary>
    /// <typeparam name="TEvent">Type of the event to handle</typeparam>
    /// <param name="handler">Handler invoked for each published event of this type</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null</exception>
    public void Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

        var eventType = typeof(TEvent);
        _lock.EnterWriteLock();
        try
        {
            if (!_subscribers.ContainsKey(eventType))
                _subscribers[eventType] = new List<Delegate>();

            _subscribers[eventType].Add(handler);
            _logger.LogDebug("Subscribed to event type: {EventType}", eventType.Name);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Removes a previously registered handler for events of type <typeparamref name="TEvent"/>
    /// </summary>
    /// <typeparam name="TEvent">Type of the event the handler was registered for</typeparam>
    /// <param name="handler">Handler instance to remove; no effect if it was not subscribed</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null</exception>
    public void Unsubscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

        var eventType = typeof(TEvent);
        _lock.EnterWriteLock();
        try
        {
            if (_subscribers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);
                if (!handlers.Any())
                    _subscribers.Remove(eventType);

                _logger.LogDebug("Unsubscribed from event type: {EventType}", eventType.Name);
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}
