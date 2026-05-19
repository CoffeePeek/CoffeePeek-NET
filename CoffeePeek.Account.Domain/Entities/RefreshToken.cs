using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Domain.Entities;

namespace CoffeePeek.Account.Domain.Entities;

public sealed class RefreshToken : Entity<Guid>
{
    public string Token { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public bool IsRevoked { get; private set; }
    public string DeviceName { get; private set; }
    public string IpAddress { get; private set; }
    public Guid UserId { get; private set; }
    public User? User { get; private set; }

    // ReSharper disable once UnusedMember.Local
    private RefreshToken() { }

    internal RefreshToken(Guid userId, string token, TimeSpan ttl, string deviceName, string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException("Token is required");
        
        UserId = userId;
        Token = token;
        ExpiryDate = DateTime.UtcNow.Add(ttl);
        DeviceName = deviceName;
        IpAddress = ipAddress;
        IsRevoked = false;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }
    

    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiryDate;

    public static RefreshToken Create(Guid userId, string token, TimeSpan ttl, string deviceName, string ipAddress)
    {
        return new RefreshToken(userId, token, ttl, deviceName, ipAddress);
    }

    public void Revoke()
    {
        if (IsRevoked)
            return;

        IsRevoked = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}