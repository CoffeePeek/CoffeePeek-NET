using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Contract.Response.CoffeeShop;

public class GetCoffeeShopsInModerationByIdResponse(ModerationShopDto[] moderationShop)
{
    public ModerationShopDto[] ModerationShop { get; } = moderationShop;
}