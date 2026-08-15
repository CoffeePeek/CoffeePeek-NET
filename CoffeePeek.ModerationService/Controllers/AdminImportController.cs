using System.Text.Json;
using CoffeePeek.Contract.Dtos.Import;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Moderation.Application.Features.Import.ApplyImportDecisions;
using CoffeePeek.Moderation.Application.Features.Import.DecideImportCandidate;
using CoffeePeek.Moderation.Application.Features.Import.GetImportCandidateById;
using CoffeePeek.Moderation.Application.Features.Import.GetImportCandidates;
using CoffeePeek.Moderation.Application.Features.Import.GetImportStats;
using CoffeePeek.Moderation.Application.Features.Import.RefreshGoogleStatus;
using CoffeePeek.Moderation.Application.Features.Import.RefreshOsmImport;
using CoffeePeek.Shared.Auth;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.ModerationService.Controllers;

[ApiController]
[Route("api/admin/import")]
[Authorize(Policy = RoleConsts.Moderator)]
[Tags("Admin Import")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class AdminImportController(IMessageBus bus, IUserContext userContext) : ControllerBase
{
    [HttpPost("osm/refresh")]
    [ProducesResponseType<Response<OsmRefreshResultDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshOsm(CancellationToken ct)
    {
        var response = await bus.InvokeAsync<Response<OsmRefreshResultDto>>(new RefreshOsmImportCommand(), ct);
        return Ok(response);
    }

    [HttpPost("decisions")]
    [ProducesResponseType<Response<ApplyImportDecisionsResultDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ApplyDecisions(
        [FromBody] JsonElement body,
        [FromQuery] bool overrideClosed = false,
        CancellationToken ct = default)
    {
        Dictionary<string, string>? decisions = null;
        if (body.ValueKind == JsonValueKind.Object &&
            body.TryGetProperty("decisions", out var nested) &&
            nested.ValueKind == JsonValueKind.Object)
        {
            decisions = nested.EnumerateObject()
                .Where(p => p.Value.ValueKind == JsonValueKind.String)
                .ToDictionary(p => p.Name, p => p.Value.GetString() ?? "");
        }

        var command = new ApplyImportDecisionsCommand(
            decisions,
            body,
            overrideClosed,
            userContext.GetUserIdOrThrow());

        var response = await bus.InvokeAsync<Response<ApplyImportDecisionsResultDto>>(command, ct);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("candidates")]
    [ProducesResponseType<Response<GetImportCandidatesResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCandidates(
        [FromQuery] ImportQueueStatus? status = ImportQueueStatus.Pending,
        [FromQuery] ImportCollectorBucket? bucket = null,
        [FromQuery] CoffeeFocus? focus = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var response = await bus.InvokeAsync<Response<GetImportCandidatesResponse>>(
            new GetImportCandidatesQuery(status, bucket, focus, search, page, pageSize), ct);

        if (response.IsSuccess && response.Data is not null)
        {
            Response.Headers.TryAdd("X-Total-Count", response.Data.TotalItems.ToString());
            Response.Headers.TryAdd("X-Total-Pages", response.Data.TotalPages.ToString());
            Response.Headers.TryAdd("X-Current-Page", response.Data.CurrentPage.ToString());
            Response.Headers.TryAdd("X-Page-Size", response.Data.PageSize.ToString());
        }

        return Ok(response);
    }

    [HttpGet("candidates/{id:guid}")]
    [ProducesResponseType<Response<ShopImportCandidateDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCandidate(Guid id, CancellationToken ct)
    {
        var response = await bus.InvokeAsync<Response<ShopImportCandidateDto>>(
            new GetImportCandidateByIdQuery(id), ct);
        return response.IsSuccess ? Ok(response) : NotFound(response);
    }

    [HttpPost("candidates/{id:guid}/google-refresh")]
    [ProducesResponseType<Response<ShopImportCandidateDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshGoogle(
        Guid id,
        [FromQuery] bool force = false,
        CancellationToken ct = default)
    {
        var response = await bus.InvokeAsync<Response<ShopImportCandidateDto>>(
            new RefreshGoogleStatusCommand(id, force), ct);

        if (response.IsSuccess)
            return Ok(response);

        return response.StatusCode switch
        {
            StatusCodes.Status404NotFound => NotFound(response),
            StatusCodes.Status400BadRequest => BadRequest(response),
            _ => StatusCode(response.StatusCode ?? StatusCodes.Status502BadGateway, response)
        };
    }

    [HttpPost("candidates/{id:guid}/decide")]
    [ProducesResponseType<Response<ShopImportCandidateDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Decide(
        Guid id,
        [FromBody] DecideImportCandidateRequest request,
        CancellationToken ct)
    {
        var command = new DecideImportCandidateCommand(
            id,
            request.Status,
            request.CoffeeFocus,
            request.TagSlugs,
            request.OverrideClosed,
            userContext.GetUserIdOrThrow());

        var response = await bus.InvokeAsync<Response<ShopImportCandidateDto>>(command, ct);
        if (response.IsSuccess)
            return Ok(response);

        return response.StatusCode switch
        {
            StatusCodes.Status404NotFound => NotFound(response),
            _ => BadRequest(response)
        };
    }

    [HttpGet("stats")]
    [ProducesResponseType<Response<ImportStatsDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var response = await bus.InvokeAsync<Response<ImportStatsDto>>(new GetImportStatsQuery(), ct);
        return Ok(response);
    }
}

public record DecideImportCandidateRequest(
    ImportQueueStatus Status,
    CoffeeFocus? CoffeeFocus,
    string[]? TagSlugs,
    bool OverrideClosed = false);
