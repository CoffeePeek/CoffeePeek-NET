namespace OutboxBackgroundService;

/// <summary>
/// Interface for outbox processors.
/// </summary>
public interface IOutboxProcessor
{
    Task ProcessOutboxEventsAsync(CancellationToken cancellationToken);
}