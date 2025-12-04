using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.ModerationService.Models;
using CoffeePeek.ModerationService.Repositories;
using CoffeePeek.ModerationService.Repositories.Interfaces;
using MediatR;

namespace CoffeePeek.ModerationService.Handlers;

public class SendCoffeeShopToModerationHandler(
    IModerationShopRepository repository,
    IUnitOfWork unitOfWork) 
    : IRequestHandler<SendCoffeeShopToModerationRequest, Response<SendCoffeeShopToModerationResponse>>
{
    public async Task<Response<SendCoffeeShopToModerationResponse>> Handle(SendCoffeeShopToModerationRequest request,
        CancellationToken cancellationToken)
    {
        var existingShop = await repository.GetByNameAndAddressAsync(request.Name, request.NotValidatedAddress, request.UserId);

        if (existingShop != null)
        {
            return Response.ErrorResponse<Response<SendCoffeeShopToModerationResponse>>("A moderation submission with this name and address already exists.");
        }

        var moderationShop = new ModerationShop
        {
            Name = request.Name,
            NotValidatedAddress = request.NotValidatedAddress,
            UserId = request.UserId,
            ModerationStatus = ModerationStatus.Pending,
            Status = ShopStatus.NotConfirmed
        };

        await repository.AddAsync(moderationShop);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Response.SuccessResponse<Response<SendCoffeeShopToModerationResponse>>(null, "CoffeeShop added to moderation.");
    }
}


