using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Responses;

public sealed class GetAllModerationShopsResultDto
{
    public ModerationShopDto[] ModerationShops { get; init; } = [];
}
