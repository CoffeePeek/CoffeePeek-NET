using CoffeePeek.Account.Application.Features.Admin.ChangeRole;
using CoffeePeek.Account.Application.Features.Admin.InvalidateCache;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CoffeePeek.AccountService.Controllers;

[ApiController]
[Route("api/admin/account")]
[Authorize(Policy = RoleConsts.Admin)]
public class AdminController(IMediator mediator) : ControllerBase
{
    [HttpPut("role")]
    [SwaggerOperation(Summary = "Change role of user")]
    public Task<Response> ChangeRole([FromQuery] Guid userIdOfChange, [FromQuery] Guid roleId)
    {
        return mediator.Send(new ChangeRoleCommand(User.GetUserIdOrThrow(), userIdOfChange, roleId));
    }

    [HttpDelete("cache")]
    [ProducesResponseType(typeof(Response<InvalidateCacheResponse>), StatusCodes.Status200OK)] 
    [ProducesResponseType(typeof(Response), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Invalidate cache")]
    public async Task<Response<InvalidateCacheResponse>> InvalidateCache(
        [FromQuery] string? category = null,
        [FromQuery] bool all = false)
    {
        return await mediator.Send(new InvalidateCacheCommand(category, all));
    }


    [HttpGet("cache/categories")]
    [ProducesResponseType(typeof(Response<Dictionary<string, string>>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Get cache categories")]
    public IActionResult GetCacheCategories()
    {
        return Ok(Response<Dictionary<string, string>>.Success(CacheKey.Categories.Account.GetDescriptions()));
    }
}