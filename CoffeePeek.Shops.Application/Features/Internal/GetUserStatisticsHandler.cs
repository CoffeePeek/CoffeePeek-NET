using CoffeePeek.Contract.Requests.Internal;
using CoffeePeek.Contract.Response.Internal;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Entities;
using MediatR;
using Review = CoffeePeek.Shops.Domain.Entities.ReviewAggregate.Review;

namespace CoffeePeek.Shops.Application.Handlers.Internal;

public class GetUserStatisticsHandler (
    IGenericRepository<CheckIn> checkInRepository,
    IGenericRepository<Review> reviewsRepository)
    : IRequestHandler<GetUserStatisticsCommand, Response<GetUserStatisticsResponse>>
{
    public async Task<Response<GetUserStatisticsResponse>> Handle(GetUserStatisticsCommand request, CancellationToken cancellationToken)
    {
        //TODO: CP-106 add hybrid cache
        var checkInCount = await checkInRepository.CountAsync(c => c.UserId == request.UserId, cancellationToken);

        var reviewCount = await reviewsRepository.CountAsync(r => r.UserId == request.UserId, cancellationToken);

        var response = new GetUserStatisticsResponse(checkInCount, reviewCount);
        
        return Response<GetUserStatisticsResponse>.Success(response);
    }
}