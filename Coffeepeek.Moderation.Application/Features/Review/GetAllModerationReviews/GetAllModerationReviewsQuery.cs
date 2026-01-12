using CoffeePeek.Contract.Responses;
using MediatR;

namespace Coffeepeek.Moderation.Application.Features.Review.GetAllModerationReviews;

public record GetAllModerationReviewsQuery : IRequest<Response<GetAllModerationReviewsResponse>>
{
    
}