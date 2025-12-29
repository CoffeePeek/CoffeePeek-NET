using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.Account.Domain.Aggregates.UserAggregate;

public class Role : Entity<Guid>
{
    [MaxLength(255)]
    public string Name { get; private set; }
    
    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
    private Role(){}

    internal Role(string name)
    {
        Name = name;
    }
}