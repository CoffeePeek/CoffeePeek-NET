using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shops.Application.Features.Review.GetReviewsByUserId;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IMediator = MediatR.IMediator;

namespace CoffeePeek.ShopsService.Controllers;

[ApiController]
[Authorize]
[Route("api/users/{userId:guid}/reviews")]
public class UserReviewsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(Response<GetReviewsByUserIdResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserReviews(
        Guid userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery][Range(1, 100)] int pageSize = 10)
    {
        if (userId == Guid.Empty)
            return BadRequest(Response<GetReviewsByUserIdResponse>.Error("Invalid user ID"));

        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = new GetReviewsByUserIdQuery(userId, pageNumber, pageSize);
        var response = await mediator.Send(query);

        if (response is { IsSuccess: true, Data: not null })
        {
            Response.Headers["X-Total-Count"] = response.Data.TotalItems.ToString();
            Response.Headers["X-Total-Pages"] = response.Data.TotalPages.ToString();
            Response.Headers["X-Current-Page"] = response.Data.CurrentPage.ToString();
            Response.Headers["X-Page-Size"] = response.Data.PageSize.ToString();
        }

        return Ok(response);
    }
}