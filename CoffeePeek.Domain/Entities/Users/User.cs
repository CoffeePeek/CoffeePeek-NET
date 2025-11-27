using System.ComponentModel.DataAnnotations;
using CoffeePeek.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;

namespace CoffeePeek.Domain.Entities.Users;

public class User : IdentityUser<int>
{
    public User()
    {
        UserRoles = new HashSet<UserRole>();
        Reviews = new HashSet<Review.Review>();
    }
    
    [MaxLength(255)]
    public string? About { get; set; }
    public bool IsSoftDeleted { get; set; }
    
    public virtual ICollection<Review.Review> Reviews { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; set; }
}