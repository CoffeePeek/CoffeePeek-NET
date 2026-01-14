using System.ComponentModel;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Moderation.Application.Features.Review.ChangeStatusModerationReview;
using Coffeepeek.Moderation.Application.Features.Review.GetAllModerationReviews;
using CoffeePeek.Moderation.Application.Features.Review.GetAllModerationReviews;
using CoffeePeek.Moderation.Application.Features.Review.SendReviewToModeration;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shared.Infrastructure.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.ModerationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModerationReviewController(IMediator mediator) : Controller
{
    [HttpGet]
    [Authorize(Policy = RoleConsts.Moderator)]
    [ProducesResponseType(typeof(Response<GetAllModerationReviewsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [Description("Get all moderation reviews")]
    public async Task<Response<GetAllModerationReviewsResponse>> GetAllModerationReviews(
        GetAllModerationReviewsQuery query)
    {
        return await mediator.Send(query);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CreateEntityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [Description("Create new moderation review")]
    public async Task<CreateEntityResponse> SendReviewToModeration(SendReviewToModerationCommand command)
    {
        var commandWithUser = command with { UserId = User.GetUserIdOrThrow() };
        return await mediator.Send(commandWithUser);
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