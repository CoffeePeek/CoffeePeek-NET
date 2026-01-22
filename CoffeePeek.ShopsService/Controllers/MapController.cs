using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetShopsInBounds;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CoffeePeek.ShopsService.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class MapController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<Response<GetShopsInBoundsResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [SwaggerOperation("Get coffee shops in bounds")]
    public async Task<IActionResult> GetShopsInBounds(
        [FromQuery] [Range(-90, 90)] decimal minLat,
        [FromQuery] [Range(-180, 180)] decimal minLon,
        [FromQuery] [Range(-90, 90)] decimal maxLat,
        [FromQuery] [Range(-180, 180)] decimal maxLon)
    {
        if (minLat > maxLat || minLon > maxLon)
        {
            return BadRequest(Response<GetShopsInBoundsResponse>.Error("Invalid bounds: min values must be less than or equal to max values"));
        }

        var query = new GetShopsInBoundsQuery(minLat, minLon, maxLat, maxLon);
        
        var response = await mediator.Send(query);
        return Ok(response);
    }
}