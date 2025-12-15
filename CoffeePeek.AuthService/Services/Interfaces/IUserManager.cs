﻿using CoffeePeek.AuthService.Entities;

namespace CoffeePeek.AuthService.Services;

public interface IUserManager
{
    Task AddToRoleAsync(UserCredentials user, string roleName, CancellationToken ct = default);
    List<Role> GetRolesAsync(UserCredentials user);
    Task SetAuthenticationToken(UserCredentials user, string defaultProvider, string refreshTokenName, string refreshToken, string deviceName, string ipAddress);
    Task<UserCredentials?> FindByIdAsync(Guid userId);
    Task<UserCredentials?> FindByEmailAsync(string requestEmail);
    Task RemoveAuthenticationTokenAsync(UserCredentials user, string defaultProvider, string refreshTokenName);
    Task<bool> CheckPasswordAsync(UserCredentials user, string requestPassword);
    RefreshToken? GetAuthenticationToken(UserCredentials user, string refreshToken);
    Task RevokeAllUserTokensAsync(Guid userId);
}