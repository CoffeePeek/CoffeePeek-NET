using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop.Favorite;
using MediatR;

namespace CoffeePeek.Shops.Application.Commands.CoffeeShop.Favorite;

public record GetAllFavoritesCommand(Guid UserId) : IRequest<Response<GetAllFavoritesResponse>>;