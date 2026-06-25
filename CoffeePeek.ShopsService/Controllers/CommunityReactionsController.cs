using CoffeePeek.Shared.Auth;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Features.Public.Reactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.ShopsService.Controllers;

/// <summary>Authenticated community reaction actions.</summary>
[ApiController]
[Authorize]
[Route("api/community/reactions")]
[Tags("Community")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class CommunityReactionsController(IMessageBus bus, IUserContext userContext) : ControllerBase
{
    /// <summary>Sets, changes, or removes the caller's reaction on a feed item.</summary>
    [HttpPut]
    [ProducesResponseType<Response<SetCommunityReactionResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetReaction(
        [FromBody] SetCommunityReactionCommand command,
        CancellationToken cancellationToken)
    {
        command = command with
        {
            UserId = userContext.GetUserIdOrThrow(),
            UserName = userContext.GetUsernameOrThrow()
        };

        var response = await bus.InvokeAsync<Response<SetCommunityReactionResponse>>(command, cancellationToken);
        return Ok(response);
    }
}
