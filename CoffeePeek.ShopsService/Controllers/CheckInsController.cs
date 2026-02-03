using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shops.Application.Features.CheckIn.CreateCheckIn;
using CoffeePeek.Shops.Application.Features.CheckIn.GetUserCheckIns;
using CoffeePeek.Shops.Application.Features.CoffeeShop.CheckIn.GetUserCheckIns;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CoffeePeek.ShopsService.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class CheckInsController(IMediator mediator, IUserContext userContext) : ControllerBase
{
    /// <summary>
    /// Создаёт запись о чекине в кофейне, используя данные из переданной команды и текущего пользователя.
    /// </summary>
    /// <param name="command">Команда с данными для создания чекина. Поля UserId и UserName будут перезаписаны значениями из текущего пользовательского контекста.</param>
    /// <returns>Response с данными CreateCheckInResponse, содержащий результат операции создания чекина.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Response<CreateCheckInResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Create check-in for coffee shop")]
    public Task<Response<CreateCheckInResponse>> CreateCheckIn([FromBody] CreateCheckInCommand command)
    {
        command = command with { UserId = userContext.GetUserIdOrThrow(), UserName = userContext.GetUsernameOrThrow() };
        return mediator.Send(command);
    }

    /// <summary>
    /// Возвращает список чек‑инов текущего пользователя с поддержкой постраничной навигации.
    /// </summary>
    /// <param name="pageNumber">Номер страницы — берётся из заголовка "X-Page-Number"; нормализуется до значения не меньше 1.</param>
    /// <param name="pageSize">Размер страницы — берётся из заголовка "X-Page-Size"; если значение &lt;= 0 устанавливается 10, максимальное значение 100.</param>
    /// <returns>HTTP 200 с объектом Response&lt;GetUserCheckInsResponse&gt;, содержащим чек‑ины текущего пользователя; при успешном результате в ответ добавляются заголовки пагинации.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Response<GetUserCheckInsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Get check-ins for current user")]
    public async Task<IActionResult> GetMyCheckIns(
        [FromHeader(Name = "X-Page-Number")] int pageNumber = 1,
        [FromHeader(Name = "X-Page-Size")] int pageSize = 10)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

        var userId = userContext.GetUserIdOrThrow();
        var response = await mediator.Send(new GetUserCheckInsCommand(userId, pageNumber, pageSize));

        if (response is { IsSuccess: true, Data: not null })
        {
            AddPaginationHeaders(response.Data.TotalItems, response.Data.TotalPages, response.Data.CurrentPage, response.Data.PageSize);
        }

        return Ok(response);
    }

    private void AddPaginationHeaders(int totalItems, int totalPages, int currentPage, int pageSize)
    {
        Response.Headers.TryAdd("X-Total-Count", totalItems.ToString());
        Response.Headers.TryAdd("X-Total-Pages", totalPages.ToString());
        Response.Headers.TryAdd("X-Current-Page", currentPage.ToString());
        Response.Headers.TryAdd("X-Page-Size", pageSize.ToString());
    }
}