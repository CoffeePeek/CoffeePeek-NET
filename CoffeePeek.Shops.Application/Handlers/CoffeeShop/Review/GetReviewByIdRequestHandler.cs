using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop.Review;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Application.Commands.CoffeeShop.Review;
using MapsterMapper;
using MediatR;

namespace CoffeePeek.Shops.Application.Handlers.CoffeeShop.Review;

public class GetReviewByIdRequestHandler(
    IGenericRepository<Shops.Domain.Entities.Review> reviewRepository,
    IMapper mapper)
    : IRequestHandler<GetReviewByIdCommand, Response<GetReviewByIdResponse>>
{
    public async Task<Response<GetReviewByIdResponse>> Handle(GetReviewByIdCommand command,
        CancellationToken cancellationToken)
    {
        var review = await reviewRepository
            .FirstOrDefaultAsNoTrackingAsync(x => x.Id == command.Id, cancellationToken);
        
        if (review is null)
        {
            return Response<GetReviewByIdResponse>.Error("Review not found");
        }

        var reviewDto = mapper.Map<CoffeeShopReviewDto>(review);
        var response = new GetReviewByIdResponse(reviewDto);

        return Response<GetReviewByIdResponse>.Success(response);
    }
}