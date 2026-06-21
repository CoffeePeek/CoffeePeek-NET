using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shops.Application.Services;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Shops.Infrastructure.Consumers;

public static class ModerationShopApproveHandler
{
    public static async Task<ModerationShopApproveCompleteResponse> Handle(
        ModerationShopApprovedEvent message,
        ICreateShopFromModerationService createShopService,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        var logger = loggerFactory.CreateLogger(nameof(ModerationShopApproveHandler));

        logger.LogInformation(
            "Received ModerationShopApprovedEvent for moderation shop {ModerationShopId} ({ShopName})",
            message.Shop.Id,
            message.Shop.Name);

        var shopId = await createShopService.CreateShopFromApprovedEventAsync(
            message.Shop,
            creatorId: message.UserId,
            moderationId: message.Shop.Id,
            ct);

        logger.LogInformation(
            "Created published shop {ShopId} from moderation shop {ModerationShopId}",
            shopId,
            message.Shop.Id);

        return new ModerationShopApproveCompleteResponse(message.Shop.Id, shopId);
    }
}
