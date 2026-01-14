using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Application.Features.Review.GetReviewsByUserId;

public class GetReviewsByUserIdRequestHandler(
    IGenericRepository<Shops.Domain.Entities.ReviewAggregate.Review> reviewRepository, 
    IMapper mapper)
    : IRequestHandler<GetReviewsByUserIdQuery, Response<GetReviewsByUserIdResponse>>
{
    public async Task<Response<GetReviewsByUserIdResponse>> Handle(GetReviewsByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        var query = reviewRepository
            .QueryAsNoTracking()
            .Where(x => x.UserId == request.UserId)
            .OrderByDescending(r => r.CreatedAtUtc);

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

        var reviews = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var reviewDtos = mapper.Map<ReviewDto[]>(reviews);

        var response = new GetReviewsByUserIdResponse(
            reviewDtos,
            totalItems,
            totalPages,
            request.PageNumber,
            request.PageSize);

        return Response<GetReviewsByUserIdResponse>.Success(response);
    }
}