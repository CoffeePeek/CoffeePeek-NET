using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Domain.Databases;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.BusinessLogic.RequestHandlers.CoffeeShop.Review;

public class GetReviewByIdRequestHandler(CoffeePeekDbContext dbContext, IMapper mapper) 
    : IRequestHandler<GetReviewByIdRequest, Response<GetReviewByIdResponse>>
{
    public async Task<Response<GetReviewByIdResponse>> Handle(GetReviewByIdRequest request, CancellationToken cancellationToken)
    {
        var review = await dbContext.Reviews
            .AsNoTracking()
            .Include(r => r.Shop)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (review is null)
        {
            return Response.ErrorResponse<Response<GetReviewByIdResponse>>("Review not found");
        }

        var reviewDto = mapper.Map<CoffeeShopReviewDto>(review);
        var response = new GetReviewByIdResponse(reviewDto);

        return Response.SuccessResponse<Response<GetReviewByIdResponse>>(response);
    }
}