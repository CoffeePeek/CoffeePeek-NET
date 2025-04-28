using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Data;
using CoffeePeek.Domain.Entities.Shop;
using CoffeePeek.Domain.UnitOfWork;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.BusinessLogic.RequestHandlers.CoffeeShop;

public class GetCoffeeShopsInReviewByIdRequestHandler(
    IRepository<ReviewShop> reviewShopRepository,
    IMapper mapper)
    : IRequestHandler<GetCoffeeShopsInReviewByIdRequest, Response<GetCoffeeShopsInReviewByIdResponse>>
{
    public async Task<Response<GetCoffeeShopsInReviewByIdResponse>> Handle(GetCoffeeShopsInReviewByIdRequest request, 
        CancellationToken cancellationToken)
    {
        var reviewShops = await reviewShopRepository.FindBy(x => x.UserId == request.UserId)
            .ToArrayAsync(cancellationToken);

        var reviews = mapper.Map<ReviewShopDto[]>(reviewShops);

        var result = new GetCoffeeShopsInReviewByIdResponse(reviews);
        
        return Response.SuccessResponse<Response<GetCoffeeShopsInReviewByIdResponse>>(result);
    }
}