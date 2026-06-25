using CoffeePeek.Contract.Enums;
using CoffeePeek.Shared.Auth;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Features.Public.Feed;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.ShopsService.Controllers;

/// <summary>Public community feed of reviews, check-ins, and posts.</summary>
[ApiController]
[Route("api/public/feed")]
[AllowAnonymous]
[Tags("Public")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class PublicFeedController(IMessageBus bus, IUserContext userContext) : ControllerBase
{
    /// <summary>
    /// Returns a paginated community feed combining published reviews, check-ins, and posts,
    /// ordered by most recent activity first. Optional auth enriches items with the viewer's reactions.
    /// </summary>
    [HttpGet]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = ["page", "pageSize", "filter", "cityId"])]
    [ProducesResponseType<Response<GetCommunityFeedResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFeed(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] CommunityFeedFilter filter = CommunityFeedFilter.All,
        [FromQuery] Guid? cityId = null,
        CancellationToken cancellationToken = default)
    {
        if (filter == CommunityFeedFilter.Following && !userContext.IsAuthenticated)
            return Unauthorized();

        var query = new GetCommunityFeedQuery(page, pageSize, filter, cityId)
        {
            ViewerUserId = userContext.GetUserId()
        };

        var response = await bus.InvokeAsync<Response<GetCommunityFeedResponse>>(query, cancellationToken);
        return Ok(response);
    }
}
