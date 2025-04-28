using CoffeePeek.Domain.Entities.Users;

namespace CoffeePeek.Domain.Entities.Auth;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int UserId { get; set; }
    public virtual User User { get; set; }
}