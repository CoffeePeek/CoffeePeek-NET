using CoffeePeek.AuthService.Configuration;
using CoffeePeek.AuthService.Entities;
using CoffeePeek.AuthService.Models;
using CoffeePeek.AuthService.Repositories;
using Microsoft.Extensions.Options;

namespace CoffeePeek.AuthService.Services;

public class UserManager(
    IUserCredentialsRepository userRepository,
    IRoleRepository roleRepository,
    IOptions<JWTOptions> options,
    IPasswordHasherService passwordHasherService) : IUserManager
{
    private readonly JWTOptions _options = options.Value;

    public async Task AddToRoleAsync(UserCredentials user, string roleName, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(roleName))
        {
            throw new ArgumentException("Role name cannot be empty.", nameof(roleName));
        }

        var role = await roleRepository.GetRoleAsync(roleName);
        if (role == null)
        {
            throw new InvalidOperationException($"Role '{roleName}' does not exist.");
        }

        if (user.UserRoles.All(ur => ur.RoleId != role.Id))
        {
            user.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id,
                Role = role
            });

            await userRepository.UpdateAsync(user, ct);
        }
    }

    public List<Role> GetRolesAsync(UserCredentials user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var roles = user.UserRoles?.Select(ur => ur.Role!).ToList() ?? [];
        return roles;
    }

    public async Task SetAuthenticationTokenAsync(UserCredentials user, string defaultProvider, string refreshTokenName,
        string token)
    {
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Name = refreshTokenName,
            LoginProvider = defaultProvider,
            Token = token,
            ExpiryDate = DateTime.Now.AddDays(_options.RefreshTokenLifetimeDays),
            IsRevoked = false,
        };
        user.RefreshTokens.Add(refreshToken);

        await userRepository.UpdateAsync(user);
    }

    public Task<UserCredentials?> FindByIdAsync(Guid userId)
    {
        return userRepository.GetByIdAsync(userId);
    }

    public Task<UserCredentials?> FindByEmailAsync(string requestEmail)
    {
        return userRepository.GetByEmailAsync(requestEmail);
    }

    public Task RemoveAuthenticationTokenAsync(UserCredentials user, string defaultProvider, string refreshTokenName)
    {
        var token = user.RefreshTokens.FirstOrDefault(rt =>
            rt.Name == refreshTokenName && rt.LoginProvider == defaultProvider);

        if (token != null)
        {
            user.RefreshTokens.Remove(token);
        }

        return userRepository.UpdateAsync(user);
    }

    public string? GetAuthenticationTokenAsync(UserCredentials user, string defaultProvider, string refreshTokenName)
    {
        var token = user.RefreshTokens.FirstOrDefault(rt =>
            rt.Name == refreshTokenName && rt.LoginProvider == defaultProvider);
        return token?.Token;
    }

    public Task<bool> CheckPasswordAsync(UserCredentials user, string requestPassword)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestPassword);

        var hashedPassword = user.PasswordHash;

        if (string.IsNullOrEmpty(hashedPassword))
        {
            return Task.FromResult(false);
        }

        var isPasswordValid = passwordHasherService.VerifyPassword(hashedPassword, requestPassword);

        return Task.FromResult(isPasswordValid);
    }
}