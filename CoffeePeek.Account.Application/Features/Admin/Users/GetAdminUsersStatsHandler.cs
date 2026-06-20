using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Account.Application.Features.Admin.Users;

public static class GetAdminUsersStatsHandler
{
    public static async Task<Response<GetAdminUsersStatsResponse>> Handle(
        GetAdminUsersStatsQuery _,
        IAdminUserQueryRepository repository,
        CancellationToken ct)
    {
        var stats = await repository.GetStatsAsync(ct);

        return Response<GetAdminUsersStatsResponse>.Success(new GetAdminUsersStatsResponse(
            stats.TotalUsers,
            stats.ActiveUsers,
            stats.BlockedUsers,
            stats.RegisteredToday,
            stats.UsersByRole));
    }
}
