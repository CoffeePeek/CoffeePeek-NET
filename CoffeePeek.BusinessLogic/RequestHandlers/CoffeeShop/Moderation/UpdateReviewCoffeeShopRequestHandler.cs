using CoffeePeek.BusinessLogic.Models;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using CoffeePeek.Domain.Entities.Shop;
using CoffeePeek.Domain.UnitOfWork;
using MapsterMapper;
using MediatR;

namespace CoffeePeek.BusinessLogic.RequestHandlers.CoffeeShop.Moderation;

internal class UpdateReviewCoffeeShopRequestHandler(IRepository<ModerationShop> reviewShopRepository,
    IMapper mapper) 
    : IRequestHandler<UpdateModerationCoffeeShopRequest, Response<UpdateModerationCoffeeShopResponse>>
{
    public async Task<Response<UpdateModerationCoffeeShopResponse>> Handle(UpdateModerationCoffeeShopRequest request,
        CancellationToken cancellationToken)
    {
        var shop = await reviewShopRepository
            .FirstOrDefaultAsync(x => x.Id == request.ReviewShopId && x.UserId == request.UserId, cancellationToken);

        if (shop == null)
        {
            return Response.ErrorResponse<Response<UpdateModerationCoffeeShopResponse>>(null, "Moderation CoffeeShop not found");
        }

        var model = new ReviewShopModel(shop);
        
        var requestEntity = mapper.Map<ModerationShop>(request);
        
        model.Update(requestEntity);
        
        await reviewShopRepository.SaveChangesAsync(cancellationToken);

        

        return Response.SuccessResponse<Response<UpdateModerationCoffeeShopResponse>>();
    }
}