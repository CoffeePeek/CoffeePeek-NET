using CoffeePeek.Client.App.Core.Cache;
using CoffeePeek.Client.App.Core.Identity;

namespace CoffeePeek.Client.App.Infrastructure.Identity;

public sealed class JwtUserRoleAccessor(IClientSession session) : IUserRoleAccessor
{
    public IReadOnlyList<string> GetRoles() => JwtRoleParser.ReadRoles(session.AccessToken);

    public bool IsInRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return false;

        foreach (var r in GetRoles())
        {
            if (string.Equals(r, role, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
