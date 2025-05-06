using System.ComponentModel;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using CoffeePeek.Domain.Enums.Shop;
using CoffeePeek.Infrastructure.Services.Auth.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CoffeeShopReviewController(IMediator mediator, IUserContextService userContextService) : Controller
{
     [HttpGet]
     [Description("Get all user coffee shop reviews")]
     public async Task<Response<GetCoffeeShopsInReviewByIdResponse>> GetCoffeeShopsInReviewByUserId()
     {
         if (!userContextService.TryGetUserId(out var userId))
         {
             return Contract.Response.Response.ErrorResponse<Response<GetCoffeeShopsInReviewByIdResponse>>(
                 "User ID not found or invalid.");
         }
    
         var request = new GetCoffeeShopsInReviewByIdRequest(userId);
         
         return await mediator.Send(request);
     }
    
    [HttpPost]
    [Description("Adds a new coffee shop to review")]
    public async Task<Response<SendCoffeeShopToReviewResponse>> SendCoffeeShopToReview(
        [FromBody] SendCoffeeShopToReviewRequest request)
    {
        if (!userContextService.TryGetUserId(out var userId))
        {
            return Contract.Response.Response.ErrorResponse<Response<SendCoffeeShopToReviewResponse>>(
                "User ID not found or invalid.");
        }

        request.UserId = userId;

        return await mediator.Send(request);
    }

    [HttpPut]
    [Description("Updates a coffee shop to review")]
    public async Task<Response<UpdateReviewCoffeeShopResponse>> UpdateReviewCoffeeShop(
        [FromForm] UpdateReviewCoffeeShopRequest request)
    {
        if (!userContextService.TryGetUserId(out var userId))
        {
            return Contract.Response.Response.ErrorResponse<Response<UpdateReviewCoffeeShopResponse>>(
                "User ID not found or invalid.");
        }

        request.UserId = userId;

        return await mediator.Send(request);
    }

    [HttpPut("status")]
    [Description("Updates a review coffee shop status(TODO add role auth)")]
    public async Task<Response> UpdateReviewCoffeeShopStatus([FromQuery] int id, 
        [FromQuery] ReviewStatus status)
    {
        if (!userContextService.TryGetUserId(out var userId))
        {
            return Contract.Response.Response.ErrorResponse<Response<UpdateReviewCoffeeShopResponse>>(
                "User ID not found or invalid.");
        }
        
        var request = new UpdateReviewCoffeeShopStatusRequest(id, status, userId);
        
        return await mediator.Send(request);
    }
}