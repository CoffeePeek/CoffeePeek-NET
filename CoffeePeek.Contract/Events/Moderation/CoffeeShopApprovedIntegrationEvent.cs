using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Contract.Events.Moderation;

public record CoffeeShopApprovedIntegrationEvent(Guid CreatorId, ShopDto Shop);
