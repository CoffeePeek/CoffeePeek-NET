using CoffeePeek.Api.Extensions;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewCoffeeController(IMediator mediator) : Controller
{
    [HttpGet]
    [Authorize]
    public Task<Response<GetAllReviewsResponse>> GetAllReviews()
    {
        return mediator.Send(new GetAllReviewsRequest(HttpContext.GetUserIdOrThrow()));
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public Task<Response<GetReviewByIdResponse>> GetAllReviews(int id)
    {
        return mediator.Send(new GetReviewByIdRequest(id));
    }

    [HttpPost]
    [Authorize]
    public Task<Response<AddCoffeeShopReviewResponse>> AddCoffeeShopReview([FromBody] AddCoffeeShopReviewRequest request)
    {
        request.UserId = HttpContext.GetUserIdOrThrow();
        return mediator.Send(request);
    }

    [HttpPut]
    [Authorize]
    public Task<Response<UpdateCoffeeShopReviewResponse>> UpdateCoffeeShopReview([FromBody] UpdateCoffeeShopReviewRequest request)
    {
        request.UserId = HttpContext.GetUserIdOrThrow();
        return mediator.Send(request);
    }
}