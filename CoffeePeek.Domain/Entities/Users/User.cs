namespace CoffeePeek.Domain.Entities.Users;

public class User : BaseEntity
{
    public string? UserName { get; set; }
    public string Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public string PasswordHash { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public bool IsSoftDeleted { get; set; }
}