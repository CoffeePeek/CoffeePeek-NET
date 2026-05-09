using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Moderation.Application.Features.Shop.GetAllModerationShops;

public record GetAllModerationShopsResponse(ModerationShopDto[] ModerationShops);