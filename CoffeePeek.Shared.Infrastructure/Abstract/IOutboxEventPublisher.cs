namespace CoffeePeek.Shared.Infrastructure.Outbox;

/// <summary>
/// Interface for publishing events to outbox pattern.
/// Ensures events are saved to database in the same transaction as business data.
/// </summary>
public interface IOutboxEventPublisher
{
    /// <summary>
    /// Publishes an event to the outbox table.
    /// The event will be processed asynchronously by the outbox background service.
    /// </summary>
    /// <typeparam name="TEvent">Type of the event</typeparam>
    /// <param name="event">The event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class;
}

