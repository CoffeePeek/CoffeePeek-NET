using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shops.Application.Services;

namespace CoffeePeek.Shops.Infrastructure.Consumers;

public class ModerationShopApproveHandler(ICreateShopFromModerationService createShopService)
{
    public async Task<ModerationShopApproveCompleteResponse> Handle(ModerationShopApprovedEvent message)
    {
        var shopId = await createShopService.CreateShopFromApprovedEventAsync(
            message.Shop,
            creatorId: message.UserId,
            moderationId: message.Shop.Id);

        return new ModerationShopApproveCompleteResponse(message.Shop.Id, shopId);
    }
}