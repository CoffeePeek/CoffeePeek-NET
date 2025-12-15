using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.ModerationService.Repositories.Interfaces;
using MapsterMapper;
using MediatR;

namespace CoffeePeek.ModerationService.Handlers;

public class GetCoffeeShopsInModerationByIdHandler(IModerationShopRepository repository, IMapper mapper)
    : IRequestHandler<GetCoffeeShopsInModerationByIdRequest, Response<GetCoffeeShopsInModerationByIdResponse>>
{
    public async Task<Response<GetCoffeeShopsInModerationByIdResponse>> Handle(
        GetCoffeeShopsInModerationByIdRequest request,
        CancellationToken cancellationToken)
    {
        var shops = await repository.GetByUserIdAsync(request.UserId);
        var dtos = mapper.Map<ModerationShopDto[]>(shops);

        var result = new GetCoffeeShopsInModerationByIdResponse(dtos);

        return Response<GetCoffeeShopsInModerationByIdResponse>.Success(result);
    }
}