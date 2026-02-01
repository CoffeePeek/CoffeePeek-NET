using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shops.Application.Features.Internal.GetAllCities;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Catalogs.GetAllCities;

public record GetCitiesCommand : IRequest<Response<GetCitiesResponse>>;