namespace CoffeePeek.AuthService.Entities;

public class UserRole
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    public virtual UserCredentials User { get; set; }
    public virtual Role Role { get; set; }
}