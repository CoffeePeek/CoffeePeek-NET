using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Account.Domain.Aggregates.UserAggregate;

public class UserRole : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }

    public UserCredential? User { get; private set; }
    public Role? Role { get; private set; }
    
    // ReSharper disable once UnusedMember.Local
    private UserRole(){}
    
    internal UserRole(Guid userId, Guid roleId)
    {
        Id = Guid.NewGuid();
        
        UserId = userId;
        RoleId = roleId;
    }
}