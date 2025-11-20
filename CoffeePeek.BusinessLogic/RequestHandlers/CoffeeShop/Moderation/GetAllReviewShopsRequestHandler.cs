using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Domain.Entities.Shop;
using CoffeePeek.Domain.UnitOfWork;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.BusinessLogic.RequestHandlers.CoffeeShop.Moderation;

public class GetAllReviewShopsRequestHandler(
    IRepository<ModerationShop> moderationShopRepository,
    IMapper mapper)
    : IRequestHandler<GetAllModerationShopsRequest, Response<GetCoffeeShopsInModerationByIdResponse>>
{
    public async Task<Response<GetCoffeeShopsInModerationByIdResponse>> Handle(GetAllModerationShopsRequest request, 
        CancellationToken cancellationToken)
    {
        var reviewShops = await moderationShopRepository.GetAll()
            .ToArrayAsync(cancellationToken);

        var reviews = mapper.Map<ModerationShopDto[]>(reviewShops);

        var result = new GetCoffeeShopsInModerationByIdResponse(reviews);
        
        return Response.SuccessResponse<Response<GetCoffeeShopsInModerationByIdResponse>>(result);
    }
}

