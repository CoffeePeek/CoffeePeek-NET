using CoffeePeek.Contract.Enums;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Features.Public.Comments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.ShopsService.Controllers;

/// <summary>Public read access to community comment threads.</summary>
[ApiController]
[Route("api/public/comments")]
[AllowAnonymous]
[Tags("Public")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class PublicCommentsController(IMessageBus bus) : ControllerBase
{
    /// <summary>
    /// Returns paginated top-level comments and one level of replies for a review or check-in.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<Response<GetCommunityCommentsResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComments(
        [FromQuery] CommunityCommentTargetType targetType,
        [FromQuery] Guid targetId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await bus.InvokeAsync<Response<GetCommunityCommentsResponse>>(
            new GetCommunityCommentsQuery(targetType, targetId, page, pageSize),
            cancellationToken);

        return Ok(response);
    }
}
