using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shared.Infrastructure.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.ModerationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModerationController(IMediator mediator) : Controller
{
    [HttpGet]
    [Authorize]
    [Description("Get all user coffee shop moderations")]
    public async Task<Response<GetCoffeeShopsInModerationByIdResponse>> GetCoffeeShopsInModerationByUserId()
    {
        var userId = User.GetUserIdOrThrow();
        var request = new GetCoffeeShopsInModerationByIdRequest(userId);
        
        return await mediator.Send(request);
    }

    [HttpGet("all")]
    [Authorize(Policy = RoleConsts.Admin)]
    [Description("Get all coffee shop reviews for moderation")]
    public async Task<Response<GetCoffeeShopsInModerationByIdResponse>> GetAllModerationShops()
    {
        var request = new GetAllModerationShopsRequest();
        return await mediator.Send(request);
    }
    
    [HttpPost]
    [Authorize]
    [Description("Adds a new coffee shop to moderation")]
    public async Task<Response<SendCoffeeShopToModerationResponse>> SendCoffeeShopToModeration(
        [FromBody] SendCoffeeShopToModerationRequest request)
    {
        var userId = User.GetUserIdOrThrow();
        request.UserId = userId;

        return await mediator.Send(request);
    }

    [HttpPut]
    [Authorize]
    [Description("Updates a coffee shop to moderation")]
    public async Task<Response<UpdateModerationCoffeeShopResponse>> UpdateModerationCoffeeShop(
        [FromForm] UpdateModerationCoffeeShopRequest request)
    {
        var userId = User.GetUserIdOrThrow();
        request.UserId = userId;

        return await mediator.Send(request);
    }

    [HttpPut("status")]
    [Authorize(Policy = RoleConsts.Admin)]
    [Description("Updates a review coffee shop status")]
    public async Task<Response> UpdateModerationCoffeeShopStatus(
        [FromQuery, Required] Guid id, 
        [FromQuery, Required] ModerationStatus status)
    {
        var userId = User.GetUserIdOrThrow();
        
        var request = new UpdateModerationCoffeeShopStatusRequest(id, status, userId);
        
        return await mediator.Send(request);
    }
}
