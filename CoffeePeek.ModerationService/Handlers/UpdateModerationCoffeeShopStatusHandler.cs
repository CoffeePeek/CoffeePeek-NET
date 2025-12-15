using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Contract.Enums;
using ShopScheduleIntervalDto = CoffeePeek.Contract.Dtos.Schedule.ShopScheduleIntervalDto;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.ModerationService.Repositories.Interfaces;
using CoffeePeek.Shared.Extensions.Exceptions;
using MapsterMapper;
using MassTransit;
using MediatR;
using Response = CoffeePeek.Contract.Responses.Response;

namespace CoffeePeek.ModerationService.Handlers;

public class UpdateModerationCoffeeShopStatusHandler(
    IModerationShopRepository repository,
    IPublishEndpoint publishEndpoint,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateModerationCoffeeShopStatusHandler> logger) 
    : IRequestHandler<UpdateModerationCoffeeShopStatusRequest, Response>
{
    public async Task<Response> Handle(UpdateModerationCoffeeShopStatusRequest request, CancellationToken cancellationToken)
    {
        var shop = await repository.GetByIdAsync(request.Id);

        if (shop == null)
        {
            logger.LogWarning("CoffeeShop with ID {CoffeeShopId} not found for moderation status update.", request.Id);
            throw new NotFoundException("CoffeeShop not found");
        }
        
        shop.ModerationStatus = request.ModerationStatus;
        logger.LogInformation("Updating moderation status for CoffeeShop {CoffeeShopId} to {NewStatus}.", request.Id, request.ModerationStatus);

        if (request.ModerationStatus == ModerationStatus.Approved)
        {
            var approvedEvent = new CoffeeShopApprovedEvent(request.UserId, mapper.Map<ShopDto>(shop));

            await publishEndpoint.Publish(approvedEvent, cancellationToken);
            logger.LogInformation("Published CoffeeShopApprovedEvent for CoffeeShop {CoffeeShopId}.", request.Id);
        }

        await repository.UpdateAsync(shop);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Response.Success();
    }
}
