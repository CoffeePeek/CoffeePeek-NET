using CoffeePeek.Client.App.Core.Cache;
using CoffeePeek.Client.App.Core.Identity;

namespace CoffeePeek.Client.App.Infrastructure.Identity;

public sealed class JwtUserIdentityAccessor(IClientSession session) : IUserIdentityAccessor
{
    public Guid? GetCurrentUserIdOrNull() =>
        JwtSubParser.TryGetSubGuid(session.AccessToken, out var id) ? id : null;
}
