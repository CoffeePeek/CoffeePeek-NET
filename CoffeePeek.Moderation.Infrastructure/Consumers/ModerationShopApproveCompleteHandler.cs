using CoffeePeek.Contract.Responses;
using CoffeePeek.Moderation.Domain.Aggregates;
using Wolverine.Attributes;

namespace CoffeePeek.Moderation.Infrastructure.Consumers;

public static class ModerationShopApproveCompleteHandler
{
    [Transactional]
    public static async Task Handle(
        ModerationShopApproveCompleteResponse @event, 
        IModerationShopRepository repository, 
        CancellationToken ct)
    {
        var moderationShop = await repository.GetByIdWithOutDetails(@event.ModerationShopId, ct);

        moderationShop?.AddShopId(@event.ShopId);
    }
}