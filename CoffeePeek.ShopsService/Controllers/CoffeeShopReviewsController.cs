using CoffeePeek.Shared.Auth;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Features.Review.CanCreateCoffeeShopReview;
using CoffeePeek.Shops.Application.Features.Review.DeleteReviewFromCoffeeShop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.ShopsService.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class CoffeeShopReviewsController(IMessageBus bus, IUserContext userContext) : ControllerBase
{
    /// <summary>
    /// Check if user can create review for coffee shop
    /// </summary>
    /// <param name="shopId"></param>
    /// <returns></returns>
    [HttpGet("can-create")]
    [ProducesResponseType(typeof(Response<CanCreateCoffeeShopReviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CanCreateReview([FromQuery] Guid shopId)
    {
        if (shopId == Guid.Empty)
            return BadRequest(Response<CanCreateCoffeeShopReviewResponse>.Error("Invalid shop ID"));

        var userId = userContext.GetUserIdOrThrow();
        var query = new CanCreateCoffeeShopReviewQuery(userId, shopId);
        var response = await bus.InvokeAsync<Response<CanCreateCoffeeShopReviewResponse>>(query);

        return Ok(response);
    }

    /// <summary>
    /// Delete review by ID
    /// </summary>
    /// <param name="shopId"></param>
    /// <param name="reviewId"></param>
    /// <returns></returns>
    [HttpDelete("{reviewId:guid}")]
    [ProducesResponseType(typeof(Response), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteReview(Guid shopId, Guid reviewId)
    {
        var command = new DeleteReviewFromCoffeeShopCommand(reviewId);
        var response = await bus.InvokeAsync<Response>(command);

        return response.IsSuccess ? NoContent() : NotFound(response);
    }
}