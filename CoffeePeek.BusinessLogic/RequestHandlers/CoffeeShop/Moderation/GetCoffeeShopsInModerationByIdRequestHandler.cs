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

public class GetCoffeeShopsInModerationByIdRequestHandler(
    IRepository<ModerationShop> moderationShopRepository,
    IMapper mapper)
    : IRequestHandler<GetCoffeeShopsInModerationByIdRequest, Response<GetCoffeeShopsInModerationByIdResponse>>
{
    public async Task<Response<GetCoffeeShopsInModerationByIdResponse>> Handle(GetCoffeeShopsInModerationByIdRequest request, 
        CancellationToken cancellationToken)
    {
        var reviewShops = await moderationShopRepository.FindBy(x => x.UserId == request.UserId)
            .ToArrayAsync(cancellationToken);

        var reviews = mapper.Map<ModerationShopDto[]>(reviewShops);

        var result = new GetCoffeeShopsInModerationByIdResponse(reviews);
        
        return Response.SuccessResponse<Response<GetCoffeeShopsInModerationByIdResponse>>(result);
    }
}