namespace CoffeePeek.Contract.Events.Shops;

public record ReviewAddedEvent(Guid UserId, Guid ShopId, Guid ReviewId);