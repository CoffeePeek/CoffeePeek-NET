using System.ComponentModel.DataAnnotations;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Infrastructure.Abstract;

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
        Name = name;
    }
}