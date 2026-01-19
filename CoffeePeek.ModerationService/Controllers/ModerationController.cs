using System.ComponentModel;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Requests;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Moderation.Application.Features.Shop.GenerateUploadUrl;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.ModerationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModerationController(IMediator mediator) : ControllerBase
{
    [HttpPost("upload-urls")]
    [Description("Get urls for presigned upload photos")]
    public async Task<Response<List<GenerateUploadUrlResponse>>> GenerateUploadUrls([FromBody] List<UploadUrlRequest> requests)
    {
        var command = new GenerateUploadUrlsCommand(requests);
        return await mediator.Send(command);
    }
}
