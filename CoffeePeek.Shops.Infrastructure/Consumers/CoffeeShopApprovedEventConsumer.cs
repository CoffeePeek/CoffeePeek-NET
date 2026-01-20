using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shops.Application.Services;
using CoffeePeek.Shared.Infrastructure.Constants;
using DotNetCore.CAP;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Shops.Infrastructure.Consumers;

public class ModerationShopApprovedHandler(
    ICreateShopFromModerationService createShopService,
    ILogger<ModerationShopApprovedHandler> logger) : ICapSubscribe
{
    [CapSubscribe(CapEventNames.Moderation.ShopApproved)]
    public async Task<ModerationShopApproveCompleteResponse> Handle(ModerationShopApprovedEvent @event, CancellationToken cancellationToken)
    {
        var shopDto = @event.Shop;

        logger.LogInformation("Received {EventName} for UserId: {UserId}, ShopId: {ShopId}",
            nameof(ModerationShopApprovedEvent), @event.UserId, shopDto.Id);

        var shopId = await createShopService.CreateShopFromApprovedEventAsync(
            shopDto,
            creatorId:@event.UserId,
            moderationId: shopDto.Id,
            cancellationToken);
        
        return new ModerationShopApproveCompleteResponse(shopDto.Id, shopId);
    }
}