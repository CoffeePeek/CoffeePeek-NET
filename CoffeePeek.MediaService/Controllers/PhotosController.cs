using System.ComponentModel;
using CoffeePeek.MediaService.Requests;
using CoffeePeek.MediaService.Responses;
using CoffeePeek.Shared.Auth;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.MediaService.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class PhotosController(IMessageBus bus, IUserContext userContext) : ControllerBase
{
    /// <summary>
    /// Generate presigned upload url for avatar photo
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpPost("avatar")]
    [Description("Get url for presigned upload avatar photo")]
    [ProducesResponseType<Response<GenerateUploadUrlResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateUploadUrl([FromBody] UploadUrlRequest request, CancellationToken ct)
    {
        request = request with { OwnerId = userContext.GetUserIdOrThrow() };
        var response = await bus.InvokeAsync<Response<GenerateUploadUrlResponse>>(request, ct);
        return Ok(response);
    }
    
    /// <summary>
    /// Generate presigned upload urls for shop photos
    /// </summary>
    /// <param name="requests"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpPost("shop")]
    [Description("Get urls for presigned upload photos")]
    [ProducesResponseType<Response<List<GenerateUploadUrlResponse>>>( StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateUploadUrls([FromBody] List<UploadUrlRequest> requests, CancellationToken ct)
    {
        var ownerId = userContext.GetUserIdOrThrow();
        requests = requests.Select(x => x with { OwnerId = ownerId }).ToList();
        
        var response = await bus.InvokeAsync<Response<List<GenerateUploadUrlResponse>>>(requests, ct);
        return Ok(response);
    }
}