using System.Net;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Account.Application.Features.Admin.Users.Sessions;

public record GetAdminUserSessionsQuery(Guid UserId);

public record GetAdminUserSessionsResponse(AdminUserSessionResponse[] Sessions);

public static class GetAdminUserSessionsHandler
{
    public static async Task<Response<GetAdminUserSessionsResponse>> Handle(
        GetAdminUserSessionsQuery query,
        IUserRepository userRepository,
        CancellationToken ct)
    {
        var user = await userRepository.GetById(query.UserId, ct);
        if (user is null)
            return Response<GetAdminUserSessionsResponse>.Error(HttpStatusCode.NotFound, "User not found.");

        var sessions = user.RefreshTokens
            .OrderByDescending(t => t.CreatedAtUtc)
            .Select(t => new AdminUserSessionResponse(
                t.Id,
                t.DeviceName,
                t.IpAddress,
                t.ExpiryDate,
                t.IsRevoked,
                t.CreatedAtUtc))
            .ToArray();

        return Response<GetAdminUserSessionsResponse>.Success(new GetAdminUserSessionsResponse(sessions));
    }
}
