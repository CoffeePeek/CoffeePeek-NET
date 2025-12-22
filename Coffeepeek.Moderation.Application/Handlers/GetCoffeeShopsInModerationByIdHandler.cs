using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Moderation.Application.Commands;
using CoffeePeek.Moderation.Domain.Repositories;
using MapsterMapper;
using MediatR;

namespace CoffeePeek.Moderation.Application.Handlers;

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