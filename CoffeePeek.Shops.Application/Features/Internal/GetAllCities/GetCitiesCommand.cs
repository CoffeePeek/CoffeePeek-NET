using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Internal.GetAllCities;

public record GetCitiesCommand : IRequest<Response<GetCitiesResponse>>;