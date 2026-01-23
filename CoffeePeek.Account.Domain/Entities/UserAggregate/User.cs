using CoffeePeek.Account.Domain.Entities.RoleAggregate;
using CoffeePeek.Account.Domain.Events;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Account.Domain.Entities.UserAggregate;

public class User : Entity<Guid>
{
    public Username Username { get; private set; }
    public PhoneNumber? PhoneNumber { get; private set; }

    public UserCredential Credentials { get; private set; }
    public UserStatistics Statistics { get; private set; }

    public string? About { get; private set; }
    public Guid? PhotoMetadataId { get; private set; }
    public PhotoMetadata? PhotoMetadata { get; private set; }
    public bool IsSoftDelete { get; private set; }

    private readonly List<RefreshToken> _refreshTokens = [];
    public ICollection<RefreshToken> RefreshTokens => _refreshTokens;

    private readonly IList<Role> _roles = [];
    public ICollection<Role> Roles => _roles;
    
    // ReSharper disable once UnusedMember.Local
    private User() { }

    public static User Register(string invalidEmail, string invalidUsername, string passwordHash, Role defaultRole)
    {
        Email email;
        Username userName;
        try
        {
            email = Email.Create(invalidEmail);
            userName = Username.Create(invalidUsername);
        }
        catch (DomainException)
        {
            throw;
        }
        catch
        {
            throw new DomainException("Invalid email or username");
        }
        
        var userId = Guid.NewGuid();
        var token = Guid.NewGuid().ToString("N");

        var user = new User
        {
            Id = userId,
            Username = userName,
            Credentials = UserCredential.CreateBasic(email, passwordHash, token),
            Statistics = UserStatistics.Empty()
        };

        user.AssignRole(defaultRole);
        
        user.AddDomainEvent(new UserRegisteredInternalEvent(userId, email.Value, userName, token));
        
        return user;
    }

    public static User CreateExternal(string invalidEmail, string provider, string providerId)
    {
        var email = Email.Create(invalidEmail);
        var userId = Guid.NewGuid();
        return new User
        {
            Id = userId,
            Username = Username.Create(email.Value.Split('@')[0]),
            Credentials = UserCredential.CreateExternal(email, provider, providerId),
            Statistics = UserStatistics.Empty()
        };
    }

    public void AddSession(string token, TimeSpan ttl, string device, string ip)
    {
        if (_refreshTokens.Count(t => t.IsActive) >= BusinessConstants.MaxActiveSessions)
        {
            _refreshTokens.Where(t => t.IsActive).OrderBy(t => t.CreatedAtUtc).First().Revoke();
        }

        _refreshTokens.Add(new RefreshToken(Id, token, ttl, device, ip));
    }

    public void RotateRefreshToken(string oldRefreshToken, string newRefreshToken, TimeSpan ttl, string device, string ip)
    {
        var existing = _refreshTokens.FirstOrDefault(t => t.Token == oldRefreshToken);
        if (existing is not { IsActive: true })
        {
            RevokeAllSessions();
            throw new DomainException("Security breach: Token reused.");
        }

        existing.Revoke();
        AddSession(newRefreshToken, ttl, device, ip);
    }

    public void RevokeAllSessions() => _refreshTokens.ForEach(t => t.Revoke());

    public void Logout(string tokenValue) =>
        _refreshTokens.FirstOrDefault(t => t.Token == tokenValue)?.Revoke();

    public void AssignRole(Role role)
    {
        if (_roles.Any(r => r.Id == role.Id)) return;
        _roles.Add(role);
    }

    public void UpdateAbout(string about)
    {
        if (about != About)
        {
            About = about;
        }
    }

    public void SetSoftDelete()
    {
        IsSoftDelete = true;
    }

    public void UpdateAvatar(PhotoMetadata photoMetadata)
    {
        PhotoMetadata = photoMetadata;
    }
    
    public void UpdatePhoneNumber(PhoneNumber phoneNumber)
    {
        if (phoneNumber != PhoneNumber)
        {
            PhoneNumber = phoneNumber;
        }
    }
    
    public void UpdateUsername(Username userName)
    {
        if (userName != Username)
        {
            Username = userName;
        }
    }

    public void SoftDelete()
    {
        IsSoftDelete = true;
    }

    public void ConfirmEmail(string token)
    {
        if (Credentials.EmailConfirmed)
            throw new DomainException("Email already confirmed.");

        Credentials = Credentials.ConfirmEmail(token);
        AddDomainEvent(new EmailConfirmedInternalEvent());
    }
}