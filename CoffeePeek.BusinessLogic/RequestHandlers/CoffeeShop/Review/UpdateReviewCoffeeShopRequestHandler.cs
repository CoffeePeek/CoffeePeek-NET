using CoffeePeek.BusinessLogic.Models;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using CoffeePeek.Data;
using CoffeePeek.Domain.Entities.Shop;
using CoffeePeek.Domain.UnitOfWork;
using MapsterMapper;
using MediatR;

namespace CoffeePeek.BusinessLogic.RequestHandlers.CoffeeShop.Review;

internal class UpdateReviewCoffeeShopRequestHandler(IRepository<ReviewShop> reviewShopRepository,
    IMapper mapper) 
    : IRequestHandler<UpdateReviewCoffeeShopRequest, Response<UpdateReviewCoffeeShopResponse>>
{
    public async Task<Response<UpdateReviewCoffeeShopResponse>> Handle(UpdateReviewCoffeeShopRequest request,
        CancellationToken cancellationToken)
    {
        var shop = await reviewShopRepository
            .FirstOrDefaultAsync(x => x.Id == request.ReviewShopId && x.UserId == request.UserId, cancellationToken);

        if (shop == null)
        {
            return Response.ErrorResponse<Response<UpdateReviewCoffeeShopResponse>>(null, "Review CoffeeShop not found");
        }

        var model = new ReviewShopModel(shop);
        
        var requestEntity = mapper.Map<ReviewShop>(request);
        
        model.Update(requestEntity);
        
        await reviewShopRepository.SaveChangesAsync(cancellationToken);

        

        return Response.SuccessResponse<Response<UpdateReviewCoffeeShopResponse>>();
    }
}