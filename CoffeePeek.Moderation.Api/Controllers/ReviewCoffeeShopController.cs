using CoffeePeek.Moderation.Application.Dtos;
using CoffeePeek.Moderation.Application.Requests;
using CoffeePeek.Moderation.Application.Responses;
using CoffeePeek.Moderation.Contract.Abstract;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.Moderation.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ReviewCoffeeShopController(IMediator mediator) : Controller
{
    [HttpGet("feature-get-all")]
    public Task<Response<GetReviewCoffeeShopsResponse>> GetAll(GetReviewCoffeeShopsRequest request)
    {
        return mediator.Send(request);
    }
    
    [HttpPost("feature-create")]
    public Task<Response<CreateReviewCoffeeShopResponse>> Create(CreateReviewCoffeeShopRequest request)
    {
        return mediator.Send(request);
    }
    
    [HttpPatch("{id}")]
    public Task<Response> Update(int id, [FromBody] JsonPatchDocument<ReviewShopPatchDto> patchDoc)
    {
        return default;
    }
    
}