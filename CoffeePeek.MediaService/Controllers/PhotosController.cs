using System.ComponentModel;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.MediaService.Requests;
using CoffeePeek.MediaService.Responses;
using CoffeePeek.MediaService.Services;
using CoffeePeek.Shared.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CoffeePeek.MediaService.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class PhotosController(IPhotoService photoService, IUserContext userContext) : ControllerBase
{
    [HttpPost("avatar")]
    [Description("Get url for presigned upload avatar photo")]
    [ProducesResponseType<Response<GenerateUploadUrlResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Generate presigned upload urls")]
    public async Task<IActionResult> GenerateUploadUrl([FromBody] UploadUrlRequest request, CancellationToken ct)
    {
        request = request with { OwnerId = userContext.GetUserIdOrThrow() };
        var response = await photoService.GenerateUserAvatarUploadUrl(request, ct);
        return Ok(response);
    }
    
    [HttpPost("shop")]
    [Description("Get urls for presigned upload photos")]
    [ProducesResponseType<Response<List<GenerateUploadUrlResponse>>>( StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Generate presigned upload urls")]
    public async Task<IActionResult> GenerateUploadUrls([FromBody] List<UploadUrlRequest> requests, CancellationToken ct)
    {
        var ownerId = userContext.GetUserIdOrThrow();
        requests = requests.Select(x => x with { OwnerId = ownerId }).ToList();
        
        var response = await photoService.GenerateShopUploadUrls(requests, ct);
        return Ok(response);
    }
}