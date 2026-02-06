using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shops.Application.Features.Review.CanCreateCoffeeShopReview;
using CoffeePeek.Shops.Application.Features.Review.DeleteReviewFromCoffeeShop;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CoffeePeek.ShopsService.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class CoffeeShopReviewsController(IMediator mediator, IUserContext userContext) : ControllerBase
{
    [HttpGet("can-create")]
    [ProducesResponseType(typeof(Response<CanCreateCoffeeShopReviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Check if user can create review for coffee shop")]
    public async Task<IActionResult> CanCreateReview([FromQuery] Guid shopId)
    {
        if (shopId == Guid.Empty)
            return BadRequest(Response<CanCreateCoffeeShopReviewResponse>.Error("Invalid shop ID"));

        var userId = userContext.GetUserIdOrThrow();
        var query = new CanCreateCoffeeShopReviewQuery(userId, shopId);
        var response = await mediator.Send(query);

        return Ok(response);
    }

    [HttpDelete("{reviewId:guid}")]
    [ProducesResponseType(typeof(Response), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Delete review by ID")]
    public async Task<IActionResult> DeleteReview(Guid shopId, Guid reviewId)
    {
        var command = new DeleteReviewFromCoffeeShopCommand(reviewId);
        var response = await mediator.Send(command);

        return response.IsSuccess ? NoContent() : NotFound(response);
    }
}