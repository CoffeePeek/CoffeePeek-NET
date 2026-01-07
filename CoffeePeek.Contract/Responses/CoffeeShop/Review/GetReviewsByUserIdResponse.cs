using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Contract.Response.CoffeeShop.Review;

public class GetReviewsByUserIdResponse(
    ReviewDto[] reviewDtos,
    int totalItems,
    int totalPages,
    int currentPage,
    int pageSize)
{
    public ReviewDto[] Reviews { get; init; } = reviewDtos;
    public int TotalItems { get; init; } = totalItems;
    public int TotalPages { get; init; } = totalPages;
    public int CurrentPage { get; init; } = currentPage;
    public int PageSize { get; init; } = pageSize;
}