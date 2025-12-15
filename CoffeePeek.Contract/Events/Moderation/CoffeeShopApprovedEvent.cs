using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Contract.Events.Moderation;

public record CoffeeShopApprovedEvent(Guid CreatorId, ShopDto Shop);