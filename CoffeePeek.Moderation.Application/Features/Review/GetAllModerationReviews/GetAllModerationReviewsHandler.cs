using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Moderation.Domain.Aggregates.ModerationReviewAggregate;
using CoffeePeek.Shared.Kernel.Response;
using MapsterMapper;

namespace CoffeePeek.Moderation.Application.Features.Review.GetAllModerationReviews;

public class GetAllModerationReviewsHandler
{
    public async Task<Response<GetAllModerationReviewsResponse>> Handle(
        GetAllModerationReviewsQuery _,
        IQueryModerationReviewRepository reviewRepository, 
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var moderationReviews = await reviewRepository.GetAll(cancellationToken);

        var moderationReviewDtos = mapper.Map<ModerationReviewDto[]>(moderationReviews);

        var result = new GetAllModerationReviewsResponse(moderationReviewDtos);

        return Response<GetAllModerationReviewsResponse>.Success(result);
    }
}