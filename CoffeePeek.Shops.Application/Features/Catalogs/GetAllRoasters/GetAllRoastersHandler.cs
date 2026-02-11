using CoffeePeek.Contract.Dtos.Shop;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using MapsterMapper;

namespace CoffeePeek.Shops.Application.Features.Catalogs.GetAllRoasters;

public class GetAllRoastersHandler
{
    public async Task<Response<GetAllRoastersResponse>> Handle(
        GetAllRoastersCommand request,
        IQueryRoasterRepository repository,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var roasters = await repository.GetAll();

        var roastersDto = mapper.Map<RoasterDto[]>(roasters);

        return Response<GetAllRoastersResponse>.Success(new GetAllRoastersResponse(roastersDto));
    }
}