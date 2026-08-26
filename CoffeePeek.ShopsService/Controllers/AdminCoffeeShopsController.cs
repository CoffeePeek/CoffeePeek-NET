using CoffeePeek.Shared.Auth;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Features.Admin.Shops;
using CoffeePeek.Shops.Application.Features.Admin.Shops.ReorderPhotos;
using CoffeePeek.Shops.Application.Features.Admin.Shops.SetShopTags;
using CoffeePeek.ShopsService.Controllers.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using DomainCoffeeShopStatus = CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate.CoffeeShopStatus;

namespace CoffeePeek.ShopsService.Controllers;

/// <summary>Admin management of published coffee shops.</summary>
[ApiController]
[Route("api/admin/shops")]
[Authorize(Policy = RoleConsts.Admin)]
[Tags("Admin")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class AdminCoffeeShopsController(IMessageBus bus, IUserContext userContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<Response<GetAdminCoffeeShopsResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShops(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] DomainCoffeeShopStatus? status = null,
        [FromQuery] bool? importedFromFile = null,
        CancellationToken ct = default)
    {
        var query = new GetAdminCoffeeShopsQuery(page, pageSize, search, status, importedFromFile);
        var response = await bus.InvokeAsync<Response<GetAdminCoffeeShopsResponse>>(query, ct);

        if (response.IsSuccess && response.Data is not null)
        {
            Response.Headers.TryAdd("X-Total-Count", response.Data.TotalItems.ToString());
            Response.Headers.TryAdd("X-Total-Pages", response.Data.TotalPages.ToString());
            Response.Headers.TryAdd("X-Current-Page", response.Data.CurrentPage.ToString());
            Response.Headers.TryAdd("X-Page-Size", response.Data.PageSize.ToString());
        }

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<Response<AdminPublishedShopDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShop(Guid id, CancellationToken ct)
    {
        var response = await bus.InvokeAsync<Response<AdminPublishedShopDto>>(
            new GetAdminCoffeeShopByIdQuery(id), ct);
        return response.IsSuccess ? Ok(response) : NotFound(response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<Response<AdminPublishedShopDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateShop(
        Guid id,
        [FromBody] UpdateAdminCoffeeShopRequest request,
        CancellationToken ct)
    {
        var command = new UpdateAdminCoffeeShopCommand(
            id, request.Name, request.Description, request.PriceRange, request.Status);
        var response = await bus.InvokeAsync<Response<AdminPublishedShopDto>>(command, ct);
        return response.IsSuccess ? Ok(response) : NotFound(response);
    }

    [HttpPatch("{id:guid}/visibility")]
    [ProducesResponseType<Response<AdminPublishedShopDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetVisibility(
        Guid id,
        [FromBody] SetCoffeeShopVisibilityRequest request,
        CancellationToken ct)
    {
        var response = await bus.InvokeAsync<Response<AdminPublishedShopDto>>(
            new SetAdminCoffeeShopVisibilityCommand(id, request.Hidden), ct);
        return response.IsSuccess ? Ok(response) : NotFound(response);
    }

    [HttpPatch("{id:guid}/owner")]
    [ProducesResponseType<Response<AdminPublishedShopDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignOwner(
        Guid id,
        [FromBody] AssignCoffeeShopOwnerRequest request,
        CancellationToken ct)
    {
        var response = await bus.InvokeAsync<Response<AdminPublishedShopDto>>(
            new AssignCoffeeShopOwnerCommand(id, request.OwnerUserId), ct);
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
        var response = await bus.InvokeAsync<Response<AdminPublishedShopDto>>(
            new ReorderAdminCoffeeShopPhotosCommand(id, request.PhotoIds), ct);

        if (response.IsSuccess)
            return Ok(response);

        return response.StatusCode switch
        {
            StatusCodes.Status404NotFound => NotFound(response),
            _ => BadRequest(response)
        };
    }

    [HttpPatch("{id:guid}/focus")]
    [ProducesResponseType<Response<AdminPublishedShopDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetFocus(
        Guid id,
        [FromBody] SetCoffeeShopFocusRequest request,
        CancellationToken ct)
    {
        var response = await bus.InvokeAsync<Response<AdminPublishedShopDto>>(
            new SetAdminCoffeeShopFocusCommand(id, request.Type, userContext.GetUserIdOrThrow()), ct);
        return response.IsSuccess ? Ok(response) : NotFound(response);
    }

    /// <summary>Replace the full set of filter tags assigned to a shop.</summary>
    [HttpPut("{shopId:guid}/tags")]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetTags(
        Guid shopId,
        [FromBody] SetCoffeeShopTagsRequest request,
        CancellationToken ct)
    {
        var command = new SetShopTagsCommand(
            shopId,
            request.TagIds ?? [],
            userContext.GetUserIdOrThrow());

        var response = await bus.InvokeAsync<Response>(command, ct);
        if (response.IsSuccess)
            return Ok(response);

        return response.StatusCode switch
        {
            StatusCodes.Status404NotFound => NotFound(response),
            _ => BadRequest(response)
        };
    }
}
