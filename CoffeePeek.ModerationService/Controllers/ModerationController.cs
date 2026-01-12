using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Requests;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Contract.Responses.CoffeeShop.Review;
using Coffeepeek.Moderation.Application.Features.CreateShop;
using CoffeePeek.Moderation.Application.Features.GenerateUploadUrl;
using Coffeepeek.Moderation.Application.Features.GetAllModerationShops;
using Coffeepeek.Moderation.Application.Features.Shop.UpdateModerationShopStatus;
using Coffeepeek.Moderation.Application.Features.UpdateModerationShopStatus;
using Coffeepeek.Moderation.Application.UpdateShop;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shared.Infrastructure.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.ModerationService.Controllers;

[ApiController]
[Authorize(Policy = RoleConsts.Admin)]
[Route("api/[controller]")]
public class ModerationController(IMediator mediator) : Controller
{
    [HttpPost("upload-urls")]
    [Description("Get urls for presigned upload photos")]
    public async Task<Response<List<GenerateUploadUrlResponse>>> GenerateUploadUrls([FromBody] List<UploadUrlRequest> requests)
    {
        var command = new GenerateUploadUrlsCommand(requests);
        return await mediator.Send(command);
    }
}
