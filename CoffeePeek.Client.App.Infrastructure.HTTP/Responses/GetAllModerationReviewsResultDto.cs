using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Responses;

public sealed class GetAllModerationReviewsResultDto
{
    public ModerationReviewDto[] ReviewDtos { get; init; } = [];
}
