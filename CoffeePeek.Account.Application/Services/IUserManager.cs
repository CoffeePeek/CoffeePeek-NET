using CoffeePeek.Account.Domain.Entities;

 namespace CoffeePeek.Account.Application.Services;

public interface IUserManager
{
    Task AddToRoleAsync(UserCredential user, string roleName, CancellationToken ct = default);
    List<Role> GetRolesAsync(UserCredential user);
    Task SetAuthenticationToken(UserCredential user, string defaultProvider, string refreshTokenName, string refreshToken, string deviceName, string ipAddress);
    Task<UserCredential?> FindByIdAsync(Guid userId);
    Task<UserCredential?> FindByEmailAsync(string requestEmail);
    Task RemoveAuthenticationTokenAsync(UserCredential user, string defaultProvider, string refreshTokenName);
    Task<bool> CheckPasswordAsync(UserCredential user, string requestPassword);
    RefreshToken? GetAuthenticationToken(UserCredential user, string refreshToken);
    Task RevokeAllUserTokensAsync(Guid userId);
}