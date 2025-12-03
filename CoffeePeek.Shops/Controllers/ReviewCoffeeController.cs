using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Shared.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.Shops.Controllers;

[ApiController]
[Authorize(Policy = RoleConsts.User)]
[Route("api/[controller]")]
public class ReviewCoffeeController(IMediator mediator) : Controller
{
    [HttpGet]
    public Task<Response<GetAllReviewsResponse>> GetAllReviews()
    {
        return mediator.Send(new GetAllReviewsRequest(User.GetUserIdOrThrow()));
    }

    [HttpGet("{id:int}")]
    public Task<Response<GetReviewByIdResponse>> GetReviewById(int id)
    {
        return mediator.Send(new GetReviewByIdRequest(id));
    }

    [HttpPost]
    public Task<Response<AddCoffeeShopReviewResponse>> AddCoffeeShopReview([FromBody] AddCoffeeShopReviewRequest request)
    {
        request.UserId = User.GetUserIdOrThrow();
        return mediator.Send(request);
    }

    [HttpPut]
    public Task<Response<UpdateCoffeeShopReviewResponse>> UpdateCoffeeShopReview([FromBody] UpdateCoffeeShopReviewRequest request)
    {
        request.UserId = User.GetUserIdOrThrow();
        return mediator.Send(request);
    }
}