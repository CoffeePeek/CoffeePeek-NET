using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Shops.Application.Features.Review.GetAllReviewsByShopId;

public class GetAllReviewsResponse(
    ICollection<ReviewDto> reviews,
    int totalItems,
    int totalPages,
    int currentPage,
    int pageSize)
{
    public ICollection<ReviewDto> Reviews { get; } = reviews;
    public int TotalItems { get; } = totalItems;
    public int TotalPages { get; } = totalPages;
    public int CurrentPage { get; } = currentPage;
    public int PageSize { get; } = pageSize;
}