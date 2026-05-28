#nullable enable
namespace BinanceP2pMonitor.Events;

/// <summary>
/// Base interface for domain events
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Unique identifier for the event instance
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    DateTime OccurredAt { get; }

    /// <summary>
    /// Event type name
    /// </summary>
    string EventType { get; }
}

/// <summary>
/// Base class for domain events
/// </summary>
public abstract class DomainEvent : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public abstract string EventType { get; }
}
