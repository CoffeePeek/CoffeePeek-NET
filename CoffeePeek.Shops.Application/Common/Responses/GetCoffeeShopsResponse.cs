using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Shops.Application.Common.Responses;

public class GetCoffeeShopsResponse
{
    public ShortShopDto[] CoffeeShops { get; set; }

    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}