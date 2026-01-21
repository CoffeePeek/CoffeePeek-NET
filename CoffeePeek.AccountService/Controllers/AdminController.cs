using CoffeePeek.Account.Application.Features.Admin.ChangeRole;
using CoffeePeek.Account.Application.Features.Admin.InvalidateCache;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.AccountService.Controllers;

[ApiController]
[Route("api/admin/account")]
[Authorize(Policy = RoleConsts.Admin)]
public class AdminController(IMediator mediator) : ControllerBase
{
    [HttpPut("role")]
    public Task<Response> ChangeRole([FromQuery] Guid userIdOfChange, [FromQuery]Guid roleId)
    {
        return mediator.Send(new ChangeRoleCommand(User.GetUserIdOrThrow(), userIdOfChange, roleId));
    }
    
    /// <summary>
    /// Invalidates cache by category or all cache
    /// </summary>
    /// <param name="category">Cache category: users, auth</param>
    /// <param name="all">Set to true to invalidate all cache</param>
    /// <returns>Result of cache invalidation</returns>
    [HttpDelete("cache")]
    [ProducesResponseType(typeof(Response<InvalidateCacheResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<InvalidateCacheResponse>), StatusCodes.Status400BadRequest)]
    public async Task<Response<InvalidateCacheResponse>> InvalidateCache(
        [FromQuery] string? category = null,
        [FromQuery] bool all = false)
    {
        return await mediator.Send(new InvalidateCacheCommand(category, all));
    }

    /// <summary>
    /// Get available cache categories
    /// </summary>
    /// <returns>List of available cache categories with descriptions</returns>
    [HttpGet("cache/categories")]
    [ProducesResponseType(typeof(Response<Dictionary<string, string>>), StatusCodes.Status200OK)]
    public IActionResult GetCacheCategories()
    {
        return Ok(Response<Dictionary<string, string>>.Success(CacheKey.Categories.Account.GetDescriptions()));
    }
}
