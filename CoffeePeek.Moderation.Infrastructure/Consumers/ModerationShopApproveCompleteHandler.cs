using CoffeePeek.Contract.Responses;
using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Shared.Kernel;

namespace CoffeePeek.Moderation.Infrastructure.Consumers;

public static class ModerationShopApproveCompleteHandler
{
    public static async Task Handle(
        ModerationShopApproveCompleteResponse message,
        IModerationShopRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var moderationShop = await repository.GetByIdWithOutDetails(message.ModerationShopId);

        if (moderationShop == null)
            return;

        moderationShop.AddShopId(message.ShopId);

        await unitOfWork.SaveChangesAsync(ct);
    }
}