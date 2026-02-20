using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Moderation.Application.Features.Shop.CreateShop;
using CoffeePeek.Moderation.Application.Features.Shop.GetAllModerationShops;
using CoffeePeek.Moderation.Application.Features.Shop.UpdateModerationShopStatus;
using CoffeePeek.Moderation.Application.Features.Shop.UpdateShop;
using CoffeePeek.Shared.Auth;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.ModerationService.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class ModerationShopsController(IMessageBus bus, IUserContext userContext) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = RoleConsts.Moderator)]
    [Description("Get all coffee shop reviews for moderation")]
    [ProducesResponseType<Response<GetAllModerationShopsResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllModerationShops(CancellationToken ct)
    {
        var request = new GetAllModerationShopsQuery();
        var response = await bus.InvokeAsync<Response<GetAllModerationShopsResponse>>(request, ct);
        return Ok(response);
    }

    [HttpPost]
    [Description("Adds a new coffee shop to moderation")]
    [ProducesResponseType<Response<SendCoffeeShopToModerationResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SendCoffeeShopToModeration(
        [FromBody] SendCoffeeShopToModerationCommand command, CancellationToken ct)
    {
        var commandWithUser = command with { UserId = userContext.GetUserIdOrThrow() };

        var response = await bus.InvokeAsync<Response<SendCoffeeShopToModerationResponse>>(commandWithUser, ct);

        return Ok(response);
    }

    [HttpPut]
    [Authorize(Policy = RoleConsts.Moderator)]
    public async Task<IActionResult> UpdateModerationCoffeeShop(
        [FromForm] ModerationShopDto dto, CancellationToken ct)
    {
        var userId = userContext.GetUserIdOrThrow();
        var command = new UpdateModerationCoffeeShopCommand(dto, userId);

        var response = await bus.InvokeAsync<UpdateEntityResponse<ModerationShopDto>>(command, ct);
        return Ok(response);
    }

    [HttpPut("status")]
    [Authorize(Policy = RoleConsts.Moderator)]
    public async Task<IActionResult> UpdateModerationCoffeeShopStatus(
        [FromQuery, Required] Guid id,
        [FromQuery, Required] ModerationStatus status,
        CancellationToken ct)
    {
        var request = new UpdateModerationCoffeeShopStatusCommand(userContext.GetUserIdOrThrow(), id, status);

        var response = await bus.InvokeAsync<Response>(request, ct);
        return Ok(response);
    }
}
