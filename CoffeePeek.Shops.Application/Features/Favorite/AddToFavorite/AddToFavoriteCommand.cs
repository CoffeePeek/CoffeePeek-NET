using System.Text.Json.Serialization;

namespace CoffeePeek.Shops.Application.Features.Favorite.AddToFavorite;

public record AddToFavoriteCommand([property: JsonIgnore] Guid UserId, Guid CoffeeShopId);