using System.ComponentModel;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Moderation.Application.Features.Review.ChangeStatusModerationReview;
using CoffeePeek.Moderation.Application.Features.Review.GetAllModerationReviews;
using CoffeePeek.Moderation.Application.Features.Review.SendReviewToModeration;
using CoffeePeek.Moderation.Application.Features.Review.UpdateCoffeeShopReview;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shared.Infrastructure.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.ModerationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModerationReviewsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = RoleConsts.Moderator)]
    [ProducesResponseType(typeof(Response<GetAllModerationReviewsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [Description("Get all moderation reviews")]
    public async Task<IActionResult> GetAllModerationReviews()
    {
        return Ok(await mediator.Send(new GetAllModerationReviewsQuery()));
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CreateEntityResponse), statusCode: StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [Description("Create new moderation review")]
    public async Task<IActionResult> SendReviewToModeration([FromBody] SendReviewToModerationCommand command)
    {
        var commandWithUser = command with
        {
            UserId = User.GetUserIdOrThrow(), 
            UserName = User.GetUsernameOrThrow()
        };
        return Ok(await mediator.Send(commandWithUser));
    }
    
    [HttpPut("{reviewId:guid}")]
    [ProducesResponseType(typeof(Response<UpdateCoffeeShopReviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReview(
        [FromBody] UpdateCoffeeShopReviewRequest request, Guid reviewId)
    {
        request = request with { UserId = User.GetUserIdOrThrow(), ReviewId = reviewId };
        var response = await mediator.Send(request);

        return response.IsSuccess ? Ok(response) : NotFound(response);
    }

    [HttpPut]
    [Authorize(Policy =  RoleConsts.Moderator)]
    [ProducesResponseType(typeof(Response<GetAllModerationReviewsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [Description("Change status of moderation review, when reject status need set reject reason")]
    public async Task<UpdateEntityResponse<ModerationStatus>> ChangeStatusModerationReview(
        ChangeStatusModerationReviewCommand command)
    {
        var commandWithUser = command with { UserId = User.GetUserIdOrThrow() };
        return await mediator.Send(commandWithUser);
    }
}