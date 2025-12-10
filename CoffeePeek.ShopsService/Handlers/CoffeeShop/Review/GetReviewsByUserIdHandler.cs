using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Data.Interfaces;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.ShopsService.Handlers.CoffeeShop.Review;

public class GetReviewsByUserIdRequestHandler(
    IGenericRepository<Entities.Review> reviewRepository, 
    IMapper mapper)
    : IRequestHandler<GetReviewsByUserIdCommand, Response<GetReviewsByUserIdResponse>>
{
    public async Task<Response<GetReviewsByUserIdResponse>> Handle(GetReviewsByUserIdCommand request,
        CancellationToken cancellationToken)
    {
        var query = reviewRepository
            .QueryAsNoTracking()
            .Where(x => x.UserId == request.UserId)
            .OrderByDescending(r => r.CreatedAt);

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

        var reviews = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var reviewDtos = mapper.Map<CoffeeShopReviewDto[]>(reviews);

        var response = new GetReviewsByUserIdResponse(
            reviewDtos,
            totalItems,
            totalPages,
            request.PageNumber,
            request.PageSize);

        return Response<GetReviewsByUserIdResponse>.Success(response);
    }
}