using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public record RemoveFromFavoriteCommand(Guid CoffeeShopId, Guid UserId) : IRequest<UpdateEntityResponse<Guid>>;