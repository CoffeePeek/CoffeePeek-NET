using CoffeePeek.Contract.Abstract;
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
public class CoffeeShopReviewsController(IMediator mediator) : ControllerBase
{
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