using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CoffeePeek.Domain.Entities.Users;

public class User : IdentityUser<int>
{
    public User()
    {
        Reviews = new HashSet<Review.Review>();
    }

    [MaxLength(255)]
    public string GoogleId { get; set; }
    [MaxLength(255)]
    public string AvatarUrl { get; set; }
    [MaxLength(255)]
    public string? About { get; set; }
    public bool IsSoftDeleted { get; set; }
    
    public virtual ICollection<Review.Review> Reviews { get; set; }
}