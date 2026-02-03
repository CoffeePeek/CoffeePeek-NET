using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using CoffeePeek.JobVacancies.Application.Features.Admin.InvalidateCache;
using CoffeePeek.Shared.Infrastructure.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.JobVacancies.Controllers;

[ApiController]
[Route("api/admin/vacancies")]
[Authorize(Policy = RoleConsts.Admin)]
public class AdminController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Invalidates all vacancies cache
    /// </summary>
    /// <returns>Result of cache invalidation</returns>
    [HttpDelete("cache")]
    [ProducesResponseType(typeof(Response<InvalidateCacheResponse>), StatusCodes.Status200OK)]
    public async Task<Response<InvalidateCacheResponse>> InvalidateCache()
    {
        return await mediator.Send(new InvalidateCacheCommand(InvalidateAll: true));
    }

    /// <summary>
    /// Get cache information
    /// </summary>
    /// <returns>Cache description</returns>
    [HttpGet("cache/info")]
    [ProducesResponseType(typeof(Response<string>), StatusCodes.Status200OK)]
    public IActionResult GetCacheInfo()
    {
        const string info = "Vacancies cache stores job vacancy listings. All cache is invalidated when vacancies are synced.";
        return Ok(Response<string>.Success(info));
    }
}
