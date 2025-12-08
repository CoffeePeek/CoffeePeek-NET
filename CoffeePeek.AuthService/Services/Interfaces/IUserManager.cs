using CoffeePeek.AuthService.Entities;

namespace CoffeePeek.AuthService.Services;

public interface IUserManager
{
    Task AddToRoleAsync(UserCredentials user, string roleName, CancellationToken ct = default);
    List<Role> GetRolesAsync(UserCredentials user);
    Task SetAuthenticationTokenAsync(UserCredentials user, string defaultProvider, string refreshTokenName, string refreshToken);
    Task<UserCredentials?> FindByIdAsync(Guid userId);
    Task<UserCredentials?> FindByEmailAsync(string requestEmail);
    Task RemoveAuthenticationTokenAsync(UserCredentials user, string defaultProvider, string refreshTokenName);
    string? GetAuthenticationTokenAsync(UserCredentials user, string defaultProvider, string refreshTokenName);
    Task<bool> CheckPasswordAsync(UserCredentials user, string requestPassword);
}