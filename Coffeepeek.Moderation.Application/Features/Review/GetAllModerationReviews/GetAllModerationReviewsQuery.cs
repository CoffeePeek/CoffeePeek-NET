using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using Coffeepeek.Moderation.Application.Features.Review.GetAllModerationReviews;
using MediatR;

namespace CoffeePeek.Moderation.Application.Features.Review.GetAllModerationReviews;

public record GetAllModerationReviewsQuery : IRequest<Response<GetAllModerationReviewsResponse>>;