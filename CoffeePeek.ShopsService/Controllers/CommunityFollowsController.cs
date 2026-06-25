using CoffeePeek.Shared.Auth;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Features.Public.Follows;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.ShopsService.Controllers;

/// <summary>Authenticated community follow actions.</summary>
[ApiController]
[Authorize]
[Route("api/community/follows")]
[Tags("Community")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class CommunityFollowsController(IMessageBus bus, IUserContext userContext) : ControllerBase
{
    /// <summary>Follow another user.</summary>
    [HttpPost("users/{followingUserId:guid}")]
    [ProducesResponseType<Response>(StatusCodes.Status200OK)]
    public async Task<IActionResult> FollowUser(Guid followingUserId, CancellationToken cancellationToken)
    {
        var command = new FollowUserCommand(followingUserId)
        {
            FollowerId = userContext.GetUserIdOrThrow(),
            FollowerUserName = userContext.GetUsernameOrThrow()
        };

        var response = await bus.InvokeAsync<Response>(command, cancellationToken);
        return Ok(response);
    }

    /// <summary>Unfollow a user.</summary>
    [HttpDelete("users/{followingUserId:guid}")]
    [ProducesResponseType<Response>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnfollowUser(Guid followingUserId, CancellationToken cancellationToken)
    {
        var command = new UnfollowUserCommand(followingUserId)
        {
            FollowerId = userContext.GetUserIdOrThrow()
        };

        var response = await bus.InvokeAsync<Response>(command, cancellationToken);
        return Ok(response);
    }

    /// <summary>Returns user IDs the caller is following.</summary>
    [HttpGet("users")]
    [ProducesResponseType<Response<GetFollowingResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFollowing(CancellationToken cancellationToken)
    {
        var query = new GetFollowingQuery { UserId = userContext.GetUserIdOrThrow() };
        var response = await bus.InvokeAsync<Response<GetFollowingResponse>>(query, cancellationToken);
        return Ok(response);
    }

    /// <summary>Follow a city feed.</summary>
    [HttpPost("cities/{cityId:guid}")]
    [ProducesResponseType<Response>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FollowCity(Guid cityId, CancellationToken cancellationToken)
    {
        var command = new FollowCityCommand(cityId) { UserId = userContext.GetUserIdOrThrow() };
        var response = await bus.InvokeAsync<Response>(command, cancellationToken);
        return Ok(response);
    }

    /// <summary>Unfollow a city.</summary>
    [HttpDelete("cities/{cityId:guid}")]
    [ProducesResponseType<Response>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnfollowCity(Guid cityId, CancellationToken cancellationToken)
    {
        var command = new UnfollowCityCommand(cityId) { UserId = userContext.GetUserIdOrThrow() };
        var response = await bus.InvokeAsync<Response>(command, cancellationToken);
        return Ok(response);
    }

    /// <summary>Returns city IDs the caller is following.</summary>
    [HttpGet("cities")]
    [ProducesResponseType<Response<GetFollowedCitiesResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFollowedCities(CancellationToken cancellationToken)
    {
        var query = new GetFollowedCitiesQuery { UserId = userContext.GetUserIdOrThrow() };
        var response = await bus.InvokeAsync<Response<GetFollowedCitiesResponse>>(query, cancellationToken);
        return Ok(response);
    }
}
