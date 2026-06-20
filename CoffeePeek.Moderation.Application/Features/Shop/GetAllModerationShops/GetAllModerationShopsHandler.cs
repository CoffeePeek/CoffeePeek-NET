using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Shared.Kernel.Response;
using MapsterMapper;

namespace CoffeePeek.Moderation.Application.Features.Shop.GetAllModerationShops;

public class GetAllModerationShopsHandler
{
    public async Task<Response<GetAllModerationShopsResponse>> Handle(
        GetAllModerationShopsQuery query,
        IQueryModerationShopRepository repository,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var domainStatus = query.Status.HasValue
            ? (Domain.Aggregates.ModerationReviewAggregate.Enums.ModerationStatus?)query.Status.Value
            : null;

        var (items, totalCount) = await repository.GetPagedForReviewAsync(
            query.Page,
            query.PageSize,
            domainStatus,
            query.Search,
            cancellationToken);

        var dtos = mapper.Map<ModerationShopDto[]>(items);
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        return Response<GetAllModerationShopsResponse>.Success(new GetAllModerationShopsResponse(
            dtos,
            totalCount,
            totalPages,
            query.Page,
            query.PageSize));
    }
}
