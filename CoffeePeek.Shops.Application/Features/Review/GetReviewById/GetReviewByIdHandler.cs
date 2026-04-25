using System.Net;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;
using MapsterMapper;

namespace CoffeePeek.Shops.Application.Features.Review.GetReviewById;

public class GetReviewByIdRequestHandler
{
    public async Task<Response<GetReviewByIdResponse>> Handle(GetReviewByIdQuery query,
        IQueryReviewRepository reviewRepository,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetById(query.Id, cancellationToken);

        if (review is null)
        {
            return Response<GetReviewByIdResponse>.Error(HttpStatusCode.NotFound, "Review not found");
        }

        var reviewDto = mapper.Map<ReviewDto>(review);
        var response = new GetReviewByIdResponse(reviewDto);

        return Response<GetReviewByIdResponse>.Success(response);
    }
}