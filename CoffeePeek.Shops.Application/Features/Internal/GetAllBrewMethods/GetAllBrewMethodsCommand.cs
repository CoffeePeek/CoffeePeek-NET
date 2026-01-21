using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Internal.GetAllBrewMethods;

public record GetAllBrewMethodsCommand : IRequest<Response<GetAllBrewMethodsResponse>>;