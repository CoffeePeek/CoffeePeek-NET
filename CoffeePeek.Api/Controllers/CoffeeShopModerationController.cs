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
public class CoffeeShopModerationController(IMediator mediator, IUserContextService userContextService) : Controller
{
     [HttpGet]
     [Description("Get all user coffee shop moderations")]
     public async Task<Response<GetCoffeeShopsInModerationByIdResponse>> GetCoffeeShopsInModerationByUserId()
     {
         if (!userContextService.TryGetUserId(out var userId))
         {
             return Contract.Response.Response.ErrorResponse<Response<GetCoffeeShopsInModerationByIdResponse>>(
                 "User ID not found or invalid.");
         }
    
         var request = new GetCoffeeShopsInModerationByIdRequest(userId);
         
         return await mediator.Send(request);
     }

     [HttpGet("all")]
     [Description("Get all coffee shop reviews for moderation")]
     public async Task<Response<GetCoffeeShopsInModerationByIdResponse>> GetAllModerationShops()
     {
         // TODO: Add role-based authorization (Admin only)
         var request = new GetAllModerationShopsRequest();
         return await mediator.Send(request);
     }
    
    [HttpPost]
    [Description("Adds a new coffee shop to moderation")]
    public async Task<Response<SendCoffeeShopToModerationResponse>> SendCoffeeShopToModeration(
        [FromBody] SendCoffeeShopToModerationRequest request)
    {
        if (!userContextService.TryGetUserId(out var userId))
        {
            return Contract.Response.Response.ErrorResponse<Response<SendCoffeeShopToModerationResponse>>(
                "User ID not found or invalid.");
        }

        request.UserId = userId;

        return await mediator.Send(request);
    }

    [HttpPut]
    [Description("Updates a coffee shop to moderation")]
    public async Task<Response<UpdateModerationCoffeeShopResponse>> UpdateModerationCoffeeShop(
        [FromForm] UpdateModerationCoffeeShopRequest request)
    {
        if (!userContextService.TryGetUserId(out var userId))
        {
            return Contract.Response.Response.ErrorResponse<Response<UpdateModerationCoffeeShopResponse>>(
                "User ID not found or invalid.");
        }

        request.UserId = userId;

        return await mediator.Send(request);
    }

    [HttpPut("status")]
    [Description("Updates a review coffee shop status(TODO add role auth)")]
    public async Task<Response> UpdateModerationCoffeeShopStatus([FromQuery] int id, 
        [FromQuery] ModerationStatus status)
    {
        if (!userContextService.TryGetUserId(out var userId))
        {
            return Contract.Response.Response.ErrorResponse<Response<UpdateModerationCoffeeShopResponse>>(
                "User ID not found or invalid.");
        }
        
        var request = new UpdateModerationCoffeeShopStatusRequest(id, status, userId);
        
        return await mediator.Send(request);
    }
}