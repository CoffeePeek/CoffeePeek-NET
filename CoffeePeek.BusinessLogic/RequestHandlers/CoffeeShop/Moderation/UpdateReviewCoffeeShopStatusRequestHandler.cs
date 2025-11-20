using CoffeePeek.BusinessLogic.Models;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Response;
using CoffeePeek.Domain.Entities.Shop;
using CoffeePeek.Domain.UnitOfWork;
using MediatR;

namespace CoffeePeek.BusinessLogic.RequestHandlers.CoffeeShop.Moderation;

internal class UpdateReviewCoffeeShopStatusRequestHandler(IRepository<ModerationShop> reviewRepository)
    : IRequestHandler<UpdateModerationCoffeeShopStatusRequest, Response>
{
    public async Task<Response> Handle(UpdateModerationCoffeeShopStatusRequest request, CancellationToken cancellationToken)
    {
        var reviewShop = await reviewRepository.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (reviewShop == null)
        {
            return Response.ErrorResponse<Response>("CoffeeShop not found");
        }
        
        var model = new ReviewShopModel(reviewShop);
        
        model.UpdateStatus(request.ModerationStatus);

        await reviewRepository.SaveChangesAsync(cancellationToken);
        
        return Response.SuccessResponse<Response>();
    }
}