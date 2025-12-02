using CoffeePeek.AuthService.Models;

namespace CoffeePeek.AuthService.Entities;

public class UserCredentials
{
    public Guid Id { get; set; }

    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string? OAuthProvider { get; set; }
    public string? ProviderId { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new HashSet<RefreshToken>();
    public ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
}