using CoffeePeek.Moderation.Domain.Entities;

namespace CoffeePeek.Moderation.Domain.Aggregates;

public interface IModerationAuditLogRepository
{
    Task AddAsync(ModerationAuditLog entry, CancellationToken ct = default);

    Task<(IReadOnlyList<ModerationAuditLog> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        ModerationAuditEntityType? entityType,
        ModerationAuditAction? action,
        CancellationToken ct = default);
}
