using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using CoffeePeek.Shared.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.ShopsService.Controllers;

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

    [HttpGet("{id:guid}")]
    public Task<Response<GetReviewByIdResponse>> GetReviewById(Guid id)
    {
        return mediator.Send(new GetReviewByIdCommand(id));
    }
    
    [HttpGet("user/{id:guid}")]
    public Task<Response<GetReviewsByUserIdResponse>> GetReviewsByUserId(Guid id)
    {
        return mediator.Send(new GetReviewsByUserIdCommand(id));
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