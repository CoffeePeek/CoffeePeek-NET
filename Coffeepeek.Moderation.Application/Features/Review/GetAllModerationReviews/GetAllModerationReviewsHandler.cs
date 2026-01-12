using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Moderation.Domain.Entities.ModerationReviewAggregate;
using Mapster;
using MapsterMapper;
using MediatR;

namespace Coffeepeek.Moderation.Application.Features.Review.GetAllModerationReviews;

public class GetAllModerationReviewsHandler(IModerationReviewRepository repository, IMapper mapper)
    : IRequestHandler<GetAllModerationReviewsQuery, Response<GetAllModerationReviewsResponse>>
{
    public async Task<Response<GetAllModerationReviewsResponse>> Handle(GetAllModerationReviewsQuery request,
        CancellationToken cancellationToken)
    {
        var moderationReviews = await repository.GetAll(cancellationToken);

        var moderationReviewDtos = moderationReviews.Adapt<ModerationReviewDto[]>(mapper.Config);

        var result = new GetAllModerationReviewsResponse(moderationReviewDtos);

        return Response<GetAllModerationReviewsResponse>.Success(result);
    }
}