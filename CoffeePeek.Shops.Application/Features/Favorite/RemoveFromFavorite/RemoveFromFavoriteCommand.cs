using System.Text.Json.Serialization;

namespace CoffeePeek.Shops.Application.Features.Favorite.RemoveFromFavorite;

public record RemoveFromFavoriteCommand([property: JsonIgnore] Guid UserId, Guid CoffeeShopId);