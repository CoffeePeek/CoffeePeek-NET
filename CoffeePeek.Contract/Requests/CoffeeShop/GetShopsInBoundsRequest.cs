using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public record GetShopsInBoundsRequest(
    [Range(-90, 90)] decimal MinLat,
    [Range(-180, 180)] decimal MinLon,
    [Range(-90, 90)] decimal MaxLat,
    [Range(-180, 180)] decimal MaxLon
) : IRequest<Response<GetShopsInBoundsResponse>>;