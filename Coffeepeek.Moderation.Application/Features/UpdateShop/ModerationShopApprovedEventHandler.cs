using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Moderation.Domain.Events;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Coffeepeek.Moderation.Application.Features.UpdateShop;

public class ModerationShopApprovedEventHandler(
    IOutboxEventPublisher outboxEventPublisher,
    IMapper mapper,
    ILogger<ModerationShopApprovedEventHandler> logger)
    : INotificationHandler<ModerationShopApprovedDomainEvent>
{
    public async Task Handle(ModerationShopApprovedDomainEvent notification, CancellationToken ct)
    {
        var shop = notification.Shop;
        
        logger.LogInformation("Transforming domain event for Shop {ShopId} into integration event.", shop.Id);

        var integrationEvent = new CoffeeShopApprovedIntegrationEvent(
            shop.UserId, 
            mapper.Map<ShopDto>(shop)
        );

        await outboxEventPublisher.PublishAsync(integrationEvent, ct);
        
        logger.LogInformation("Integration event for Shop {ShopId} saved to outbox.", shop.Id);
    }
}