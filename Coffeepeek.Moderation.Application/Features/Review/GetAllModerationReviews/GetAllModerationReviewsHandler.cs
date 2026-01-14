using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Moderation.Application.Features.Review.GetAllModerationReviews;
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

        var moderationReviewDtos = mapper.Map<ModerationReviewDto[]>(moderationReviews);

        var result = new GetAllModerationReviewsResponse(moderationReviewDtos);

        return Response<GetAllModerationReviewsResponse>.Success(result);
    }
}