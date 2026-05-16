using CoffeePeek.Account.Domain.Entities.RoleAggregate;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Account.Domain.Services;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Kernel.Exceptions;

namespace CoffeePeek.Account.Application.Features.Auth.OAuthLogin;

using User = Domain.Entities.UserAggregate.User;

public class ExternalAuthService(
    IQueryUserRepository userRepository,
    IRoleRepository roleRepository) : IExternalAuthService
{
    public async Task<User> GetOrCreate(string email, string provider, string providerId,
        CancellationToken ct)
    {
        // H-7/H-8: find by (provider + providerId) first — exact match
        var userByProvider = await userRepository.GetByProvider(provider, providerId, ct);
        if (userByProvider is not null)
            return userByProvider;

        // H-9: email belongs to an existing account that was NOT created via this provider.
        // Silently linking would allow account takeover — require explicit account-linking flow instead.
        var userByEmail = await userRepository.GetByEmail(email, ct);
        if (userByEmail is not null)
            throw new UnauthorizedException(
                "An account with this email already exists. Please log in with your email and password and link the external provider from your profile settings.");

        var newUser = User.CreateExternal(email, provider, providerId);

        var role = await roleRepository.GetRoleAsync(RoleConsts.User)
                   ?? throw new ApplicationException($"Role {RoleConsts.User} not found");

        newUser.AssignRole(role);
        userRepository.Add(newUser, ct);
        return newUser;
    }
}