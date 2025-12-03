using CoffeePeek.Contract.Requests.Internal;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Internal;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InternalController(IMediator mediator) : Controller
{
    [HttpGet("cities")]
    public Task<Response<GetCitiesResponse>> GetCities()
    {
        var request = new GetCitiesRequest();
        
        return mediator.Send(request);
    }
}