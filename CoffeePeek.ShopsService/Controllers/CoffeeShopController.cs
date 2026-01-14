using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shops.Application.Common.Responses;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;
using CoffeePeek.Shops.Application.Features.CoffeeShop.SearchCoffeeShops;
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
            UserId: User.GetUserId(),
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

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Response<GetCoffeeShopResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public Task<Response<GetCoffeeShopResponse>> GetCoffeeShop(Guid id)
    {
        return mediator.Send(new GetCoffeeShopQuery(id));
    }
}