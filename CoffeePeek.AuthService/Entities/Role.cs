using CoffeePeek.AuthService.Models;

namespace CoffeePeek.AuthService.Entities;

public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    
    public virtual ICollection<UserRole> UserRoles { get; set; }
}