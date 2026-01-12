using System.ComponentModel.DataAnnotations;
using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Account.Domain.Aggregates.UserAggregate;

public class Role : Entity<Guid>
{
    [MaxLength(255)]
    public string Name { get; private set; }
    
    private readonly List<UserRole>? _userRoles = [];
    public IReadOnlyCollection<UserRole>? UserRoles => _userRoles?.AsReadOnly();
    // ReSharper disable once UnusedMember.Local
    private Role(){}

    internal Role(string name)
    {
        Name = name;
    }
}