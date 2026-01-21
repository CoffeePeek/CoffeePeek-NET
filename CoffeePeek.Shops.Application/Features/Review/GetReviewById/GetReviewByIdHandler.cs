using System.Net;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shops.Domain.Entities.ReviewAggregate;
using MapsterMapper;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Review.GetReviewById;

public class GetReviewByIdRequestHandler(
    IReviewRepository reviewRepository,
    IMapper mapper)
    : IRequestHandler<GetReviewByIdQuery, Response<GetReviewByIdResponse>>
{
    public async Task<Response<GetReviewByIdResponse>> Handle(GetReviewByIdQuery query,
        CancellationToken cancellationToken)
    {
        var review = await reviewRepository
            .GetByIdAsNoTracking(query.Id, cancellationToken);
        
        if (review is null)
        {
            return Response<GetReviewByIdResponse>.Error(HttpStatusCode.NotFound,"Review not found");
        }

        var reviewDto = mapper.Map<ReviewDto>(review);
        var response = new GetReviewByIdResponse(reviewDto);

        return Response<GetReviewByIdResponse>.Success(response);
    }
}