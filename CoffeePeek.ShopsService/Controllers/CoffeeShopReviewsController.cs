using CoffeePeek.Shared.Auth;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Features.Review.DeleteReviewFromCoffeeShop;
using CoffeePeek.Shops.Application.Features.Review.GetReviewById;
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
    /// Get review by ID
    /// </summary>
    [HttpGet("{reviewId:guid}")]
    [ProducesResponseType(typeof(Response<GetReviewByIdResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetReviewById(Guid reviewId)
    {
        var response = await bus.InvokeAsync<Response<GetReviewByIdResponse>>(
            new GetReviewByIdQuery(reviewId));

        return response.IsSuccess ? Ok(response) : NotFound(response);
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
        var command = new DeleteReviewFromCoffeeShopCommand(reviewId, userContext.GetUserIdOrThrow());
        var response = await bus.InvokeAsync<Response>(command);

        return response.IsSuccess ? NoContent() : NotFound(response);
    }
}