using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.UserService.Models;

namespace CoffeePeek.Account.Domain.Aggregates.UserAggregate;

public class User : Entity<Guid>
{
    public string Username { get; private set; }

    public string? PhoneNumber { get; private set; }
    public bool PhoneNumberConfirmed { get; private set; }
    
    public string? About { get; private set; }
    
    public string Email { get; private set; }
    public bool EmailConfirmed { get; private set; }

    public bool IsSoftDelete { get; set; }
    
    public Guid UserCredentialId { get; private set; }
    public Guid? PhotoMetadataId { get; set; }
    public PhotoMetadata PhotoMetadata { get; private set; }
    public UserCredential UserCredential { get; private set; }
    public UserStatistics UserStatistics { get; private set; }
    private User() { }

    public static User Create(string email, string username, string passwordHash)
    {
        var userId = Guid.NewGuid();
        
        var user = new User
        {
            Id = userId,
            Email = email,
            Username = username,
            UserCredential = new UserCredential(email, passwordHash, userId)
        };

        return user;
    }
    
    public static User CreateExternal(string email, string provider, string providerId)
    {
        var userId = Guid.NewGuid();
        var user = new User {
            Id = userId,
            Email = email,
            Username = email,
            UserCredential = UserCredential.CreateForOAuth(email, provider, providerId, userId)
        };
        return user;
    }
    
    public void LoginExternal(string provider, string providerId, string token, string device, string ip)
    {
        UserCredential.LinkExternalProvider(provider, providerId);
        UserCredential.AddSession(token, TimeSpan.FromDays(7), device, ip);
    }
    
    public void UpdateProfile(string? username, string? about)
    {
        if (!string.IsNullOrWhiteSpace(username))
            Username = username.Trim();

        if (about is not null)
            About = about;
    }

    public void UpdatePhoto(PhotoMetadata photoMetadata)
    {
        PhotoMetadata = photoMetadata;
    }
    
    public void UpdateEmail(string email)
    {
        if (!string.IsNullOrWhiteSpace(email) && !email.Equals(Email, StringComparison.OrdinalIgnoreCase))
        {
            Email = email.Trim();
            EmailConfirmed = false;
        }
    }
    
    public void AssignRole(Role role)
    {
        UserCredential.AssignRole(role);
    }
}