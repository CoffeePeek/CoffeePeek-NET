using System.ComponentModel.DataAnnotations;
using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Account.Domain.Aggregates.UserAggregate;

public sealed class RefreshToken : Entity<Guid>
{
    [MaxLength(255)]
    public string Token { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public bool IsRevoked { get; private set; }
    [MaxLength(50)]
    public string DeviceName { get; private set; }
    [MaxLength(50)]
    public string IpAddress { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public Guid UserCredentialId { get; private set; }
    public UserCredential UserCredential { get; private set; }

    // ReSharper disable once UnusedMember.Local
    private RefreshToken() { }

    internal RefreshToken(string token, TimeSpan ttl, string deviceName, string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException("Token is required");
        
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