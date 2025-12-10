using CoffeePeek.Contract.Response.Internal;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Contract.Requests.Internal;

public record GetCitiesCommand : IRequest<Response<GetCitiesResponse>>;