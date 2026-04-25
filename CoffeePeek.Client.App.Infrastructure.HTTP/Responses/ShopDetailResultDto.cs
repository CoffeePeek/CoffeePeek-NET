using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Responses;

public sealed class ShopDetailResultDto
{
    public CoffeeShopDetailsDto ShopDto { get; set; } = null!;
}
