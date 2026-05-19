using System.Text.Json.Serialization;

namespace CoffeePeek.Shops.Application.Features.Review.DeleteReviewFromCoffeeShop;

public record DeleteReviewFromCoffeeShopCommand(Guid ReviewId, [property: JsonIgnore] Guid RequestingUserId);
