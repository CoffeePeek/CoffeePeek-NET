using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public record GetShopsInBoundsRequest(
    decimal MinLat,
    decimal MinLon,
    decimal MaxLat,
    decimal MaxLon
) : IRequest<Response<GetShopsInBoundsResponse>>;

