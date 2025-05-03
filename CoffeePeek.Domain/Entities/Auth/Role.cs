namespace CoffeePeek.Domain.Entities.Auth;

public class Role : BaseEntity
{
    public string Name { get; set; }
    
    public virtual ICollection<UserRole> UserRoles { get; set; }
}