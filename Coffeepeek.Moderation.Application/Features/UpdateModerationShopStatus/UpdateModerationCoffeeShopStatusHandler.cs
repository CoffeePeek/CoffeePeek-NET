using CoffeePeek.Contract.Enums;
using CoffeePeek.Moderation.Domain.Repositories;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MediatR;
using Microsoft.Extensions.Logging;
using Response = CoffeePeek.Contract.Responses.Response;

namespace Coffeepeek.Moderation.Application.UpdateModerationShopStatus;

public class UpdateModerationCoffeeShopStatusHandler(
    IModerationShopRepository repository,
    IUnitOfWork unitOfWork,
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
