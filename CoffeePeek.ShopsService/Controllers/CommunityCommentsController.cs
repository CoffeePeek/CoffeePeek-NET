using CoffeePeek.Shared.Auth;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Features.Public.Comments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.ShopsService.Controllers;

/// <summary>Authenticated community comment actions.</summary>
[ApiController]
[Authorize]
[Route("api/community/comments")]
[Tags("Community")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class CommunityCommentsController(IMessageBus bus, IUserContext userContext) : ControllerBase
{
    /// <summary>Creates a comment or a one-level reply on a review, check-in, or post.</summary>
    [HttpPost]
    [ProducesResponseType<Response<CreateCommunityCommentResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateComment(
        [FromBody] CreateCommunityCommentCommand command,
        CancellationToken cancellationToken)
    {
        command = command with
        {
            UserId = userContext.GetUserIdOrThrow(),
            UserName = userContext.GetUsernameOrThrow()
        };

        var response = await bus.InvokeAsync<Response<CreateCommunityCommentResponse>>(command, cancellationToken);
        return Ok(response);
    }

    /// <summary>Deletes the caller's comment. Moderators and admins may delete any comment.</summary>
    [HttpDelete("{commentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComment(Guid commentId, CancellationToken cancellationToken)
    {
        var canModerate = userContext.HasAnyRole(RoleConsts.Moderator, RoleConsts.Admin);
        var command = new DeleteCommunityCommentCommand(
            commentId,
            userContext.GetUserIdOrThrow(),
            canModerate);

        await bus.InvokeAsync<Response>(command, cancellationToken);
        return NoContent();
    }
}
