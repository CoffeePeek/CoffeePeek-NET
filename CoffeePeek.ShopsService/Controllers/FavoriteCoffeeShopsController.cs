using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shops.Application.Features.Favorite.AddToFavorite;
using CoffeePeek.Shops.Application.Features.Favorite.RemoveFromFavorite;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.ShopsService.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class FavoriteCoffeeShopsController(IMediator mediator, IUserContext userContext) : ControllerBase
{
    /// <summary>
    /// Добавляет кофейню в список избранного текущего пользователя по её идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор кофейни, которую нужно добавить в избранное.</param>
    /// <returns>Ответ 201 Created с телом CreateEntityResponse&lt;Guid&gt;, содержащим идентификатор созданной записи избранного.</returns>
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
        var response = await mediator.Send(command);

        return Created(response.EntityId.ToString(), response);
    }

    /// <summary>
    /// Удаляет кофейню из списка избранного текущего пользователя.
    /// </summary>
    /// <param name="id">Идентификатор кофейни, которую нужно удалить из избранного.</param>
    /// <returns>Ответ HTTP с кодом 204 No Content при успешном удалении.</returns>
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
        await mediator.Send(command);

        return NoContent();
    }
}