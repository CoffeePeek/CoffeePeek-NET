using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public record AddToFavoriteCommand(Guid CoffeeShopId, Guid UserId) : IRequest<CreateEntityResponse<Guid>>;
