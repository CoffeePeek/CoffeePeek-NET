using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Favorite.RemoveFromFavorite;

public record RemoveFromFavoriteCommand(Guid CoffeeShopId, Guid UserId) : IRequest<UpdateEntityResponse<Guid>>;