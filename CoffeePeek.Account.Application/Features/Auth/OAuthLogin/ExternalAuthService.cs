using CoffeePeek.Account.Domain.Aggregates.UserAggregate;
using CoffeePeek.Account.Domain.Repositories;
using CoffeePeek.Account.Domain.Services;
using CoffeePeek.Shared.Infrastructure.Constants;

namespace CoffeePeek.Account.Application.Features.Auth.OAuthLogin;

using User = Domain.Aggregates.UserAggregate.User;

public class ExternalAuthService(
    IUserRepository userRepository,
    IRoleRepository roleRepository) : IExternalAuthService
{
    public async Task<User> GetOrCreate(string email, string provider, string providerId,
        CancellationToken ct)
    {
        var user = await userRepository.GetByProvider(provider, providerId, ct)
                   ?? await userRepository.GetByEmail(email, ct);

        if (user != null)
        {
            user.UserCredential.LinkExternalProvider(provider, providerId);
            return user;
        }

        var newUser = User.CreateExternal(email, provider, providerId);

        var role = await roleRepository.GetRoleAsync(RoleConsts.User);
        newUser.AssignRole(role);

        await userRepository.Add(newUser, ct);
        return newUser;
    }
}