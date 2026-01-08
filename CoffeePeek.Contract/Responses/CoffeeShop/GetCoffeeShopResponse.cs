using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Contract.Responses.CoffeeShop;

public sealed record GetCoffeeShopResponse(CoffeeShopDetailsDto ShopDto);
