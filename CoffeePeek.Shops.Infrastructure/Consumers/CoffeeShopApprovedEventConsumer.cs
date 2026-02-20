using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shops.Application.Services;
using MassTransit;

namespace CoffeePeek.Shops.Infrastructure.Consumers;

public class ModerationShopApprovedConsumer(
    ICreateShopFromModerationService createShopService,
    IEventPublisher eventPublisher,
    IUnitOfWork unitOfWork) : IConsumer<ModerationShopApprovedEvent>
{
    public async Task Consume(ConsumeContext<ModerationShopApprovedEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;

        var shopId = await createShopService.CreateShopFromApprovedEventAsync(
            @event.Shop,
            creatorId: @event.UserId,
            moderationId: @event.Shop.Id,
            ct);

        await eventPublisher.Publish(new ModerationShopApproveCompleteResponse(@event.Shop.Id, shopId), ct);

        await unitOfWork.SaveChangesAsync(ct);
    }
}