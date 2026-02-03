using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Moderation.Application.Features.Shop.CreateShop;
using CoffeePeek.Moderation.Application.Features.Shop.GetAllModerationShops;
using CoffeePeek.Moderation.Application.Features.Shop.UpdateModerationShopStatus;
using CoffeePeek.Moderation.Application.Features.Shop.UpdateShop;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shared.Infrastructure.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CoffeePeek.ModerationService.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class ModerationShopsController(IMediator mediator, IUserContext userContext) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = RoleConsts.Moderator)]
    [Description("Get all coffee shop reviews for moderation")]
    [ProducesResponseType<Response<GetAllModerationShopsResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Get all coffee shops for moderation")]
    public async Task<Response<GetAllModerationShopsResponse>> GetAllModerationShops(CancellationToken ct)
    {
        var request = new GetAllModerationShopsQuery();
        return await mediator.Send(request, ct);
    }

    [HttpPost]
    [Description("Adds a new coffee shop to moderation")]
    [ProducesResponseType<Response<SendCoffeeShopToModerationResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Adds a new coffee shop to moderation")]
    public async Task<IActionResult> SendCoffeeShopToModeration(
        [FromBody] SendCoffeeShopToModerationCommand command, CancellationToken ct)
    {
        var commandWithUser = command with { UserId = userContext.GetUserIdOrThrow() };

        var response = await mediator.Send(commandWithUser, ct);

        return Ok(response);
    }

    [HttpPut]
    [Authorize(Policy = RoleConsts.Moderator)]
    [SwaggerOperation("Updates a coffee shop to moderation")]
    public async Task<UpdateEntityResponse<ModerationShopDto>> UpdateModerationCoffeeShop(
        [FromForm] ModerationShopDto dto, CancellationToken ct)
    {
        var userId = userContext.GetUserIdOrThrow();
        var command = new UpdateModerationCoffeeShopCommand(dto, userId);

        return await mediator.Send(command, ct);
    }

    [HttpPut("status")]
    [Authorize(Policy = RoleConsts.Moderator)]
    [SwaggerOperation("Updates a review coffee shop status")]
    public async Task<Response> UpdateModerationCoffeeShopStatus(
        [FromQuery, Required] Guid id,
        [FromQuery, Required] ModerationStatus status,
        CancellationToken ct)
    {
        var request = new UpdateModerationCoffeeShopStatusCommand(userContext.GetUserIdOrThrow(), id, status);

        return await mediator.Send(request, ct);
    }
}