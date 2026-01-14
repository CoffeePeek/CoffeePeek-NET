using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Application.Features.Review.GetAllReviewsByShopId;

using Review = Domain.Entities.ReviewAggregate.Review;

public class GetAllReviewsByShopIdRequestHandler(
    IGenericRepository<Review> reviewRepository,
    IMapper mapper)
    : IRequestHandler<GetAllReviewsByShopIdQuery, Response<GetAllReviewsResponse>>
{
    public async Task<Response<GetAllReviewsResponse>> Handle(GetAllReviewsByShopIdQuery request, CancellationToken ct)
    {
        if (request.ShopId == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(request.ShopId));
        }
        
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize <= 0 ? 10 : request.PageSize, 1, 100);

        var query = reviewRepository
            .QueryAsNoTracking()
            .Where(r => r.ShopId == request.ShopId)
            .OrderByDescending(r => r.CreatedAtUtc);

        var totalItems = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var userReviews = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
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