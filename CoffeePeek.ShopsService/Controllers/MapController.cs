using System.ComponentModel.DataAnnotations;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetShopsInBounds;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.ShopsService.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class MapController(IMessageBus bus) : ControllerBase
{
    /// <summary>
    /// Get coffee shops in bounds
    /// </summary>
    /// <param name="minLat"></param>
    /// <param name="minLon"></param>
    /// <param name="maxLat"></param>
    /// <param name="maxLon"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType<Response<GetShopsInBoundsResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
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
        
        var response = await bus.InvokeAsync<Response<GetShopsInBoundsResponse>>(query);
        return Ok(response);
    }
}