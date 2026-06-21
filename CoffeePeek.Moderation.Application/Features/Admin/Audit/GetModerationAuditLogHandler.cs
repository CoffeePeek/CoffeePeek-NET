using System.Text.Json.Serialization;
using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Moderation.Application.Features.Admin.Audit;

public record ModerationAuditLogDto(
    Guid Id,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] ModerationAuditEntityType EntityType,
    Guid EntityId,
    string EntityName,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] ModerationAuditAction Action,
    Guid ModeratorUserId,
    string? Comment,
    DateTime CreatedAtUtc);

public record GetModerationAuditLogResponse(
    IReadOnlyList<ModerationAuditLogDto> Items,
    int TotalItems,
    int TotalPages,
    int CurrentPage,
    int PageSize);

public record GetModerationAuditLogQuery(
    int Page = 1,
    int PageSize = 20,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] ModerationAuditEntityType? EntityType = null,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] ModerationAuditAction? Action = null);

public static class GetModerationAuditLogHandler
{
    public static async Task<Response<GetModerationAuditLogResponse>> Handle(
        GetModerationAuditLogQuery query,
        IModerationAuditLogRepository repository,
        CancellationToken ct)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var (items, totalCount) = await repository.GetPagedAsync(
            page, pageSize, query.EntityType, query.Action, ct);

        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var dtos = items.Select(x => new ModerationAuditLogDto(
            x.Id,
            x.EntityType,
            x.EntityId,
            x.EntityName,
            x.Action,
            x.ModeratorUserId,
            x.Comment,
            x.CreatedAtUtc)).ToList();

        return Response<GetModerationAuditLogResponse>.Success(new GetModerationAuditLogResponse(
            dtos, totalCount, totalPages, page, pageSize));
    }
}

public static class ModerationAuditWriter
{
    public static async Task WriteAsync(
        IModerationAuditLogRepository repository,
        ModerationAuditEntityType entityType,
        Guid entityId,
        string entityName,
        ModerationAuditAction action,
        Guid moderatorUserId,
        string? comment,
        CancellationToken ct)
    {
        var entry = ModerationAuditLog.Create(
            entityType, entityId, entityName, action, moderatorUserId, comment);
        await repository.AddAsync(entry, ct);
    }
}
