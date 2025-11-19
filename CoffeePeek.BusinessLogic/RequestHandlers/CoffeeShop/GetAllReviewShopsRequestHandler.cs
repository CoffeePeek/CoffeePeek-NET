using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Domain.Entities.Shop;
using CoffeePeek.Domain.UnitOfWork;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.BusinessLogic.RequestHandlers.CoffeeShop;

public class GetAllReviewShopsRequestHandler(
    IRepository<ReviewShop> reviewShopRepository,
    IMapper mapper)
    : IRequestHandler<GetAllReviewShopsRequest, Response<GetCoffeeShopsInReviewByIdResponse>>
{
    public async Task<Response<GetCoffeeShopsInReviewByIdResponse>> Handle(GetAllReviewShopsRequest request, 
        CancellationToken cancellationToken)
    {
        var reviewShops = await reviewShopRepository.GetAll()
            .ToArrayAsync(cancellationToken);

        var reviews = mapper.Map<ReviewShopDto[]>(reviewShops);

        var result = new GetCoffeeShopsInReviewByIdResponse(reviews);
        
        return Response.SuccessResponse<Response<GetCoffeeShopsInReviewByIdResponse>>(result);
    }
}

