using CoffeePeek.Contract.Constants;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Events;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Constants;
using DotNetCore.CAP;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Response = CoffeePeek.Contract.Abstract.Response;

namespace CoffeePeek.Moderation.Application.Features.Shop.UpdateModerationShopStatus;

public class UpdateModerationCoffeeShopStatusHandler(
    IModerationShopRepository repository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ICapPublisher capPublisher,
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

            using var trans = unitOfWork.BeginTransactionAsync(ct);
            var shopDto = mapper.Map<ShopDto>(shop);
            
            await capPublisher.PublishAsync(
                name: CapEventNames.Moderation.ShopApproved,
                contentObj: new ModerationShopApprovedEvent(shop.UserId, shopDto),
                callbackName: CapEventNames.Moderation.CallBack.ShopCompleted,
                cancellationToken: ct);

            await unitOfWork.SaveChangesAsync(ct);
            
            await unitOfWork.CommitTransactionAsync(ct);
        }
        else if (command.ModerationStatus == ModerationStatus.Rejected)
        {
            shop.Reject("Rejected by moderator");
            await unitOfWork.SaveChangesAsync(ct);
        }

        return Response.Success();
    }
}