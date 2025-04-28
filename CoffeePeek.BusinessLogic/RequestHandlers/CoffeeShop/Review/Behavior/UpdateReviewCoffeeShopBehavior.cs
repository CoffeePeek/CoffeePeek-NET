using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using CoffeePeek.Shared.Models.PhotoUpload;
using MassTransit;
using MediatR;

namespace CoffeePeek.BusinessLogic.RequestHandlers.CoffeeShop.Review.Behavior;

internal class UpdateReviewCoffeeShopBehavior(IPublishEndpoint publishEndpoint)
    : IPipelineBehavior<UpdateReviewCoffeeShopRequest, Contract.Response.Response<UpdateReviewCoffeeShopResponse>>
{
    public async Task<Contract.Response.Response<UpdateReviewCoffeeShopResponse>> Handle(UpdateReviewCoffeeShopRequest request,
        RequestHandlerDelegate<Contract.Response.Response<UpdateReviewCoffeeShopResponse>> next, CancellationToken cancellationToken)
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