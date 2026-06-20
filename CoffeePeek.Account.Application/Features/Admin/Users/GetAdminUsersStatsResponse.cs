namespace CoffeePeek.Account.Application.Features.Admin.Users;

public record GetAdminUsersStatsResponse(
    int TotalUsers,
    int ActiveUsers,
    int BlockedUsers,
    int RegisteredToday,
    IReadOnlyDictionary<string, int> UsersByRole);
