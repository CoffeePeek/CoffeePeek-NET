using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.ModerationService.Repositories;
using CoffeePeek.ModerationService.Repositories.Interfaces;
using MediatR;

namespace CoffeePeek.ModerationService.Handlers;

public class UpdateModerationCoffeeShopHandler(
    IModerationShopRepository repository,
    IUnitOfWork unitOfWork) 
    : IRequestHandler<UpdateModerationCoffeeShopRequest, Response<UpdateModerationCoffeeShopResponse>>
{
    public async Task<Response<UpdateModerationCoffeeShopResponse>> Handle(UpdateModerationCoffeeShopRequest request,
        CancellationToken cancellationToken)
    {
        var shop = await repository.GetByIdAsync(request.ReviewShopId);

        if (shop == null || shop.UserId != request.UserId)
        {
            return Response<UpdateModerationCoffeeShopResponse>.Error("Moderation CoffeeShop not found");
        }

        // TODO: Update shop properties from request
        // This is a simplified version - full implementation would map Address, ShopContacts, Photos, Schedules
        
        await repository.UpdateAsync(shop);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Response<UpdateModerationCoffeeShopResponse>.Success(null);
    }
}