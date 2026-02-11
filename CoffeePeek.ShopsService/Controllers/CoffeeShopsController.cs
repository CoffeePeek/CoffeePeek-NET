using System.ComponentModel.DataAnnotations;
using CoffeePeek.Shared.Auth;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Common.Responses;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;
using CoffeePeek.Shops.Application.Features.CoffeeShop.SearchCoffeeShops;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.ShopsService.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class CoffeeShopsController(IMessageBus bus, IUserContext userContext) : ControllerBase
{
    /// <summary>
    /// Search coffee shops
    /// </summary>
    /// <param name="cityId"></param>
    /// <param name="q"></param>
    /// <param name="roasters"></param>
    /// <param name="equipments"></param>
    /// <param name="beans"></param>
    /// <param name="brewMethods"></param>
    /// <param name="priceRange"></param>
    /// <param name="minRating"></param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Response<GetCoffeeShopsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
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

        var response = await bus.InvokeAsync<Response<GetCoffeeShopsResponse>>(query);

        if (response is { IsSuccess: true, Data: not null })
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
    /// Get coffee shop by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Response<GetCoffeeShopResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCoffeeShop(Guid id)
    {
        var query = new GetCoffeeShopQuery(id);
        var response = await bus.InvokeAsync<Response<GetCoffeeShopResponse>>(query);
        
        return Ok(response);
    }
}