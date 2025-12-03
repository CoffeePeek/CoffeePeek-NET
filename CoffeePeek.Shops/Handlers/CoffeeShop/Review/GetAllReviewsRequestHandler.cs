using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Domain.Databases;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.BusinessLogic.RequestHandlers.CoffeeShop.Review;

public class GetAllReviewsRequestHandler(CoffeePeekDbContext dbContext, IMapper mapper) 
    : IRequestHandler<GetAllReviewsRequest, Response<GetAllReviewsResponse>>
{
    public async Task<Response<GetAllReviewsResponse>> Handle(GetAllReviewsRequest request, CancellationToken cancellationToken)
    {
        var userReviews = await dbContext.Reviews
            .AsNoTracking()
            .Include(r => r.Shop)
            .Where(r => r.UserId == request.UserId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        var reviewDtos = mapper.Map<CoffeeShopReviewDto[]>(userReviews);

        var response = new GetAllReviewsResponse(reviewDtos);

        return Response.SuccessResponse<Response<GetAllReviewsResponse>>(response);
    }
}