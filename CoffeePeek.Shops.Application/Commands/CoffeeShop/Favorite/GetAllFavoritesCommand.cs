using CoffeePeek.Contract.Responses.CoffeeShop.Favorite;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public record GetAllFavoritesCommand(Guid UserId) : IRequest<Responses.Response<GetAllFavoritesResponse>>;