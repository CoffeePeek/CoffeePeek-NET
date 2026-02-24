using CoffeePeek.Contract.Responses;
using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Shared.Kernel;
using MassTransit;

namespace CoffeePeek.Moderation.Infrastructure.Consumers;

public class ModerationShopApproveCompleteConsumer(
    IModerationShopRepository repository,
    IUnitOfWork unitOfWork) 
{
    public async Task Consume(ModerationShopApproveCompleteResponse message)
    {
        var moderationShop = await repository.GetByIdWithOutDetails(message.ModerationShopId);

        if (moderationShop == null)
            return;

        moderationShop.AddShopId(message.ShopId);

        await unitOfWork.SaveChangesAsync();
    }
}