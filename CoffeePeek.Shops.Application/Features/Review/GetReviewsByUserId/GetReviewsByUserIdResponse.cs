using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Shops.Application.Features.Review.GetReviewsByUserId;

public record GetReviewsByUserIdResponse(
    ReviewDto[] ReviewDtos,
    int TotalItems,
    int TotalPages,
    int CurrentPage,
    int PageSize);