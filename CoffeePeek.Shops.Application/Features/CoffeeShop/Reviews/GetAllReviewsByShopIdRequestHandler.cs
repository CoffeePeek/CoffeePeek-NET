using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Application.Commands.CoffeeShop;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.Reviews;

using Review = Domain.Entities.ReviewAggregate.Review;

public class GetAllReviewsByShopIdRequestHandler(
    IGenericRepository<Review> reviewRepository,
    IMapper mapper)
    : IRequestHandler<GetAllReviewsByShopIdQuery, Response<GetAllReviewsResponse>>
{
    public async Task<Response<GetAllReviewsResponse>> Handle(GetAllReviewsByShopIdQuery request, CancellationToken ct)
    {
        var query = reviewRepository
            .QueryAsNoTracking()
            .Where(r => r.ShopId == request.ShopId)
            .OrderByDescending(r => r.CreatedAtUtc);

        var totalItems = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

        var userReviews = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var reviewDtos = mapper.Map<ReviewDto[]>(userReviews);

        var response = new GetAllReviewsResponse(
            reviews: reviewDtos,
            totalItems: totalItems,
            totalPages: totalPages,
            currentPage: request.PageNumber,
            pageSize: request.PageSize);

        return Response<GetAllReviewsResponse>.Success(response);
    }
}