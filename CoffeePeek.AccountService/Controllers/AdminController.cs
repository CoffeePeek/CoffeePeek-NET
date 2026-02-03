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
public class AdminController(IMediator mediator, IUserContext userContext) : ControllerBase
{
    /// <summary>
    /// Изменяет роль указанного пользователя.
    /// </summary>
    /// <param name="userIdOfChange">Идентификатор пользователя, роль которого нужно изменить.</param>
    /// <param name="roleId">Идентификатор новой роли, присваиваемой пользователю.</param>
    /// <returns>`Response` с результатом операции: `Response` указывает на успех при успешной смене роли или содержит информацию об ошибке в противном случае.</returns>
    [HttpPut("role")]
    [SwaggerOperation(Summary = "Change role of user")]
    public Task<Response> ChangeRole([FromQuery] Guid userIdOfChange, [FromQuery] Guid roleId)
    {
        return mediator.Send(new ChangeRoleCommand(userContext.GetUserIdOrThrow(), userIdOfChange, roleId));
    }

    /// <summary>
    /// Инициирует инвалидирование кэша для указанной категории или для всех категорий.
    /// </summary>
    /// <param name="category">Имя категории кэша для инвалидирования; <c>null</c> означает отсутствие фильтра по категории.</param>
    /// <param name="all">Если <c>true</c>, инвалидируются все категории кэша.</param>
    /// <returns>Ответ, содержащий результат операции инвалидирования кэша.</returns>
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