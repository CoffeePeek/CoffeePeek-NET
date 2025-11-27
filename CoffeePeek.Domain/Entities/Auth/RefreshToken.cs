using System.ComponentModel.DataAnnotations;
using CoffeePeek.Domain.Entities.Users;

namespace CoffeePeek.Domain.Entities.Auth;

public class RefreshToken : BaseEntity
{
    [MaxLength(70)]
    public string Token { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int UserId { get; set; }
    
    public bool IsActive => !IsRevoked && ExpiryDate > DateTime.UtcNow;
    
    public virtual User User { get; set; }
}