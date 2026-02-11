using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Shared.Kernel.Response;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;

namespace CoffeePeek.Moderation.Application.Features.Shop.UpdateModerationShopStatus;

public static class UpdateModerationCoffeeShopStatusHandler
{
    [Transactional]
    public static async Task<(Response, ModerationShopApprovedEvent?)> Handle(
        UpdateModerationCoffeeShopStatusCommand command,
        IModerationShopRepository repository,
        IMapper mapper,
        ILogger<UpdateModerationCoffeeShopStatusCommand> logger,
        CancellationToken ct)
    {
        var shop = await repository.GetByIdAsync(command.Id, ct);

        if (shop == null)
        {
            logger.LogWarning("Shop {ShopId} not found.", command.Id);
            return (Response.Error("CoffeeShop not found"), null);
        }

        ModerationShopApprovedEvent? approvedEvent = null;

        if (command.ModerationStatus == ModerationStatus.Approved)
        {
            shop.Approve();

            var shopDto = mapper.Map<ShopDto>(shop);
            approvedEvent = new ModerationShopApprovedEvent(shop.UserId, shopDto);
        }
        else if (command.ModerationStatus == ModerationStatus.Rejected)
        {
            shop.Reject("Rejected by moderator");
        }

        return (Response.Success(), approvedEvent);
    }
}
