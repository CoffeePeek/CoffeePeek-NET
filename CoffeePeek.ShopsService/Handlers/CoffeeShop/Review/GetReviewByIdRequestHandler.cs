using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Data.Interfaces;
using MapsterMapper;
using MediatR;

namespace CoffeePeek.ShopsService.Handlers.CoffeeShop.Review;

public class GetReviewByIdRequestHandler(
    IGenericRepository<Entities.Review> reviewRepository,
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