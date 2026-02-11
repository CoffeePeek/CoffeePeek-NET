using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shops.Application.Services;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;

namespace CoffeePeek.Shops.Infrastructure.Consumers;

public static class ModerationShopApprovedHandler
{
    [Transactional]
    public static async Task<ModerationShopApproveCompleteResponse> Handle(
        ModerationShopApprovedEvent @event,
        ICreateShopFromModerationService createShopService,
        ILogger logger,
        CancellationToken ct)
    {
        var shopDto = @event.Shop;

        logger.LogInformation("Processing approval for ShopId: {ShopId} by User: {UserId}", 
            shopDto.Id, @event.UserId);

        var shopId = await createShopService.CreateShopFromApprovedEventAsync(
            shopDto,
            creatorId: @event.UserId,
            moderationId: shopDto.Id,
            ct);
        
        return new ModerationShopApproveCompleteResponse(shopDto.Id, shopId);
    }
}