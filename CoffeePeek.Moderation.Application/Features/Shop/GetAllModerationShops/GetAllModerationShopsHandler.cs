using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Shared.Kernel.Response;
using MapsterMapper;

namespace CoffeePeek.Moderation.Application.Features.Shop.GetAllModerationShops;

public class GetAllModerationShopsHandler
{
    public async Task<Response<GetAllModerationShopsResponse>> Handle(
        GetAllModerationShopsQuery _, 
        IQueryModerationShopRepository repository, 
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var moderationShops = await repository.GetAllForReviewAsync(cancellationToken);
        
        var dtos = mapper.Map<ModerationShopDto[]>(moderationShops);

        return Response<GetAllModerationShopsResponse>.Success(new GetAllModerationShopsResponse(dtos));
    }
}