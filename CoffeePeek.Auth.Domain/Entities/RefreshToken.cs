using CoffeePeek.Auth.Domain.Entities;

namespace CoffeePeek.AuthService.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    public string Name { get; set; }
    public string LoginProvider { get; set; }
    public string Token { get; set; }
    public DateTime ExpiryDate { get; set; } 
    public bool IsRevoked { get; set; }
    
    public string DeviceName { get; set; }
    public string IpAddress { get; set; }
    public DateTime CreatedDate { get; set; }

    public virtual UserCredentials User { get; set; }
}