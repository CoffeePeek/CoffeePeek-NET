using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Moderation.Application.Features.Shop.UpdateShop;

public record UpdateModerationCoffeeShopCommand(ModerationShopDto ModerationShopDto, Guid UserId);