using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Contract.Responses.CoffeeShop.Favorite;

public record GetAllFavoritesResponse(List<CoffeeShopDetailsDto> FavoriteShops);
