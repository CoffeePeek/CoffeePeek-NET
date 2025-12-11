using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Constants;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.ShopsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoffeeShopController(IMediator mediator) : Controller
{
    [HttpGet]
    [ProducesResponseType(typeof(Response<GetCoffeeShopsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<Response<GetCoffeeShopsResponse>> GetCoffeeShops(
        [FromQuery] Guid? cityId,
        [FromHeader(Name = "X-Page-Number")] int pageNumber = 1,
        [FromHeader(Name = "X-Page-Size")] int pageSize = 10)
    {
        cityId ??= BusinessConstants.DefaultUnAuthorizedCityId;
        
        var response = await mediator.Send(new GetCoffeeShopsCommand(cityId.Value, pageNumber, pageSize));

        AddPaginationHeaders(response.Data);

        return response;

        void AddPaginationHeaders(GetCoffeeShopsResponse data)
        {
            Response.Headers.TryAdd("X-Total-Count", data.TotalItems.ToString());
            Response.Headers.TryAdd("X-Total-Pages", data.TotalPages.ToString());
            Response.Headers.TryAdd("X-Current-Page", data.CurrentPage.ToString());
            Response.Headers.TryAdd("X-Page-Size", data.PageSize.ToString());
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Response<GetCoffeeShopResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public Task<Response<GetCoffeeShopResponse>> GetCoffeeShop(Guid id)
    {
        return mediator.Send(new GetCoffeeShopCommand(id));
    }
    
    [HttpPost("/favorite{id:guid}")]
    [ProducesResponseType(typeof(Response<GetCoffeeShopResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public Task<CreateEntityResponse<Guid>> AddToFavorite([FromBody]Guid id, Guid userId)
    {
        return mediator.Send(new AddToFavoriteCommand(id, userId));
    }
    
    [HttpDelete("/favorite{id:guid}")]
    [ProducesResponseType(typeof(Response<GetCoffeeShopResponse>), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public Task<UpdateEntityResponse<Guid>> RemoveFromFavorite([FromBody]Guid id, Guid userId)
    {
        return mediator.Send(new RemoveFromFavoriteCommand(id, userId));
    }

    /// <summary>
    /// Выполняет поиск кофеен по заданным фильтрам и возвращает постраничный набор результатов.
    /// </summary>
    /// <param name="q">Строка поиска (по имени или описанию).</param>
    /// <param name="cityId">Опциональный фильтр по идентификатору города.</param>
    /// <param name="equipments">Опциональный массив идентификаторов оборудования для фильтрации результатов.</param>
    /// <param name="beans">Опциональный массив идентификаторов сортов кофе для фильтрации результатов.</param>
    /// <param name="minRating">Опциональный фильтр по минимальному рейтингу в диапазоне 0–5.</param>
    /// <param name="pageNumber">Номер страницы, берётся из заголовка "X-Page-Number"; нормализуется до значения как минимум 1.</param>
    /// <param name="pageSize">Размер страницы, берётся из заголовка "X-Page-Size"; нормализуется в диапазоне 1–100, по умолчанию 10.</param>
    /// <returns>Response, содержащий GetCoffeeShopsResponse с элементами результата и метаданными пагинации.</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(Response<GetCoffeeShopsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<Response<GetCoffeeShopsResponse>> SearchCoffeeShops(
        [FromQuery] string? q = null,
        [FromQuery] Guid? cityId = null,
        [FromQuery] Guid[]? equipments = null,
        [FromQuery] Guid[]? beans = null,
        [FromQuery][Range(0, 5)] decimal? minRating = null,
        [FromHeader(Name = "X-Page-Number")][Range(1, int.MaxValue)] int pageNumber = 1,
        [FromHeader(Name = "X-Page-Size")][Range(1, 100)] int pageSize = 10)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

        var command = new SearchCoffeeShopsCommand(
            Query: q,
            CityId: cityId,
            Equipments: equipments,
            Beans: beans,
            MinRating: minRating,
            PageNumber: pageNumber,
            PageSize: pageSize);

        var response = await mediator.Send(command);

        if (response.IsSuccess && response.Data != null)
        {
            AddPaginationHeaders(response.Data);
        }

        return response;

        void AddPaginationHeaders(GetCoffeeShopsResponse data)
        {
            Response.Headers.TryAdd("X-Total-Count", data.TotalItems.ToString());
            Response.Headers.TryAdd("X-Total-Pages", data.TotalPages.ToString());
            Response.Headers.TryAdd("X-Current-Page", data.CurrentPage.ToString());
            Response.Headers.TryAdd("X-Page-Size", data.PageSize.ToString());
        }
    }

    /// <summary>
    /// Возвращает кофейни, попадающие в заданный прямоугольный географический диапазон.
    /// </summary>
    /// <param name="minLat">Минимальная (южная) граница широты в градусах.</param>
    /// <param name="minLon">Минимальная (западная) граница долготы в градусах.</param>
    /// <param name="maxLat">Максимальная (северная) граница широты в градусах.</param>
    /// <param name="maxLon">Максимальная (восточная) граница долготы в градусах.</param>
    /// <returns>Response с данными GetShopsInBoundsResponse, содержащими список кофеен, попадающих в указанные границы.</returns>
    [HttpGet("map")]
    [ProducesResponseType(typeof(Response<GetShopsInBoundsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<Response<GetShopsInBoundsResponse>> GetShopsInBounds(
        [FromQuery] decimal minLat,
        [FromQuery] decimal minLon,
        [FromQuery] decimal maxLat,
        [FromQuery] decimal maxLon)
    {
        var request = new GetShopsInBoundsRequest(minLat, minLon, maxLat, maxLon);
        return mediator.Send(request);
    }
}