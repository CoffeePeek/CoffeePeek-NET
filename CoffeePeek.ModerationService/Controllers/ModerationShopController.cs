using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Contract.Responses.CoffeeShop.Review;
using Coffeepeek.Moderation.Application.Features.Shop.CreateShop;
using Coffeepeek.Moderation.Application.Features.Shop.GetAllModerationShops;
using Coffeepeek.Moderation.Application.Features.Shop.UpdateModerationShopStatus;
using Coffeepeek.Moderation.Application.UpdateShop;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shared.Infrastructure.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.ModerationService.Controllers;

[ApiController]
[Authorize(Policy = RoleConsts.Admin)]
[Route("api/[controller]")]
public class ModerationShopController(IMediator mediator) : Controller
{
    [HttpGet]
    [Description("Get all coffee shop reviews for moderation")]
    public async Task<Response<GetCoffeeShopsInModerationByIdResponse>> GetAllModerationShops(CancellationToken ct)
    {
        var request = new GetAllModerationShopsQuery();
        return await mediator.Send(request, ct);
    }
    
    [HttpPost]
    [Description("Adds a new coffee shop to moderation")]
    public async Task<Response<SendCoffeeShopToModerationResponse>> SendCoffeeShopToModeration(
        [FromBody] SendCoffeeShopToModerationCommand command, CancellationToken ct)
    {
        var commandWithUser = command with { UserId = User.GetUserIdOrThrow() };

        return await mediator.Send(commandWithUser, ct);
    }

    [HttpPut]
    [Description("Updates a coffee shop to moderation")]
    public async Task<UpdateEntityResponse<ModerationShopDto>> UpdateModerationCoffeeShop(
        [FromForm] ModerationShopDto dto, CancellationToken ct)
    {
        var userId = User.GetUserIdOrThrow();
        var command = new UpdateModerationCoffeeShopCommand(dto, userId);

        return await mediator.Send(command, ct);
    }

    [HttpPut("status")]
    [Description("Updates a review coffee shop status")]
    public async Task<Response> UpdateModerationCoffeeShopStatus(
        [FromQuery, Required] Guid id, 
        [FromQuery, Required] ModerationStatus status, 
        CancellationToken ct)
    {
        var userId = User.GetUserIdOrThrow();
        
        var request = new UpdateModerationCoffeeShopStatusCommand(id, status, userId);
        
        return await mediator.Send(request, ct);
    }
}