using CoffeePeek.Account.Domain.Services;
using CoffeePeek.Shared.Extensions.Exceptions;

namespace CoffeePeek.Account.Domain.Entities.UserAggregate;

public partial record UserCredential
{
    public Email Email { get; private set; }
    public string PasswordHash { get; init; }
    public bool EmailConfirmed { get; private set; }
    public string? OAuthProvider { get; private set; }
    public string? ProviderId { get; private set; }
    public string? EmailConfirmationToken { get; init; }
    public DateTime? EmailConfirmationExpiresAt { get; init; }

    private UserCredential()
    {
    }

    public static UserCredential CreateBasic(Email email, string passwordHash, string token) => new()
    {
        Email = email,
        PasswordHash = passwordHash,
        EmailConfirmationToken = token,
        EmailConfirmationExpiresAt = DateTime.UtcNow.AddMinutes(10)
    };

    public static UserCredential CreateExternal(Email email, string provider, string providerId) => new()
    {
        Email = email,
        OAuthProvider = provider,
        ProviderId = providerId,
        EmailConfirmed = true,
        PasswordHash = string.Empty
    };

    public UserCredential ConfirmEmail(string token)
    {
        if (token != EmailConfirmationToken) throw new DomainException("Invalid token.");
        if (DateTime.UtcNow > EmailConfirmationExpiresAt) throw new DomainException("Token expired.");

        return this with
        {
            EmailConfirmed = true,
            EmailConfirmationToken = null,
            EmailConfirmationExpiresAt = null
        };
    }
    
    public bool ValidatePassword(string password, IPasswordHasherService passwordHasher)
    {
        return passwordHasher.VerifyPassword(PasswordHash, password);;
    }

    public void LinkExternalProvider(string provider, string providerId)
    {
        if (!string.IsNullOrEmpty(OAuthProvider) && OAuthProvider != provider)
            throw new DomainException("User already linked to another provider");

        OAuthProvider = provider;
        ProviderId = providerId;
    }

    public void UpdateEmail(string invalidEmail)
    {
        Email = Email.Create(invalidEmail);
        EmailConfirmed = false;
    }
}