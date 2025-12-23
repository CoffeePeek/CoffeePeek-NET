namespace CoffeePeek.Contract.Responses.CoffeeShop.Review;

public record SendCoffeeShopToModerationResponse(Guid ShopId, string Status);