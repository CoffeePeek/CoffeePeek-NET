using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Constants;
using CoffeePeek.Shops.Application.Features.Admin.InvalidateCache;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.ShopsService.Controllers;

[ApiController]
[Route("api/admin/shops/cache")]
[Authorize(Policy = RoleConsts.Admin)]
public class AdminController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Invalidates cache by category or all cache
    /// </summary>
    /// <param name="category">Cache category: dictionaries, shops-lists, shops-details</param>
    /// <param name="all">Set to true to invalidate all cache</param>
    /// <returns>Result of cache invalidation</returns>
    [HttpDelete("")]
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
    [HttpGet("categories")]
    [ProducesResponseType(typeof(Response<Dictionary<string, string>>), StatusCodes.Status200OK)]
    public IActionResult GetCacheCategories()
    {
        return Ok(Response<Dictionary<string, string>>.Success(CacheKey.Categories.Shops.GetDescriptions()));
    }
}
