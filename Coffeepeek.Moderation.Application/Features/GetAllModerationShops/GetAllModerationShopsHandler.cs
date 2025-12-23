using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using Coffeepeek.Moderation.Application.Features.GetAllModerationShops;
using CoffeePeek.Moderation.Domain.Repositories;
using MapsterMapper;
using MediatR;

namespace Coffeepeek.Moderation.Application.GetAllModerationShops;

public class GetAllModerationShopsHandler(IModerationShopRepository repository, IMapper mapper) 
    : IRequestHandler<GetAllModerationShopsCommand, Response<GetCoffeeShopsInModerationByIdResponse>>
{
    public async Task<Response<GetCoffeeShopsInModerationByIdResponse>> Handle(GetAllModerationShopsCommand request, 
        CancellationToken cancellationToken)
    {
        var shops = await repository.GetAllAsync();
        
        var dtos = mapper.Map<ModerationShopDto[]>(shops);
        
        return Response<GetCoffeeShopsInModerationByIdResponse>.Success(new GetCoffeeShopsInModerationByIdResponse(dtos));
    }
}