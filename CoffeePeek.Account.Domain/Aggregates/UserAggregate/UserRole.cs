namespace CoffeePeek.Account.Domain.Aggregates.UserAggregate;

public class UserRole : Entity<Guid>
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    public virtual UserCredential User { get; set; }
    public virtual Role Role { get; set; }
}