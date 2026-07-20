#nullable enable
using System;

namespace BinanceP2pMonitor.Events;

/// <summary>
/// Event emitted periodically to indicate the service is alive.
/// Contains the uptime of the host and the timestamp of the last successful data fetch.
/// </summary>
public sealed class HeartbeatEvent : IEvent
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    public Guid EventId { get; } = Guid.NewGuid();

    /// <summary>
    /// Human‑readable name of the event type.
    /// </summary>
    public string EventType => nameof(HeartbeatEvent);

    /// <summary>
    /// When the event was created/occurred.
    /// </summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// How long the hosting process has been running.
    /// </summary>
    public TimeSpan Uptime { get; }

    /// <summary>
    /// Timestamp of the most recent successful fetch operation.
    /// </summary>
    public DateTime LastSuccessfulFetch { get; }

    public HeartbeatEvent(TimeSpan uptime, DateTime lastSuccessfulFetch)
    {
        Uptime = uptime;
        LastSuccessfulFetch = lastSuccessfulFetch;
        OccurredAt = DateTime.UtcNow;
    }
}
