using CoffeePeek.Shared.Auth;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Features.Favorite.AddToFavorite;
using CoffeePeek.Shops.Application.Features.Favorite.RemoveFromFavorite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.ShopsService.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class FavoriteCoffeeShopsController(IMessageBus bus, IUserContext userContext) : ControllerBase
{
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CreateEntityResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> AddToFavorite([FromQuery] Guid id)
    {
        var command = new AddToFavoriteCommand(userContext.GetUserIdOrThrow(), id);
        var response = await bus.InvokeAsync<CreateEntityResponse<Guid>>(command);

        return Created(response.EntityId.ToString(), response);
    }

    [HttpDelete]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> RemoveFromFavorite([FromQuery] Guid id)
    {
        var command = new RemoveFromFavoriteCommand(userContext.GetUserIdOrThrow(), id);
        await bus.InvokeAsync(command);

        return NoContent();
    }
}
