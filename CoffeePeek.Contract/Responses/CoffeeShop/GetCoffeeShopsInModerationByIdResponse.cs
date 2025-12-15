using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Contract.Responses.CoffeeShop;

public class GetCoffeeShopsInModerationByIdResponse(ModerationShopDto[] moderationShop)
{
    public ModerationShopDto[] ModerationShop { get; } = moderationShop;
}