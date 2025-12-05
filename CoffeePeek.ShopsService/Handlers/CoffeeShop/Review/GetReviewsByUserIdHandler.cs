using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using CoffeePeek.Data.Interfaces;
using MapsterMapper;
using MediatR;

namespace CoffeePeek.ShopsService.Handlers.CoffeeShop.Review;

public class GetReviewsByUserIdRequestHandler(
    IGenericRepository<Entities.Review> reviewRepository, 
    IMapper mapper)
    : IRequestHandler<GetReviewsByUserIdCommand, Response<GetReviewsByUserIdResponse>>
{
    public async Task<Response<GetReviewsByUserIdResponse>> Handle(GetReviewsByUserIdCommand request,
        CancellationToken cancellationToken)
    {
        var reviews = await reviewRepository
            .FindAsNoTrackingAsync(x => x.UserId == request.UserId, cancellationToken);

        var reviewDtos = mapper.Map<CoffeeShopReviewDto[]>(reviews);

        var response = new GetReviewsByUserIdResponse(reviewDtos);

        return Response<GetReviewsByUserIdResponse>.Success(response);
    }
}