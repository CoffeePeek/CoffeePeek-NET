namespace CoffeePeek.UserService.Models;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; }

    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    
    public string? About { get; set; }
    public string? AvatarUrl { get; set; }
    
    public string Email { get; set; }
    public bool EmailConfirmed { get; set; }

    public bool IsSoftDelete { get; set; }
}