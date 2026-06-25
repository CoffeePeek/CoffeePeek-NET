using CoffeePeek.Contract.Enums;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Features.Public.Feed;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.ShopsService.Controllers;

/// <summary>Public community feed of reviews and check-ins.</summary>
[ApiController]
[Route("api/public/feed")]
[AllowAnonymous]
[Tags("Public")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class PublicFeedController(IMessageBus bus) : ControllerBase
{
    /// <summary>
    /// Returns a paginated community feed combining published reviews and public check-ins,
    /// ordered by most recent activity first.
    /// </summary>
    [HttpGet]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    [ProducesResponseType<Response<GetCommunityFeedResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeed(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] CommunityFeedFilter filter = CommunityFeedFilter.All,
        CancellationToken cancellationToken = default)
    {
        var response = await bus.InvokeAsync<Response<GetCommunityFeedResponse>>(
            new GetCommunityFeedQuery(page, pageSize, filter),
            cancellationToken);

        return Ok(response);
    }
}
