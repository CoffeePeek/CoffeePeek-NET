using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Moderation.Domain.Repositories;
using MapsterMapper;
using MediatR;

namespace Coffeepeek.Moderation.Application.Features.Shop.GetAllModerationShops;

public class GetAllModerationShopsHandler(IModerationShopRepository repository, IMapper mapper) 
    : IRequestHandler<GetAllModerationShopsQuery, Response<GetCoffeeShopsInModerationByIdResponse>>
{
    public async Task<Response<GetCoffeeShopsInModerationByIdResponse>> Handle(GetAllModerationShopsQuery request, 
        CancellationToken cancellationToken)
    {
        var moderationShops = await repository.GetAllForReviewAsync();
        
        var dtos = mapper.Map<ModerationShopDto[]>(moderationShops);

        return Response.Success(new GetCoffeeShopsInModerationByIdResponse(dtos));
    }
}