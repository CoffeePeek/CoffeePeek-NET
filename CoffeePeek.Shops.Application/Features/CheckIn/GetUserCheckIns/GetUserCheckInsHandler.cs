using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using MapsterMapper;

namespace CoffeePeek.Shops.Application.Features.CheckIn.GetUserCheckIns;

public class GetUserCheckInsHandler(
    ICheckInQueries checkInQueries,
    IQueryCoffeeShopRepository queryCoffeeShopRepository,
    IMapper mapper)
{
    public async Task<Response<GetUserCheckInsResponse>> Handle(GetUserCheckInsCommand request,
        CancellationToken cancellationToken)
    {
        var checkinDtos =
            await checkInQueries.GetByUserId(request.UserId, request.PageNumber, request.PageSize, cancellationToken);

        var shopIds = checkinDtos.Select(c => c.ShopId).Distinct().ToList();
        var shopNames = await queryCoffeeShopRepository.GetShopNamesByIdsAsync(shopIds, cancellationToken);

        var checkInDtos = checkinDtos.Select(c =>
        {
            c.ShopName = shopNames.GetValueOrDefault(c.ShopId, string.Empty);
            return c;
        }).ToArray();

        var response = new GetUserCheckInsResponse(
            checkInDtos,
            TotalItems: checkinDtos.Length,
            TotalPages: (int)Math.Ceiling(checkinDtos.Length / (double)request.PageSize),
            request.PageNumber,
            request.PageSize);

        return Response<GetUserCheckInsResponse>.Success(response);
    }
}