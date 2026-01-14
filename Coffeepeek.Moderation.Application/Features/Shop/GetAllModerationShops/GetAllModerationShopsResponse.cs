using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace Coffeepeek.Moderation.Application.Features.Shop.GetAllModerationShops;

public record GetAllModerationShopsResponse(ModerationShopDto[] ModerationShop);