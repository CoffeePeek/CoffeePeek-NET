using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Shops.Domain;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.GetShopsInBounds;

public record GetShopsInBoundsRequest(
    [Range(-BusinessConstants.MaxLocationLatitude, BusinessConstants.MaxLocationLatitude)] decimal MinLat,
    [Range(-BusinessConstants.MaxLocationLongitude, BusinessConstants.MaxLocationLongitude)] decimal MinLon,
    [Range(-BusinessConstants.MaxLocationLatitude, BusinessConstants.MaxLocationLatitude)] decimal MaxLat,
    [Range(-BusinessConstants.MaxLocationLongitude, BusinessConstants.MaxLocationLongitude)] decimal MaxLon
) : IRequest<Response<GetShopsInBoundsResponse>>;