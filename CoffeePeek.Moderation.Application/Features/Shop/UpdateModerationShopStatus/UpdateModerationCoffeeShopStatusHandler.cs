using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Response;
using MapsterMapper;

namespace CoffeePeek.Moderation.Application.Features.Shop.UpdateModerationShopStatus;

public static class UpdateModerationCoffeeShopStatusHandler
{
    public static async Task<(Response, object?)> Handle(
        UpdateModerationCoffeeShopStatusCommand command,
        IModerationShopRepository repository,
        IMapper mapper,
        CancellationToken ct)
    {
        var shop = await repository.GetByIdAsync(command.Id, ct);

        if (shop == null)
            return (Response.Error("CoffeeShop not found"), null);

        object? outboundEvent = null;

        if (command.ModerationStatus == ModerationStatus.Approved)
        {
            shop.Approve();
            
            outboundEvent = new ModerationShopApprovedEvent(
                shop.UserId,
                mapper.Map<ShopDto>(shop));
        }
        else if (command.ModerationStatus == ModerationStatus.Rejected)
        {
            var rejectReason = string.IsNullOrWhiteSpace(command.Comment)
                ? "Rejected by moderator"
                : command.Comment.Trim();
            shop.Reject(rejectReason);
        }

        return (Response.Success(), outboundEvent);
    }
}