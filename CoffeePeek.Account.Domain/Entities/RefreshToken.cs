using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Account.Domain.Entities;

public sealed class RefreshToken : Entity<Guid>
{
    public string Token { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public bool IsRevoked { get; private set; }
    public string DeviceName { get; private set; }
    public string IpAddress { get; private set; }
    public DateTime CreatedDate { get; private set; }
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
        CreatedDate = DateTime.UtcNow;
        IsRevoked = false;
    }

    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiryDate;

    public void Revoke()
    {
        if (IsRevoked) return;
        IsRevoked = true;
    }
}