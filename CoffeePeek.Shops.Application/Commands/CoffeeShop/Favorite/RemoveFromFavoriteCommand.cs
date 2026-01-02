using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Shops.Application.Commands.CoffeeShop.Favorite;

public record RemoveFromFavoriteCommand(Guid CoffeeShopId, Guid UserId) : IRequest<UpdateEntityResponse<Guid>>;