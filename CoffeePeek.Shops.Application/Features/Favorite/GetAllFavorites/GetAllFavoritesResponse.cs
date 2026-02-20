using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Shops.Application.Features.Favorite.GetAllFavorites;

public record GetAllFavoritesResponse(CoffeeShopDetailsDto[] FavoriteShops);