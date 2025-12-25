using CoffeePeek.Account.Domain.Aggregates.UserAggregate;

namespace CoffeePeek.Account.Domain.Entities;

public class UserRole
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    public virtual UserCredential User { get; set; }
    public virtual Role Role { get; set; }
}