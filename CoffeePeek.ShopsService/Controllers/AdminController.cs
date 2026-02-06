using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure.Constants;
using CoffeePeek.Shared.Infrastructure.Persistence;
using CoffeePeek.Shops.Application.Features.Admin.InvalidateCache;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CoffeePeek.ShopsService.Controllers;

[ApiController]
[Route("api/admin/account/cache")]
[Authorize(Policy = RoleConsts.Admin)]
public class AdminController(IMediator mediator) : ControllerBase
{
    [HttpDelete]
    [ProducesResponseType(typeof(Response<InvalidateCacheResponse>), StatusCodes.Status200OK)] 
    [ProducesResponseType(typeof(Response), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Invalidate cache")]
    public async Task<Response<InvalidateCacheResponse>> InvalidateCache(
        [FromQuery] string? category = null,
        [FromQuery] bool all = false)
    {
        return await mediator.Send(new InvalidateCacheCommand(category, all));
    }


    [HttpGet("categories")]
    [ProducesResponseType(typeof(Response<Dictionary<string, string>>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Get cache categories")]
    public IActionResult GetCacheCategories()
    {
        return Ok(Response<Dictionary<string, string>>.Success(CacheKey.Categories.Account.GetDescriptions()));
    }
}
