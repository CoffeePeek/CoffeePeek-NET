namespace CoffeePeek.Shared.Kernel;

public interface IEventPublisher
{
    Task Publish<T>(T @event, CancellationToken ct = default) where T : class;
}