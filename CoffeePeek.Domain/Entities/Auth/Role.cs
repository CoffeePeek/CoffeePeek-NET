using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.Domain.Entities.Auth;

public class Role : BaseEntity
{
    [MaxLength(15)]
    public string Name { get; set; }
    
    public virtual ICollection<UserRole> UserRoles { get; set; }
}