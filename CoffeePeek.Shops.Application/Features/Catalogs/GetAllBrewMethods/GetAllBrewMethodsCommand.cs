using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Catalogs.GetAllBrewMethods;

public record GetAllBrewMethodsCommand : IRequest<Response<GetAllBrewMethodsResponse>>;