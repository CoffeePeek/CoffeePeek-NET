using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetShopsInBounds;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.ShopsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MapController(IMediator mediator) : Controller
{
    [HttpGet()]
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