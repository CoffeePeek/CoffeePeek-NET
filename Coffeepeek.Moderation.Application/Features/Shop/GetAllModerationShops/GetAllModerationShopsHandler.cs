using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using Coffeepeek.Moderation.Application.Features.Shop.GetAllModerationShops;
using CoffeePeek.Moderation.Domain.Repositories;
using MapsterMapper;
using MediatR;

namespace Coffeepeek.Moderation.Application.Features.GetAllModerationShops;

public class GetAllModerationShopsHandler(IModerationShopRepository repository, IMapper mapper) 
    : IRequestHandler<GetAllModerationShopsCommand, Response<GetCoffeeShopsInModerationByIdResponse>>
{
    public async Task<Response<GetCoffeeShopsInModerationByIdResponse>> Handle(GetAllModerationShopsCommand request, 
        CancellationToken cancellationToken)
    {
        var moderationShops = await repository.GetAllForReviewAsync();
        
        var dtos = mapper.Map<ModerationShopDto[]>(moderationShops);

        return Response.Success(new GetCoffeeShopsInModerationByIdResponse(dtos));
    }
}