using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Internal;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Contract.Requests.Internal;

public record GetUserStatisticsCommand(Guid UserId) : IRequest<Response<GetUserStatisticsResponse>>;


