using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.Internal;
using MediatR;

namespace CoffeePeek.Contract.Requests.Internal;

public record GetAllBrewMethodsCommand : IRequest<Response<GetAllBrewMethodsResponse>>;