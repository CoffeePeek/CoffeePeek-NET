using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shops.Application;
using CoffeePeek.Shops.Application.Common.Responses;
using CoffeePeek.Shops.Application.Features.CoffeeShop;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShops;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetShopsInBounds;
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
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<Response<GetCoffeeShopsResponse>> GetCoffeeShops(
        [FromQuery, Required] Guid cityId,
        [FromQuery, Required] int page,
        [FromQuery, Required] int pageSize)
    {
        var command = new GetCoffeeShopsQuery(User.GetUserId(), cityId, page, pageSize);
        var response = await mediator.Send(command);

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
        return mediator.Send(new GetCoffeeShopQuery(id));
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(Response<GetCoffeeShopsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<Response<GetCoffeeShopsResponse>> SearchCoffeeShops(
        [FromQuery] string? q = null,
        [FromQuery] Guid? cityId = null,
        [FromQuery] Guid[]? roasters = null,
        [FromQuery] Guid[]? equipments = null,
        [FromQuery] Guid[]? beans = null,
        [FromQuery] Guid[]? brewMethods = null,
        [FromQuery] Contract.Enums.PriceRange? priceRange = null,
        [FromQuery][Range(0, 5)] decimal? minRating = null,
        [FromHeader(Name = "X-Page-Number")][Range(1, int.MaxValue)] int pageNumber = 1,
        [FromHeader(Name = "X-Page-Size")][Range(1, 100)] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

        var command = new SearchCoffeeShopsQuery(
            UserId: User.GetUserId(),
            Query: q,
            CityId: cityId,
            Roasters: roasters,
            Equipments: equipments,
            Beans: beans,
            BrewMethods: brewMethods,
            PriceRange: priceRange,
            MinRating: minRating,
            PageNumber: pageNumber,
            PageSize: pageSize);

        var response = await mediator.Send(command, cancellationToken);

        if (response is { IsSuccess: true, Data: not null })
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

    [HttpGet("map")]
    [ProducesResponseType(typeof(Response<GetShopsInBoundsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<Response<GetShopsInBoundsResponse>> GetShopsInBounds(
        [FromQuery] [Range(-90, 90)] decimal minLat,
        [FromQuery] [Range(-180, 180)] decimal minLon,
        [FromQuery] [Range(-90, 90)] decimal maxLat,
        [FromQuery] [Range(-180, 180)] decimal maxLon)
    {
        if (minLat > maxLat || minLon > maxLon)
        {
            return Response<GetShopsInBoundsResponse>.Error("Invalid bounds: min values must be less than or equal to max values");
        }

        var request = new GetShopsInBoundsRequest(minLat, minLon, maxLat, maxLon);
        return await mediator.Send(request);
    }
}