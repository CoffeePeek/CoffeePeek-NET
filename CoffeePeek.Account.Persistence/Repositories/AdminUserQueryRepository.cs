using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Account.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Account.Persistence.Repositories;

public class AdminUserQueryRepository(AccountDbContext dbContext) : IAdminUserQueryRepository
{
    public async Task<(IReadOnlyList<User> Items, int TotalCount)> GetUsersAsync(
        int page,
        int pageSize,
        string? search,
        string? role,
        CancellationToken ct = default)
    {
        var query = dbContext.Users
            .AsNoTracking()
            .Include(u => u.Roles)
            .Include(u => u.PhotoMetadata)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(u =>
                u.Username.Value.ToLower().Contains(term) ||
                u.Credentials.Email.Value.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(u => u.Roles.Any(r => r.Name == role));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(u => u.CreatedAtUtc)
            .Skip((int)Math.Min((long)(page - 1) * pageSize, int.MaxValue))
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<AdminUserStats> GetStatsAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;

        var totalUsers = await dbContext.Users.CountAsync(ct);
        var blockedUsers = await dbContext.Users.CountAsync(u => u.IsSoftDelete, ct);
        var registeredToday = await dbContext.Users.CountAsync(u => u.CreatedAtUtc >= today, ct);

        var usersByRole = await dbContext.Users
            .AsNoTracking()
            .SelectMany(u => u.Roles)
            .GroupBy(r => r.Name)
            .Select(g => new { Role = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Role, x => x.Count, ct);

        return new AdminUserStats(
            TotalUsers: totalUsers,
            ActiveUsers: totalUsers - blockedUsers,
            BlockedUsers: blockedUsers,
            RegisteredToday: registeredToday,
            UsersByRole: usersByRole);
    }
}
