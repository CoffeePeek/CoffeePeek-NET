using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Moderation.Domain.Entities;
using MapsterMapper;
using MediatR;

namespace CoffeePeek.Moderation.Application.Features.Shop.GetAllModerationShops;

public class GetAllModerationShopsHandler(IModerationShopRepository repository, IMapper mapper) 
    : IRequestHandler<GetAllModerationShopsQuery, Response<GetAllModerationShopsResponse>>
{
    public async Task<Response<GetAllModerationShopsResponse>> Handle(GetAllModerationShopsQuery request, 
        CancellationToken cancellationToken)
    {
        var moderationShops = await repository.GetAllForReviewAsync();
        
        var dtos = mapper.Map<ModerationShopDto[]>(moderationShops);

        return Response<GetAllModerationShopsResponse>.Success(new GetAllModerationShopsResponse(dtos));
    }
}