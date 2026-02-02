using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Catalogs.GetAllCities;

public record GetCitiesCommand : IRequest<Response<GetCitiesResponse>>;