using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Contract.Response.CoffeeShop;

public class GetAllReviewsResponse(
    ICollection<CoffeeShopReviewDto> reviews,
    int totalItems,
    int totalPages,
    int currentPage,
    int pageSize)
{
    public ICollection<CoffeeShopReviewDto> Reviews { get; } = reviews;
    public int TotalItems { get; } = totalItems;
    public int TotalPages { get; } = totalPages;
    public int CurrentPage { get; } = currentPage;
    public int PageSize { get; } = pageSize;
}