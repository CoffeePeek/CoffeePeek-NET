using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Internal.GetUserStatistics;

public record GetUserStatisticsCommand(Guid UserId) : IRequest<Response<GetUserStatisticsResponse>>;