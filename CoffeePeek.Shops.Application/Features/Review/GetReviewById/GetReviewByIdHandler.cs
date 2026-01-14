using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MapsterMapper;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Review.GetReviewById;

public class GetReviewByIdRequestHandler(
    IGenericRepository<Domain.Entities.ReviewAggregate.Review> reviewRepository,
    IMapper mapper)
    : IRequestHandler<GetReviewByIdQuery, Response<GetReviewByIdResponse>>
{
    public async Task<Response<GetReviewByIdResponse>> Handle(GetReviewByIdQuery query,
        CancellationToken cancellationToken)
    {
        var review = await reviewRepository
            .FirstOrDefaultAsNoTrackingAsync(x => x.Id == query.Id, cancellationToken);
        
        if (review is null)
        {
            return Response<GetReviewByIdResponse>.Error("Review not found");
        }

        var reviewDto = mapper.Map<ReviewDto>(review);
        var response = new GetReviewByIdResponse(reviewDto);

        return Response<GetReviewByIdResponse>.Success(response);
    }
}