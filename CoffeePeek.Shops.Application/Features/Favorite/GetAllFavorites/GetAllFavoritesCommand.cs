using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop.Favorite;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Favorite.GetAllFavorites;

public record GetAllFavoritesCommand(Guid UserId) : IRequest<Response<GetAllFavoritesResponse>>;