﻿using CoffeePeek.AuthService.Entities;
using CoffeePeek.AuthService.Repositories;
using CoffeePeek.Shared.Infrastructure.Options;
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
                Role = role,
                User = user
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

    public async Task SetAuthenticationToken(UserCredentials user, string defaultProvider, string refreshTokenName,
        string token, string deviceName, string ipAddress)
    {
        // Задел на будущее: Ограничение количества активных устройств
        const int maxActiveDevices = 5; // Этот параметр можно вынести в конфигурацию
        var activeTokens = user.RefreshTokens.Where(rt => !rt.IsRevoked).ToList();

        if (activeTokens.Count >= maxActiveDevices)
        {
            // Аннулируем самый старый токен
            var oldestToken = activeTokens.OrderBy(rt => rt.CreatedDate).First();
            oldestToken.IsRevoked = true;
        }
        
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Name = refreshTokenName,
            LoginProvider = defaultProvider,
            Token = token,
            ExpiryDate = DateTime.UtcNow.AddDays(_options.RefreshTokenLifetimeDays),
            IsRevoked = false,
            DeviceName = deviceName,
            IpAddress = ipAddress,
            CreatedDate = DateTime.UtcNow
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

    public RefreshToken? GetAuthenticationToken(UserCredentials user, string tokenValue)
    {
        return user.RefreshTokens.FirstOrDefault(rt => rt.Token == tokenValue);
    }

    public Task RevokeAllUserTokensAsync(Guid userId)
    {
        var user = userRepository.GetByIdAsync(userId).Result;
        if (user == null) return Task.CompletedTask;

        foreach (var token in user.RefreshTokens)
        {
            token.IsRevoked = true;
        }
        return userRepository.UpdateAsync(user);
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