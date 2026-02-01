using CoffeePeek.Contract.Abstract;
using MediatR;
using Newtonsoft.Json;

namespace CoffeePeek.Shops.Application.Features.Favorite.AddToFavorite;

public record AddToFavoriteCommand([property: JsonIgnore] Guid UserId, Guid CoffeeShopId)
    : IRequest<CreateEntityResponse<Guid>>;