using CoffeePeek.Shared.Kernel;
using MassTransit;

namespace CoffeePeek.Shared.Persistence;

public class MassTransitEventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public Task Publish<T>(T @event, CancellationToken ct = default) where T : class
        => publishEndpoint.Publish(@event, ct);
}