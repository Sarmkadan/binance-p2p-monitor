using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BinanceP2pMonitor.Events
{
    /// <summary>
    /// Extension methods for <see cref="EventBus"/>.
    /// </summary>
    public static class EventBusExtensions
    {
        /// <summary>
        /// Publishes a single event without requiring a <see cref="CancellationToken"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="bus">The event bus.</param>
        /// <param name="event">The event to publish.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="bus"/> or <paramref name="event"/> is <c>null</c>.
        /// </exception>
        public static Task PublishAsync<TEvent>(this EventBus bus, TEvent @event)
            where TEvent : IEvent
        {
            ArgumentNullException.ThrowIfNull(bus);
            ArgumentNullException.ThrowIfNull(@event);
            return bus.PublishAsync(@event, CancellationToken.None);
        }

        /// <summary>
        /// Publishes multiple events without requiring a <see cref="CancellationToken"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the events.</typeparam>
        /// <param name="bus">The event bus.</param>
        /// <param name="events">The events to publish.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="bus"/> or <paramref name="events"/> is <c>null</c>.
        /// </exception>
        public static Task PublishManyAsync<TEvent>(this EventBus bus, IEnumerable<TEvent> events)
            where TEvent : IEvent
        {
            ArgumentNullException.ThrowIfNull(bus);
            ArgumentNullException.ThrowIfNull(events);
            return bus.PublishManyAsync(events, CancellationToken.None);
        }

        /// <summary>
        /// Subscribes to an event using a simple <see cref="Action{T}"/> handler.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="bus">The event bus.</param>
        /// <param name="handler">The handler to invoke when the event is published.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="bus"/> or <paramref name="handler"/> is <c>null</c>.
        /// </exception>
        public static void Subscribe<TEvent>(this EventBus bus, Action<TEvent> handler)
            where TEvent : IEvent
        {
            ArgumentNullException.ThrowIfNull(bus);
            ArgumentNullException.ThrowIfNull(handler);
            bus.Subscribe<TEvent>((e, ct) =>
            {
                handler(e);
                return Task.CompletedTask;
            });
        }
    }
}
