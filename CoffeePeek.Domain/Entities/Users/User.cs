using System.ComponentModel.DataAnnotations;
using CoffeePeek.Domain.Entities.Auth;

namespace CoffeePeek.Domain.Entities.Users;

public class User : BaseEntity
{
    public User()
    {
        RefreshTokens = new HashSet<RefreshToken>();
        UserRoles = new HashSet<UserRole>();
        Reviews = new HashSet<Review.Review>();
    }
    
    public string? UserName { get; set; }
    [MaxLength(30)]
    public string Email { get; set; }
    [MaxLength(255)]
    public string? About { get; set; }
    public bool EmailConfirmed { get; set; }
    [MaxLength(250)]
    public string PasswordHash { get; set; }
    [MaxLength(18)]
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public bool IsSoftDeleted { get; set; }
    
    public virtual ICollection<Review.Review> Reviews { get; set; }
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; set; }
}