using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Contract.Response.CoffeeShop;

public class GetUserCheckInsResponse(
    CheckInDto[] checkIns,
    int totalItems,
    int totalPages,
    int currentPage,
    int pageSize)
{
    public CheckInDto[] CheckIns { get; init; } = checkIns;
    public int TotalItems { get; init; } = totalItems;
    public int TotalPages { get; init; } = totalPages;
    public int CurrentPage { get; init; } = currentPage;
    public int PageSize { get; init; } = pageSize;
}