namespace CoffeePeek.Moderation.Application.Features.Shop.CreateShop;

public record SendCoffeeShopToModerationResponse(Guid ShopId, string Status, bool IsAddressValidated);