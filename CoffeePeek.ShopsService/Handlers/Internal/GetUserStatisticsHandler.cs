using CoffeePeek.Contract.Requests.Internal;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Internal;
using CoffeePeek.Contract.Responses;
using CoffeePeek.ShopsService.DB;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.ShopsService.Handlers.Internal;

public class GetUserStatisticsHandler(ShopsDbContext dbContext)
    : IRequestHandler<GetUserStatisticsCommand, Response<GetUserStatisticsResponse>>
{
    public async Task<Response<GetUserStatisticsResponse>> Handle(GetUserStatisticsCommand request, CancellationToken cancellationToken)
    {
        var checkInCount = await dbContext.CheckIns
            .AsNoTracking()
            .CountAsync(c => c.UserId == request.UserId, cancellationToken);

        var reviewCount = await dbContext.Reviews
            .AsNoTracking()
            .CountAsync(r => r.UserId == request.UserId, cancellationToken);

        var response = new GetUserStatisticsResponse(checkInCount, reviewCount);
        
        return Response<GetUserStatisticsResponse>.Success(response);
    }
}