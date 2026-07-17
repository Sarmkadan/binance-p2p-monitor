using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BinanceP2pMonitor.Events
{
    /// <summary>
    /// Extension methods for <see cref="EventBus"/> that provide simplified APIs for common scenarios.
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
        /// <param name="events">The events to publish. Must not be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="bus"/> or <paramref name="events"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="events"/> is empty.
        /// </exception>
        public static Task PublishManyAsync<TEvent>(this EventBus bus, IEnumerable<TEvent> events)
            where TEvent : IEvent
        {
            ArgumentNullException.ThrowIfNull(bus);
            ArgumentNullException.ThrowIfNull(events);

            var eventList = events.ToList();
            if (eventList.Count == 0)
            {
                throw new ArgumentException("Events collection cannot be empty.", nameof(events));
            }

            return bus.PublishManyAsync(eventList, CancellationToken.None);
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

            bus.Subscribe<TEvent>((e, _) =>
            {
                handler(e);
                return Task.CompletedTask;
            });
        }
    }
}
