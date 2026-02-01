using System.ComponentModel;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Requests;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Moderation.Application.Features.Shop.GenerateUploadUrl;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CoffeePeek.ModerationService.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class ModerationController(IMediator mediator) : ControllerBase
{
    [HttpPost("upload-urls")]
    [Description("Get urls for presigned upload photos")]
    [ProducesResponseType(typeof(Response<List<GenerateUploadUrlResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Generate presigned upload urls")]
    public async Task<Response<List<GenerateUploadUrlResponse>>> GenerateUploadUrls([FromBody] List<UploadUrlRequest> requests)
    {
        var command = new GenerateUploadUrlsCommand(requests);
        return await mediator.Send(command);
    }
}