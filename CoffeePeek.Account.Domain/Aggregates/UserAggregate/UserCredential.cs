using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Account.Domain.Services;
using CoffeePeek.Shared.Extensions.Exceptions;

namespace CoffeePeek.Account.Domain.Aggregates.UserAggregate;

public class UserCredential : Entity<Guid>
{
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string? OAuthProvider { get; private set; }
    public string? ProviderId { get; private set; }
    public Guid UserId { get; private set; }
    
    public virtual User? User { get; set; }

    private readonly List<RefreshToken> _refreshTokens = [];
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
    
    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
    
    private UserCredential(){}

    internal UserCredential(string email, string passwordHash, Guid userId)
    {
        Email = email;
        PasswordHash = passwordHash;
        UserId = userId;
    }
    
    public void LinkExternalProvider(string provider, string providerId)
    {
        if (!string.IsNullOrEmpty(OAuthProvider) && OAuthProvider != provider)
            throw new DomainException("User already linked to another provider");

        OAuthProvider = provider;
        ProviderId = providerId;
    }

    public void AddSession(string token, TimeSpan ttl, string device, string ip)
    {
        if (_refreshTokens.Count(t => !t.IsRevoked) >= 5)
        {
            _refreshTokens.OrderBy(t => t.CreatedDate).First().Revoke();
        }

        _refreshTokens.Add(new RefreshToken(token, ttl, device, ip));
    }
    
    public RefreshToken RotateRefreshToken(string oldTokenValue, string newTokenValue, TimeSpan ttl, string device, string ip)
    {
        var existingToken = _refreshTokens.FirstOrDefault(t => t.Token == oldTokenValue);

        if (existingToken is not { IsActive: true })
        {
            RevokeAllSessions();
            throw new DomainException("Invalid or reused refresh token. All sessions revoked for security.");
        }

        existingToken.Revoke();
        
        var newToken = new RefreshToken(newTokenValue, ttl, device, ip);
        _refreshTokens.Add(newToken);
        
        return newToken;
    }
    
    public static UserCredential CreateForOAuth(string email, string provider, string providerId, Guid userId)
    {
        return new UserCredential 
        {
            Email = email,
            OAuthProvider = provider,
            ProviderId = providerId,
            UserId = userId,
            PasswordHash = string.Empty
        };
    }
    
    public void AssignRole(Role role)
    {
        if (_userRoles.Any(ur => ur.RoleId == role.Id)) return;
        
        _userRoles.Add(new UserRole { 
            UserId = Id, 
            RoleId = role.Id, 
            Role = role 
        });
    }

    public void RevokeAllSessions()
    {
        foreach (var token in _refreshTokens.Where(t => t.IsActive))
        {
            token.Revoke();
        }
    }
    
    public bool ValidatePassword(string password, IPasswordHasherService passwordHasher)
    {
        return passwordHasher.VerifyPassword(PasswordHash, password);
    }

    public RefreshToken AddRefreshToken(string token, TimeSpan ttl, string device, string ip)
    {
        var refreshToken = new RefreshToken(token, ttl, device, ip);
        _refreshTokens.Add(refreshToken);
        return refreshToken;
    }
    
    public void Logout(string refreshTokenValue)
    {
        var token = _refreshTokens.FirstOrDefault(t => t.Token == refreshTokenValue);
        
        if (token is { IsActive: true })
        {
            token.Revoke();
        }
    }
}