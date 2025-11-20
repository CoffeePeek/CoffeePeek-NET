using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using CoffeePeek.Shared.Models.PhotoUpload;
using MassTransit;
using MediatR;

namespace CoffeePeek.BusinessLogic.RequestHandlers.CoffeeShop.Review.Behavior;

internal class UpdateReviewCoffeeShopBehavior(IPublishEndpoint publishEndpoint)
    : IPipelineBehavior<UpdateModerationCoffeeShopRequest, Contract.Response.Response<UpdateModerationCoffeeShopResponse>>
{
    public async Task<Contract.Response.Response<UpdateModerationCoffeeShopResponse>> Handle(UpdateModerationCoffeeShopRequest request,
        RequestHandlerDelegate<Contract.Response.Response<UpdateModerationCoffeeShopResponse>> next, CancellationToken cancellationToken)
    {
        var response = await next();

        if (request.ShopPhotos is { Count: > 0 } && response.Success)
        {
            await publishEndpoint.Publish<IPhotoUploadRequested>(new
            {
                UserId = request.UserId,
                ShopId = request.ReviewShopId,
                Photos = request.ShopPhotos
            }, cancellationToken);
        }
        
        return response;
    }
}