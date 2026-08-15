using CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;
using CoffeeShop.Moderation.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Moderation.Persistence.Repositories;

public class ShopImportCandidateRepository(ModerationDbContext dbContext) : IShopImportCandidateRepository
{
    public Task<ShopImportCandidate?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        dbContext.ShopImportCandidates.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyDictionary<string, ShopImportCandidate>> GetByExternalIdsAsync(
        ImportSource source,
        IReadOnlyCollection<string> externalIds,
        CancellationToken ct = default)
    {
        if (externalIds.Count == 0)
            return new Dictionary<string, ShopImportCandidate>();

        var items = await dbContext.ShopImportCandidates
            .Where(c => c.Source == source && externalIds.Contains(c.ExternalId))
            .ToListAsync(ct);

        return items.ToDictionary(c => c.ExternalId);
    }

    public void Add(ShopImportCandidate candidate) => dbContext.ShopImportCandidates.Add(candidate);

    public async Task<(IReadOnlyList<ShopImportCandidate> Items, int TotalCount)> SearchAsync(
        ImportQueueStatus? status,
        ImportCollectorBucket? bucket,
        ImportCoffeeFocus? focus,
        ImportRejectReason? rejectReason,
        string? search,
        bool excludeStale,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = dbContext.ShopImportCandidates.AsNoTracking();

        if (status.HasValue)
            query = query.Where(c => c.QueueStatus == status.Value);

        if (bucket.HasValue)
            query = query.Where(c => c.CollectorBucket == bucket.Value);
        else if (excludeStale)
            query = query.Where(c => c.CollectorBucket != ImportCollectorBucket.Stale);

        if (focus.HasValue)
            query = query.Where(c => c.CoffeeFocus == focus.Value);

        if (rejectReason.HasValue)
            query = query.Where(c => c.RejectReason == rejectReason.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(c =>
                (c.Name != null && EF.Functions.ILike(c.Name, term)) ||
                (c.Address != null && EF.Functions.ILike(c.Address, term)) ||
                EF.Functions.ILike(c.ExternalId, term));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.CollectorBucket)
            .ThenBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<ImportCandidateStats> GetStatsAsync(CancellationToken ct = default)
    {
        var rows = await dbContext.ShopImportCandidates
            .AsNoTracking()
            .Select(c => new { c.QueueStatus, c.CoffeeFocus, c.CollectorBucket, c.RejectReason })
            .ToListAsync(ct);

        var rejected = rows.Where(r => r.QueueStatus == ImportQueueStatus.Rejected).ToList();

        return new ImportCandidateStats(
            rows.Count(r => r.QueueStatus == ImportQueueStatus.Pending),
            rows.Count(r => r.QueueStatus == ImportQueueStatus.Skipped),
            rows.Count(r => r.QueueStatus == ImportQueueStatus.Published),
            rejected.Count,
            rows.Where(r => r.CoffeeFocus.HasValue)
                .GroupBy(r => r.CoffeeFocus!.Value)
                .ToDictionary(g => g.Key, g => g.Count()),
            rows.GroupBy(r => r.CollectorBucket)
                .ToDictionary(g => g.Key, g => g.Count()),
            new ImportRejectedByReasonStats(
                rejected.Count(r => r.RejectReason == ImportRejectReason.Closed),
                rejected.Count(r => r.RejectReason == ImportRejectReason.Invalid),
                rejected.Count(r => r.RejectReason == ImportRejectReason.NotCoffee),
                rejected.Count(r => r.RejectReason is null)));
    }
}
