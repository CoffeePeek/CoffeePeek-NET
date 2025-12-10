using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.ModerationService.Handlers;
using CoffeePeek.ModerationService.Repositories;
using CoffeePeek.ModerationService.Repositories.Interfaces;
using MediatR;

namespace CoffeePeek.ModerationService.Handlers;

public class GetCoffeeShopsInModerationByIdHandler(IModerationShopRepository repository) 
    : IRequestHandler<GetCoffeeShopsInModerationByIdRequest, Response<GetCoffeeShopsInModerationByIdResponse>>
{
    public async Task<Response<GetCoffeeShopsInModerationByIdResponse>> Handle(GetCoffeeShopsInModerationByIdRequest request, 
        CancellationToken cancellationToken)
    {
        var shops = await repository.GetByUserIdAsync(request.UserId);
        var dtos = shops.Select(ModerationShopMapper.MapToDto).ToArray();
        var result = new GetCoffeeShopsInModerationByIdResponse(dtos);
        
        return Response<GetCoffeeShopsInModerationByIdResponse>.Success(result);
    }
}