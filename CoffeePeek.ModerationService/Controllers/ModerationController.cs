using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Contract.Responses.CoffeeShop.Review;
using Coffeepeek.Moderation.Application.Features.CreateShop;
using CoffeePeek.Moderation.Application.Features.GenerateUploadUrl;
using Coffeepeek.Moderation.Application.Features.GetAllModerationShops;
using Coffeepeek.Moderation.Application.Features.UpdateModerationShopStatus;
using Coffeepeek.Moderation.Application.UpdateShop;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shared.Infrastructure.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.ModerationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModerationController(IMediator mediator) : Controller
{
    [HttpGet]
    [Authorize(Policy = RoleConsts.Admin)]
    [Description("Get all coffee shop reviews for moderation")]
    public async Task<Response<GetCoffeeShopsInModerationByIdResponse>> GetAllModerationShops()
    {
        var request = new GetAllModerationShopsCommand();
        return await mediator.Send(request);
    }
    
    [HttpPost("upload-urls")]
    [Authorize]
    [Description("Get urls for presigned upload photos")]
    public async Task<Response<List<GenerateUploadUrlResponse>>> GenerateUploadUrls([FromBody] List<UploadUrlRequest> requests)
    {
        var command = new GenerateUploadUrlsCommand(requests);
        return await mediator.Send(command);
    }
    
    [HttpPost]
    [Authorize]
    [Description("Adds a new coffee shop to moderation")]
    public async Task<Response<SendCoffeeShopToModerationResponse>> SendCoffeeShopToModeration(
        [FromBody] SendCoffeeShopToModerationCommand command)
    {
        var userId = User.GetUserIdOrThrow();
        command.UserId = userId;

        return await mediator.Send(command);
    }

    [HttpPut]
    [Authorize]
    [Description("Updates a coffee shop to moderation")]
    public async Task<UpdateEntityResponse<ModerationShopDto>> UpdateModerationCoffeeShop(
        [FromForm] ModerationShopDto dto)
    {
        var userId = User.GetUserIdOrThrow();
        var command = new UpdateModerationCoffeeShopCommand(dto, userId);

        return await mediator.Send(command);
    }

    [HttpPut("status")]
    [Authorize(Policy = RoleConsts.Admin)]
    [Description("Updates a review coffee shop status")]
    public async Task<Response> UpdateModerationCoffeeShopStatus(
        [FromQuery, Required] Guid id, 
        [FromQuery, Required] ModerationStatus status)
    {
        var userId = User.GetUserIdOrThrow();
        
        var request = new UpdateModerationCoffeeShopStatusCommand(id, status, userId);
        
        return await mediator.Send(request);
    }
}
