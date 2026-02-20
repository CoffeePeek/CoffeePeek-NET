using CoffeePeek.Contract.Responses;
using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Shared.Kernel;
using MassTransit;

namespace CoffeePeek.Moderation.Infrastructure.Consumers;

public class ModerationShopApproveCompleteConsumer(
    IModerationShopRepository repository,
    IUnitOfWork unitOfWork) : IConsumer<ModerationShopApproveCompleteResponse>
{
    public async Task Consume(ConsumeContext<ModerationShopApproveCompleteResponse> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;

        var moderationShop = await repository.GetByIdWithOutDetails(@event.ModerationShopId, ct);

        if (moderationShop == null)
            return;

        moderationShop.AddShopId(@event.ShopId);

        await unitOfWork.SaveChangesAsync(ct);
    }
}