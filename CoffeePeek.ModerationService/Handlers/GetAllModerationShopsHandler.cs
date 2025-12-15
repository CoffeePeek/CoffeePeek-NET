using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.ModerationService.Repositories.Interfaces;
using MapsterMapper;
using MediatR;

namespace CoffeePeek.ModerationService.Handlers;

public class GetAllModerationShopsHandler(IModerationShopRepository repository, IMapper mapper) 
    : IRequestHandler<GetAllModerationShopsRequest, Response<GetCoffeeShopsInModerationByIdResponse>>
{
    public async Task<Response<GetCoffeeShopsInModerationByIdResponse>> Handle(GetAllModerationShopsRequest request, 
        CancellationToken cancellationToken)
    {
        var shops = await repository.GetAllAsync();
        
        var dtos = mapper.Map<ModerationShopDto[]>(shops);
        
        return Response<GetCoffeeShopsInModerationByIdResponse>.Success(new GetCoffeeShopsInModerationByIdResponse(dtos));
    }
}