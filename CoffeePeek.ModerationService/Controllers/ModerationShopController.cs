using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using Coffeepeek.Moderation.Application.Features.Shop.CreateShop;
using Coffeepeek.Moderation.Application.Features.Shop.GetAllModerationShops;
using CoffeePeek.Moderation.Application.Features.Shop.UpdateModerationShopStatus;
using Coffeepeek.Moderation.Application.UpdateShop;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shared.Infrastructure.Constants;
using FluentResults;
using FluentResults.Extensions.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.ModerationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModerationShopController(IMediator mediator) : Controller
{
    [HttpGet]
    [Authorize(Policy = RoleConsts.Moderator)]
    [Description("Get all coffee shop reviews for moderation")]
    public async Task<Response<GetAllModerationShopsResponse>> GetAllModerationShops(CancellationToken ct)
    {
        var request = new GetAllModerationShopsQuery();
        return await mediator.Send(request, ct);
    }

    [HttpPost]
    [Description("Adds a new coffee shop to moderation")]
    [ProducesResponseType(typeof(SendCoffeeShopToModerationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SendCoffeeShopToModeration(
        [FromBody] SendCoffeeShopToModerationCommand command, CancellationToken ct)
    {
        var commandWithUser = command with { UserId = User.GetUserIdOrThrow() };

        var result =  await mediator.Send(commandWithUser, ct);
        
        return Ok(result);
    }

    [HttpPut]
    [Authorize(Policy = RoleConsts.Moderator)]
    [Description("Updates a coffee shop to moderation")]
    public async Task<UpdateEntityResponse<ModerationShopDto>> UpdateModerationCoffeeShop(
        [FromForm] ModerationShopDto dto, CancellationToken ct)
    {
        var userId = User.GetUserIdOrThrow();
        var command = new UpdateModerationCoffeeShopCommand(dto, userId);

        return await mediator.Send(command, ct);
    }

    [HttpPut("status")]
    [Authorize(Policy = RoleConsts.Moderator)]
    [Description("Updates a review coffee shop status")]
    public async Task<Response> UpdateModerationCoffeeShopStatus(
        [FromQuery, Required] Guid id,
        [FromQuery, Required] ModerationStatus status,
        CancellationToken ct)
    {
        var request = new UpdateModerationCoffeeShopStatusCommand(User.GetUserIdOrThrow(),id, status);

        return await mediator.Send(request, ct);
    }
}