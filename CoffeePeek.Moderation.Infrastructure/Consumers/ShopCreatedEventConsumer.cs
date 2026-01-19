using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Moderation.Domain.Repositories;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Moderation.Infrastructure.Consumers;

public class ShopCreatedEventConsumer(
    IModerationShopRepository moderationShopRepository,
    IUnitOfWork unitOfWork,
    ILogger<ShopCreatedEventConsumer> logger)
    : IConsumer<ShopCreatedEvent>
{
    public async Task Consume(ConsumeContext<ShopCreatedEvent> context)
    {
        var @event = context.Message;
        
        logger.LogInformation("Received ShopCreatedEvent for ShopId: {ShopId}, ModerationId: {ModerationId}",
            @event.ShopId, @event.ModerationId);

        var moderationShop = await moderationShopRepository.GetByIdAsync(@event.ModerationId);
        
        if (moderationShop == null)
        {
            logger.LogWarning("ModerationShop with Id {ModerationId} not found", @event.ModerationId);
            return;
        }

        moderationShop.SetShopId(@event.ShopId);
        
        await unitOfWork.SaveChangesAsync(context.CancellationToken);
        
        logger.LogInformation("Updated ModerationShop {ModerationId} with ShopId {ShopId}",
            @event.ModerationId, @event.ShopId);
    }
}
