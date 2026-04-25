using System.ComponentModel.DataAnnotations;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Domain.Entities;

namespace CoffeePeek.Account.Domain.Entities.RoleAggregate;

public class Role : Entity<Guid>
{
    [MaxLength(255)]
    public string Name { get; private set; }
    
    private readonly IList<User> _userRoles = [];
    public ICollection<User> Users => _userRoles;
    
    // ReSharper disable once UnusedMember.Local
    private Role(){}

    internal Role(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }

    public static Role Create(string name)
    {
        return new Role(name);
    }
}