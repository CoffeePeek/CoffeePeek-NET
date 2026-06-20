namespace CoffeePeek.Account.Domain.Entities.UserAggregate;

public interface IAdminUserQueryRepository
{
    Task<(IReadOnlyList<User> Items, int TotalCount)> GetUsersAsync(
        int page,
        int pageSize,
        string? search,
        string? role,
        CancellationToken ct = default);

    Task<AdminUserStats> GetStatsAsync(CancellationToken ct = default);
}

public record AdminUserStats(
    int TotalUsers,
    int ActiveUsers,
    int BlockedUsers,
    int RegisteredToday,
    IReadOnlyDictionary<string, int> UsersByRole);
