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
using Swashbuckle.AspNetCore.Annotations;

namespace CoffeePeek.ModerationService.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class ModerationReviewsController(IMediator mediator, IUserContext userContext) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = RoleConsts.Moderator)]
    [ProducesResponseType(typeof(Response<GetAllModerationReviewsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [SwaggerOperation("Get all moderation reviews")]
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
    [SwaggerOperation("Create new moderation review")]
    public async Task<IActionResult> SendReviewToModeration([FromBody] SendReviewToModerationCommand command)
    {
        command = command with
        {
            UserId = userContext.GetUserIdOrThrow(),
            UserName = userContext.GetUsernameOrThrow()
        };

        var response = await mediator.Send(command);

        return Ok(response);
    }

    [HttpPut("{reviewId:guid}")]
    [ProducesResponseType(typeof(Response<UpdateCoffeeShopReviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Update moderation review")]
    public async Task<IActionResult> UpdateReview(
        [FromBody] UpdateCoffeeShopReviewCommand command, Guid reviewId)
    {
        command = command with { UserId = userContext.GetUserIdOrThrow(), ReviewId = reviewId };
        var response = await mediator.Send(command);

        return response.IsSuccess ? Ok(response) : NotFound(response);
    }

    [HttpPut]
    [Authorize(Policy = RoleConsts.Moderator)]
    [ProducesResponseType<UpdateEntityResponse<ModerationStatus>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [SwaggerOperation("Change status of moderation review, when reject status need set reject reason")]
    public async Task<IActionResult> ChangeStatusModerationReview(
        ChangeStatusModerationReviewCommand command)
    {
        var commandWithUser = command with { UserId = userContext.GetUserIdOrThrow() };
        return Ok(await mediator.Send(commandWithUser));
    }
}