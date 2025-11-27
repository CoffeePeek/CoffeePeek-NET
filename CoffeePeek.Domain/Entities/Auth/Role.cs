using Microsoft.AspNetCore.Identity;

namespace CoffeePeek.Domain.Entities.Auth;

public sealed class Role : IdentityRole<int>
{
    public ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
}