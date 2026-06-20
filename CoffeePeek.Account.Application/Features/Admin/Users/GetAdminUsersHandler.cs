using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Kernel.Response;
using DomainUser = CoffeePeek.Account.Domain.Entities.UserAggregate.User;

namespace CoffeePeek.Account.Application.Features.Admin.Users;

public static class GetAdminUsersHandler
{
    public static async Task<Response<GetAdminUsersResponse>> Handle(
        GetAdminUsersQuery query,
        IAdminUserQueryRepository repository,
        CancellationToken ct)
    {
        var (items, totalCount) = await repository.GetUsersAsync(
            query.Page, query.PageSize, query.Search, query.Role, ct);

        var users = items.Select(MapUser).ToArray();
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        return Response<GetAdminUsersResponse>.Success(new GetAdminUsersResponse(
            users,
            totalCount,
            totalPages,
            query.Page,
            query.PageSize));
    }

    internal static AdminUserResponse MapUser(DomainUser user) => new(
        user.Id,
        user.Username.Value,
        user.Credentials.Email.Value,
        user.CreatedAtUtc,
        user.About,
        user.PhotoMetadata != null
            ? $"https://bucket-dev-771f.up.railway.app/coffee.avatars/{user.PhotoMetadata.StorageKey}"
            : null,
        user.Statistics.ReviewCount,
        user.Statistics.CheckInCount,
        user.Statistics.AddedShopsCount,
        user.Roles.Select(r => r.Name).ToArray(),
        user.IsSoftDelete);
}
