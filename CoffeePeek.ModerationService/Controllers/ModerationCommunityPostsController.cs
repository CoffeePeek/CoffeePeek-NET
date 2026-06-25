using CoffeePeek.Contract.Dtos.Public;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Moderation.Application.Features.CommunityPost.ChangeStatusModerationCommunityPost;
using CoffeePeek.Moderation.Application.Features.CommunityPost.GetAllModerationCommunityPosts;
using CoffeePeek.Moderation.Application.Features.CommunityPost.GetModerationCommunityPostById;
using CoffeePeek.Moderation.Application.Features.CommunityPost.SendCommunityPostToModeration;
using CoffeePeek.Shared.Auth;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.ModerationService.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class ModerationCommunityPostsController(IMessageBus bus, IUserContext userContext) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = RoleConsts.Moderator)]
    [ProducesResponseType(typeof(Response<GetAllModerationCommunityPostsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetAllModerationCommunityPostsQuery query,
        CancellationToken cancellationToken)
    {
        var response = await bus.InvokeAsync<Response<GetAllModerationCommunityPostsResponse>>(query, cancellationToken);

        if (response.IsSuccess && response.Data is not null)
        {
            Response.Headers.TryAdd("X-Total-Count", response.Data.TotalItems.ToString());
            Response.Headers.TryAdd("X-Total-Pages", response.Data.TotalPages.ToString());
            Response.Headers.TryAdd("X-Current-Page", response.Data.CurrentPage.ToString());
            Response.Headers.TryAdd("X-Page-Size", response.Data.PageSize.ToString());
        }

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = RoleConsts.Moderator)]
    [ProducesResponseType<Response<ModerationCommunityPostDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var response = await bus.InvokeAsync<Response<ModerationCommunityPostDto>>(
            new GetModerationCommunityPostByIdQuery(id), ct);
        return response.IsSuccess ? Ok(response) : NotFound(response);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CreateEntityResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SendToModeration([FromBody] SendCommunityPostToModerationCommand command)
    {
        command = command with
        {
            UserId = userContext.GetUserIdOrThrow(),
            UserName = userContext.GetUsernameOrThrow()
        };

        var response = await bus.InvokeAsync<CreateEntityResponse>(command);
        return Ok(response);
    }

    [HttpPut]
    [Authorize(Policy = RoleConsts.Moderator)]
    [ProducesResponseType<UpdateEntityResponse<ModerationStatus>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangeStatus(ChangeStatusModerationCommunityPostCommand command)
    {
        var commandWithUser = command with { UserId = userContext.GetUserIdOrThrow() };
        var response = await bus.InvokeAsync<UpdateEntityResponse<ModerationStatus>>(commandWithUser);
        return Ok(response);
    }
}
