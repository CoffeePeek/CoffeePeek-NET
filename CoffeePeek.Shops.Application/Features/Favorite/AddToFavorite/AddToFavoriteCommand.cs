using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Favorite.AddToFavorite;

public record AddToFavoriteCommand(Guid CoffeeShopId, Guid UserId) : IRequest<CreateEntityResponse<Guid>>;
