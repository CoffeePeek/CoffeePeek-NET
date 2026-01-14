using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Entities;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Internal.GetUserStatistics;

public class GetUserStatisticsHandler (
    IGenericRepository<CheckIn> checkInRepository,
    IGenericRepository<Domain.Entities.ReviewAggregate.Review> reviewsRepository)
    : IRequestHandler<GetUserStatisticsCommand, Response<GetUserStatisticsResponse>>
{
    public async Task<Response<GetUserStatisticsResponse>> Handle(GetUserStatisticsCommand request, CancellationToken cancellationToken)
    {
        var checkInCount = await checkInRepository.CountAsync(c => c.UserId == request.UserId, cancellationToken);

        var reviewCount = await reviewsRepository.CountAsync(r => r.UserId == request.UserId, cancellationToken);

        var response = new GetUserStatisticsResponse(checkInCount, reviewCount);
        
        return Response<GetUserStatisticsResponse>.Success(response);
    }
}