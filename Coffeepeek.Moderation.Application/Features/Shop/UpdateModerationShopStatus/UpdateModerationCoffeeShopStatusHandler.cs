using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Moderation.Application.Features.Shop.UpdateModerationShopStatus;
using CoffeePeek.Moderation.Domain.Repositories;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Response = CoffeePeek.Contract.Abstract.Response;

namespace Coffeepeek.Moderation.Application.Features.Shop.UpdateModerationShopStatus;

public class UpdateModerationCoffeeShopStatusHandler(
    IModerationShopRepository repository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateModerationCoffeeShopStatusHandler> logger) 
    : IRequestHandler<UpdateModerationCoffeeShopStatusCommand, Response>
{
    public async Task<Response> Handle(UpdateModerationCoffeeShopStatusCommand command, CancellationToken ct)
    {
        var shop = await repository.GetByIdAsync(command.Id);

        if (shop == null)
        {
            logger.LogWarning("Shop {ShopId} not found.", command.Id);
            return Response.Error("CoffeeShop not found");
        }

        if (command.ModerationStatus == ModerationStatus.Approved)
        {
            shop.Approve();
            
            // Create event with DTO for Outbox
            var shopDto = mapper.Map<ShopDto>(shop);
            shop.AddDomainEvent(new ModerationShopApprovedEvent(shop.UserId, shopDto));
            
            logger.LogInformation("Shop {ShopId} approved.", command.Id);
        }
        else if (command.ModerationStatus == ModerationStatus.Rejected)
        {
            shop.Reject("Rejected by moderator");
        }

        await unitOfWork.SaveChangesAsync(ct);

        return Response.Success();
    }
}
