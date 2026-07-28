using CoffeePeek.Shared.Auth;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Features.Admin.Shops;
using CoffeePeek.Shops.Application.Features.Owner;
using CoffeePeek.Shops.Application.Features.Owner.ReorderPhotos;
using CoffeePeek.ShopsService.Controllers.Admin;
using CoffeePeek.ShopsService.Controllers.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.ShopsService.Controllers;

/// <summary>Owner portal for managing assigned coffee shops.</summary>
[ApiController]
[Route("api/owner/coffee-shops")]
[Authorize(Policy = RoleConsts.Owner)]
[Tags("Owner")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class OwnerCoffeeShopsController(IMessageBus bus, IUserContext userContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<Response<GetOwnerCoffeeShopsResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyShops(CancellationToken ct)
    {
        var ownerId = userContext.GetUserIdOrThrow();
        var response = await bus.InvokeAsync<Response<GetOwnerCoffeeShopsResponse>>(
            new GetOwnerCoffeeShopsQuery(ownerId), ct);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<Response<AdminPublishedShopDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyShop(Guid id, CancellationToken ct)
    {
        var ownerId = userContext.GetUserIdOrThrow();
        var response = await bus.InvokeAsync<Response<AdminPublishedShopDto>>(
            new GetOwnerCoffeeShopByIdQuery(id, ownerId), ct);
        return response.IsSuccess ? Ok(response) : NotFound(response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<Response<AdminPublishedShopDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMyShop(
        Guid id,
        [FromBody] UpdateOwnerCoffeeShopRequest request,
        CancellationToken ct)
    {
        var ownerId = userContext.GetUserIdOrThrow();
        var command = new UpdateOwnerCoffeeShopCommand(
            id,
            ownerId,
            request.Name,
            request.Description,
            request.PhoneNumber,
            request.Email,
            request.SiteLink,
            request.InstagramLink);

        var response = await bus.InvokeAsync<Response<AdminPublishedShopDto>>(command, ct);
        return response.IsSuccess ? Ok(response) : NotFound(response);
    }

    /// <summary>Reorder gallery photos. Body must list every shop photo ID in the new display order (first = cover).</summary>
    [HttpPut("{id:guid}/photos/order")]
    [ProducesResponseType<Response<AdminPublishedShopDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderPhotos(
        Guid id,
        [FromBody] ReorderCoffeeShopPhotosRequest request,
        CancellationToken ct)
    {
        var ownerId = userContext.GetUserIdOrThrow();
        var response = await bus.InvokeAsync<Response<AdminPublishedShopDto>>(
            new ReorderOwnerCoffeeShopPhotosCommand(id, ownerId, request.PhotoIds), ct);

        if (response.IsSuccess)
            return Ok(response);

        return response.StatusCode switch
        {
            StatusCodes.Status404NotFound => NotFound(response),
            _ => BadRequest(response)
        };
    }
}
