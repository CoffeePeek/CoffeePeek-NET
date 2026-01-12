using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace Coffeepeek.Moderation.Application.Features.Review.GetAllModerationReviews;

public record GetAllModerationReviewsResponse(ModerationReviewDto[] ReviewDtos);