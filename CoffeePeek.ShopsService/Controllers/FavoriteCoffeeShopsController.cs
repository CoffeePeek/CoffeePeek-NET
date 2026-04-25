using CoffeePeek.Shared.Auth;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Features.Favorite.AddToFavorite;
using CoffeePeek.Shops.Application.Features.Favorite.GetAllFavorites;
using CoffeePeek.Shops.Application.Features.Favorite.RemoveFromFavorite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.ShopsService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("User Favorites")]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class FavoriteCoffeeShopsController(IMessageBus bus, IUserContext userContext) : ControllerBase
{
    /// <summary>
    /// Get all favorite coffee shops for the current user
    /// </summary>
    /// <remarks>
    /// This endpoint retrieves a complete list of coffee shops that the authenticated user has marked as favorites.
    /// 
    /// **Authentication:**
    /// - Requires a valid session (managed by Gateway). 
    /// - Expects `X-User-Id` header to be present (automatically injected by the Gateway).
    /// 
    /// **Response:**
    /// - Returns an array of detailed coffee shop objects.
    /// </remarks>
    /// <returns>A list of favorite coffee shops</returns>
    /// <response code="200">Successfully retrieved the list of favorites</response>
    /// <response code="401">User is not authenticated or X-User-Id header is missing</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(Response<GetAllFavoritesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllFavorites()
    {
        var command = new GetAllFavoritesCommand(userContext.GetUserIdOrThrow());
        var response = await bus.InvokeAsync<Response<GetAllFavoritesResponse>>(command);
        return Ok(response);
    }
    
    /// <summary>
    /// Add a coffee shop to your personal favorites
    /// </summary>
    /// <remarks>
    /// **Behavior:**
    /// - Requires an authenticated user.
    /// - Returns the ID of the favorite record created.
    /// - Throws 409 Conflict if the shop is already in favorites.
    /// </remarks>
    /// <param name="id">The GUID of the coffee shop</param>
    /// <response code="201">Successfully added to favorites</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="409">Coffee shop is already in favorites</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CreateEntityResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddToFavorite([FromQuery] Guid id)
    {
        var command = new AddToFavoriteCommand(userContext.GetUserIdOrThrow(), id);
        var response = await bus.InvokeAsync<CreateEntityResponse<Guid>>(command);

        return CreatedAtAction(nameof(AddToFavorite), new { id = response.EntityId }, response);
    }
    
    /// <summary>
    /// Remove a coffee shop from your favorites
    /// </summary>
    /// <remarks>
    /// **Behavior:**
    /// - If the shop was not in favorites, it may still return 204 or 404 depending on your handler logic.
    /// </remarks>
    /// <param name="id">The GUID of the coffee shop to remove</param>
    /// <response code="204">Successfully removed (or was not there)</response>
    /// <response code="401">User is not authenticated</response>
    [HttpDelete]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromFavorite([FromQuery] Guid id)
    {
        var command = new RemoveFromFavoriteCommand(userContext.GetUserIdOrThrow(), id);
        await bus.InvokeAsync(command);

        return NoContent();
    }
}