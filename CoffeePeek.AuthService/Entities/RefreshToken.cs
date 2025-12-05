using CoffeePeek.AuthService.Models;

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

    public virtual UserCredentials User { get; set; }
}