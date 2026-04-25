using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Responses;

public sealed class GetReviewsByUserIdResultDto
{
    public ReviewDto[] ReviewDtos { get; set; } = [];

    public int TotalItems { get; set; }

    public int TotalPages { get; set; }

    public int CurrentPage { get; set; }

    public int PageSize { get; set; }
}
