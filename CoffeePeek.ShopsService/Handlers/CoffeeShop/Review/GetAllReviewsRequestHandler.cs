using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.ShopsService.DB;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.ShopsService.Handlers.CoffeeShop.Review;

public class GetAllReviewsRequestHandler(ShopsDbContext dbContext, IMapper mapper) 
    : IRequestHandler<GetAllReviewsRequest, Response<GetAllReviewsResponse>>
{
    public async Task<Response<GetAllReviewsResponse>> Handle(GetAllReviewsRequest request, CancellationToken cancellationToken)
    {
        var query = dbContext.Reviews
            .AsNoTracking()
            .Include(r => r.Shop)
            .Where(r => r.UserId == request.UserId)
            .OrderByDescending(r => r.CreatedAt);

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

        var userReviews = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var reviewDtos = mapper.Map<CoffeeShopReviewDto[]>(userReviews);

        var response = new GetAllReviewsResponse(
            reviews: reviewDtos,
            totalItems: totalItems,
            totalPages: totalPages,
            currentPage: request.PageNumber,
            pageSize: request.PageSize);

        return Response<GetAllReviewsResponse>.Success(response);
    }
}