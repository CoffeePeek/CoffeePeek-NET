using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Moderation.Domain.Aggregates.ModerationReviewAggregate;
using CoffeePeek.Shared.Kernel.Response;
using MapsterMapper;

namespace CoffeePeek.Moderation.Application.Features.Review.GetAllModerationReviews;

public class GetAllModerationReviewsHandler
{
    public async Task<Response<GetAllModerationReviewsResponse>> Handle(
        GetAllModerationReviewsQuery query,
        IQueryModerationReviewRepository reviewRepository,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var domainStatus = query.Status.HasValue
            ? (Domain.Aggregates.ModerationReviewAggregate.Enums.ModerationStatus?)query.Status.Value
            : null;

        var (items, totalCount) = await reviewRepository.GetPagedAsync(
            query.Page,
            query.PageSize,
            domainStatus,
            query.Search,
            cancellationToken);

        var moderationReviewDtos = mapper.Map<ModerationReviewDto[]>(items);
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        return Response<GetAllModerationReviewsResponse>.Success(new GetAllModerationReviewsResponse(
            moderationReviewDtos,
            totalCount,
            totalPages,
            query.Page,
            query.PageSize));
    }
}
