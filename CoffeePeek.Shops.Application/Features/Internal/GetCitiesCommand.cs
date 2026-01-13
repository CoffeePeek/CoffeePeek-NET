using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.Internal;
using MediatR;

namespace CoffeePeek.Contract.Requests.Internal;

public record GetCitiesCommand : IRequest<Response<GetCitiesResponse>>;