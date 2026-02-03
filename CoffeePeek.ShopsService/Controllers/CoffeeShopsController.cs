using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shops.Application.Common.Responses;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;
using CoffeePeek.Shops.Application.Features.CoffeeShop.SearchCoffeeShops;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CoffeePeek.ShopsService.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class CoffeeShopsController(IMediator mediator, IUserContext userContext) : ControllerBase
{
    /// <summary>
    /// Ищет кофейни по заданным фильтрам и параметрам пагинации; при успешном результате добавляет заголовки, описывающие пагинацию.
    /// </summary>
    /// <param name="cityId">Идентификатор города для фильтрации результатов (необязательно).</param>
    /// <param name="q">Строка поиска по названию или описанию кофейн (необязательно).</param>
    /// <param name="roasters">Список идентификаторов ростеров для фильтрации (необязательно).</param>
    /// <param name="equipments">Список идентификаторов оборудования для фильтрации (необязательно).</param>
    /// <param name="beans">Список идентификаторов сортов кофе для фильтрации (необязательно).</param>
    /// <param name="brewMethods">Список идентификаторов методов заваривания для фильтрации (необязательно).</param>
    /// <param name="priceRange">Диапазон цен для фильтрации (необязательно).</param>
    /// <param name="minRating">Минимальный рейтинг кафе; значение в диапазоне 0–5 (необязательно).</param>
    /// <param name="page">Номер страницы результатов, минимум 1.</param>
    /// <param name="pageSize">Количество элементов на страницу, от 1 до 100.</param>
    /// <returns>200 OK с объектом Response&lt;GetCoffeeShopsResponse&gt; при успешном выполнении; в зависимости от ситуации может вернуть 400, 404, 500 или 503.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Response<GetCoffeeShopsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [SwaggerOperation(Summary = "Search coffee shops", Description = "Search coffee shops by criteria")]
    public async Task<IActionResult> GetCoffeeShops(
        [FromQuery] Guid? cityId = null,
        [FromQuery] string? q = null,
        [FromQuery] Guid[]? roasters = null,
        [FromQuery] Guid[]? equipments = null,
        [FromQuery] Guid[]? beans = null,
        [FromQuery] Guid[]? brewMethods = null,
        [FromQuery] Contract.Enums.PriceRange? priceRange = null,
        [FromQuery][Range(0, 5)] decimal? minRating = null,
        [FromQuery][Range(1, int.MaxValue)] int page = 1,
        [FromQuery][Range(1, 100)] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

        var query = new SearchCoffeeShopsQuery(
            UserId: userContext.GetUserId(),
            Query: q,
            CityId: cityId,
            Roasters: roasters,
            Equipments: equipments,
            Beans: beans,
            BrewMethods: brewMethods,
            PriceRange: priceRange,
            MinRating: minRating,
            PageNumber: page,
            PageSize: pageSize);

        var response = await mediator.Send(query);

        if (response.IsSuccess && response.Data != null)
        {
            AddPaginationHeaders(response.Data);
        }

        return Ok(response);

        void AddPaginationHeaders(GetCoffeeShopsResponse data)
        {
            Response.Headers.TryAdd("X-Total-Count", data.TotalItems.ToString());
            Response.Headers.TryAdd("X-Total-Pages", data.TotalPages.ToString());
            Response.Headers.TryAdd("X-Current-Page", data.CurrentPage.ToString());
            Response.Headers.TryAdd("X-Page-Size", data.PageSize.ToString());
        }
    }

    /// <summary>
    /// Возвращает информацию о кофейне по её идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор кофейни (GUID).</param>
    /// <returns>`Response<GetCoffeeShopResponse>` с данными кофейни при успешном запросе, в противном случае ответ с соответствующей ошибкой.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Response<GetCoffeeShopResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [SwaggerOperation(Summary = "Get coffee shop by ID")]
    public Task<Response<GetCoffeeShopResponse>> GetCoffeeShop(Guid id)
    {
        return mediator.Send(new GetCoffeeShopQuery(id, userContext.GetUserId()));
    }
}