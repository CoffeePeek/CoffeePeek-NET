using System.Text.Json.Serialization;
using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Favorite.RemoveFromFavorite;

public record RemoveFromFavoriteCommand([property: JsonIgnore] Guid UserId, Guid CoffeeShopId)
    : IRequest<UpdateEntityResponse<Guid>>;